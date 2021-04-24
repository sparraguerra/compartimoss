using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Management.CosmosDB.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Azbp.Security.Functions
{
    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public static class SecurityFunctions
    {
        [FunctionName(nameof(AzureAd))]
        public static async Task<IActionResult> AzureAd(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
                ILogger log, ClaimsPrincipal principal)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var responseMessage =
                    $"Hello, {principal.Identity.Name}. This HTTP triggered function executed successfully.";

            log.LogInformation(responseMessage);
            return new OkObjectResult(responseMessage);
        }

        [FunctionName(nameof(KeyVault))]
        public static async Task<IActionResult> KeyVault(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var cosmosDbConnectionString =
                    Environment.GetEnvironmentVariable("ConnectionStrings:CosmosDb", EnvironmentVariableTarget.Process);

            var responseMessage =
                    $"ConnectionString retrieved from KeyVault: {cosmosDbConnectionString}";

            log.LogInformation(responseMessage);
            return new OkObjectResult(responseMessage);
        }

        [FunctionName(nameof(CosmosDb))]
        public static async Task<IActionResult> CosmosDb(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
           ILogger log)
        {
            var subscriptionId = Environment.GetEnvironmentVariable("CosmosDb:SubscriptionId", EnvironmentVariableTarget.Process);
            var resourceGroupName = Environment.GetEnvironmentVariable("CosmosDb:ResourceGroupName", EnvironmentVariableTarget.Process);
            var accountName = Environment.GetEnvironmentVariable("CosmosDb:AccountName", EnvironmentVariableTarget.Process);
            var cosmosDbEndpoint = Environment.GetEnvironmentVariable("CosmosDb:Endpoint", EnvironmentVariableTarget.Process);
            var databaseName = Environment.GetEnvironmentVariable("CosmosDb:DatabaseName", EnvironmentVariableTarget.Process);
            var containerName = Environment.GetEnvironmentVariable("CosmosDb:ContainerName", EnvironmentVariableTarget.Process);


            log.LogInformation("C# HTTP trigger function processed a request.");
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com/");
            string endpoint = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{accountName}/listKeys?api-version=2020-06-01-preview";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var result = await httpClient.PostAsync(endpoint, new StringContent(""));
            var keys = await result.Content.ReadAsAsync<DatabaseAccountListKeysResult>();

            log.LogInformation("Starting to create the client");

            CosmosClient client = new CosmosClient(cosmosDbEndpoint, keys.PrimaryMasterKey);

            log.LogInformation("Client created");

            var database = client.GetDatabase(databaseName);
            var container = database.GetContainer(containerName);

            log.LogInformation("Retrieving data from CosmosDb");

            string id = req.Query["id"];
            using var feedIterator = container.GetItemLinqQueryable<Item>()
                     .Where(b => b.Id == id)
                     .ToFeedIterator();
            var items = new List<Item>();
            while (feedIterator.HasMoreResults)
            {
                foreach (var item in await feedIterator.ReadNextAsync())
                {
                    items.Add(item);
                }
            }

            var responseMessage =
                            $"Record retrieved from CosmosDb:" +
                            $"{JsonConvert.SerializeObject(items)}";

            log.LogInformation(responseMessage);
            return new OkObjectResult(responseMessage);

        }
    }
}
