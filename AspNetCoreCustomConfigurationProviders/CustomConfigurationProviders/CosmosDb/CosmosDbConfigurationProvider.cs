using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
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
        Dictionary<string, object> results = new();
        using FeedIterator<Dictionary<string, object>> resultSetIterator = 
                    container.GetItemQueryIterator<Dictionary<string, object>>(query, requestOptions: new QueryRequestOptions { MaxConcurrency = 1 });

        while (resultSetIterator.HasMoreResults)
        {
            FeedResponse<Dictionary<string, object>> response = Task.Run(async () => await resultSetIterator.ReadNextAsync()).Result;

            foreach (var result in response)
            {
                var configurationObject = JObject.Parse(result.ToString());
                var allConfiguration = ParseProperties(configurationObject);
                foreach (var configurationItem in allConfiguration)
                {
                    Data[configurationItem.Key] = "";// configurationItem.Value;
                }
            }


            //var configurationObject = JObject.Parse(response.ToString());
            //var allConfiguration = ParseProperties(configurationObject);
            //foreach (var configurationItem in allConfiguration)
            //{
            //    Data[configurationItem.Key] = "";// configurationItem.Value;
            //}
        }
    }

    private Dictionary<string, object> ParseProperties(JObject? result)
    {
        Dictionary<string, object> properties = new();
        if (result is null)
        {
            return properties;
        }
        foreach (var prop in result.Properties())
        {
            if (prop.Name.StartsWith("_"))
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
