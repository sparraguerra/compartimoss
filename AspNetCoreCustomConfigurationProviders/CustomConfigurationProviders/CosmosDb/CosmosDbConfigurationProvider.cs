using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace CustomConfigurationProviders.CosmosDb;

public class CosmosDbConfigurationProvider : ConfigurationProvider
{
    private readonly CosmosDbConfigurationSource? source; 

    public CosmosDbConfigurationProvider(CosmosDbConfigurationSource source)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));         
    }

    public override void Load()
    {
        var client = !string.IsNullOrWhiteSpace(source?.ConnectionString) ?
                                new CosmosClient(source?.ConnectionString) :
                                new CosmosClient(source?.Endpoint, source?.AuthKey);

        var container = client.GetContainer(source?.DatabaseName, source?.ContainerName);

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
}
