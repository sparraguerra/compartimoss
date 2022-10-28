using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace CustomConfigurationProviders.CosmosDb;

public class CosmosDbConfigurationProvider : ConfigurationProvider
{
    private CosmosClient cosmosClient;
    private const string instanceName = "host";
    private const string processorName = "changeFeedSample";
    private const string leaseContainerName = "leases";
    ChangeFeedProcessor? processor = null;
    private readonly CosmosDbConfigurationSource? source; 

    public CosmosDbConfigurationProvider(CosmosDbConfigurationSource source)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));

        if(string.IsNullOrWhiteSpace(source.DatabaseName))
        {
            throw new ArgumentException("DatabaseName");
        }
        if (string.IsNullOrWhiteSpace(source.ContainerName))
        {
            throw new ArgumentException("ContainerName");
        }
        
        cosmosClient = !string.IsNullOrWhiteSpace(source?.ConnectionString) ?
                                new CosmosClient(source?.ConnectionString) :
                                new CosmosClient(source?.Endpoint, source?.AuthKey);

        if (source?.ChangeFeed == true)
        {
            processor = StartChangeFeedProcessorAsync(source.DatabaseName, leaseContainerName, source.ContainerName).GetAwaiter().GetResult(); ;
        }
    }

    public override void Load()
    {        
        var container = cosmosClient.GetContainer(source?.DatabaseName, source?.ContainerName);

        var queryOptions = new QueryRequestOptions { MaxItemCount = -1 };
        QueryDefinition query = new($"SELECT * FROM {source?.ContainerName} c");

        using var resultSetIterator = 
                    container.GetItemQueryIterator<JObject>(query, requestOptions: new QueryRequestOptions { MaxConcurrency = 1 });

        while (resultSetIterator.HasMoreResults)
        {
            var response = Task.Run(async () => await resultSetIterator.ReadNextAsync()).Result;

            foreach (var result in response)
            { 
                var allConfiguration = ParseProperties(result);
                foreach (var configurationItem in allConfiguration)
                {
                    var key = !string.IsNullOrWhiteSpace(source?.Prefix) ?
                              $"{source?.Prefix}:{configurationItem.Key}" :
                              configurationItem.Key;
                    Data[key] =  configurationItem.Value; 
                }
            }
        }
    }

    private Dictionary<string, string> ParseProperties(JObject? result)
    {
        Dictionary<string, string> properties = new();
        if (result is null)
        {
            return properties;
        }
        foreach (var prop in result.Properties())
        {
            if (prop.Name.StartsWith("_") || prop.Name.ToLowerInvariant() == "id")
            {
                continue;
            }

            string key = prop.Name;
            if (prop.Value.Type == JTokenType.Object)
            {
                var innerKeys = ParseProperties(prop.Value as JObject);
                foreach (var innerKey in innerKeys)
                {
                    properties.Add($"{key}:{innerKey.Key}", innerKey.Value);
                }
            }
            else
            {
                properties.Add(key, prop.Value.ToString());
            }
        }

        return properties;
    }

    private async Task<ChangeFeedProcessor> StartChangeFeedProcessorAsync(string databaseName, string leaseContainerName, string sourceContainerName)
    {
        Container leaseContainer = cosmosClient.GetContainer(databaseName, leaseContainerName);
        ChangeFeedProcessor changeFeedProcessor = cosmosClient.GetContainer(databaseName, sourceContainerName)
            .GetChangeFeedProcessorBuilder<JObject>(processorName: processorName, onChangesDelegate: HandleChangesAsync)
            .WithInstanceName(instanceName)
            .WithLeaseContainer(leaseContainer)
            .Build();
         
        await changeFeedProcessor.StartAsync(); 
        return changeFeedProcessor;
    }

    private async Task HandleChangesAsync(ChangeFeedProcessorContext context, IReadOnlyCollection<JObject> changes, CancellationToken cancellationToken)
    {
        this.Load();
    }
}
