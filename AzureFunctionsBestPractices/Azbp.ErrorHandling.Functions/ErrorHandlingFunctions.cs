using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Azbp.ErrorHandling.Functions
{
    public class Item
    {
        [JsonProperty("id")]
        [Required]
        public string Id { get; set; }

        [JsonProperty("name")]
        [Required]
        [StringLength(10)]
        public string Name { get; set; }
    }

    public enum ExceptionType
    {
        ArgumentException,
        ArgumentNullException,
        InvalidOperationException,
        OutOfMemoryException
    }

    public static class ErrorHandlingFunctions
    {
        [FunctionName(nameof(Simplest))]
        public static async Task<IActionResult> Simplest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                var parsed = Enum.TryParse(req.Query["exceptionType"], true, out ExceptionType exceptionType);
                if (parsed)
                {
                    Eval(exceptionType);
                }

                throw new InvalidCastException();
            }
            catch (Exception ex)
            {
                string responseMessage = string.Empty;
                switch (ex)
                {
                    case ArgumentNullException t:
                        responseMessage = nameof(ArgumentNullException);
                        break;
                    case ArgumentException t:
                        responseMessage = nameof(ArgumentException);
                        break;
                    case InvalidOperationException t:
                        responseMessage = nameof(InvalidOperationException);
                        break;
                    case OutOfMemoryException t:
                        responseMessage = nameof(OutOfMemoryException);
                        break;
                    case InvalidCastException t:
                        responseMessage = nameof(InvalidCastException);
                        break;
                }

                log.LogError(responseMessage);
                return new OkObjectResult(responseMessage);
            }
        }

        private static object Eval(ExceptionType exceptionType) =>
            exceptionType switch
            {

                ExceptionType.ArgumentException => throw new ArgumentException(),
                ExceptionType.ArgumentNullException => throw new ArgumentNullException(),
                ExceptionType.InvalidOperationException => throw new InvalidOperationException(),
                _ => throw new OutOfMemoryException(nameof(exceptionType))
            };


        [FunctionName(nameof(FixedDelayRetry))]
        [FixedDelayRetry(3, "00:00:05")]
        public static async Task<IActionResult> FixedDelayRetry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation("FixedDelayRetry throwing exception");

            var exceptionMessage = "FixedDelayRetry thrown exception";
            throw new ApplicationException(exceptionMessage);
        }


        [FunctionName(nameof(ExponentialBackoffRetry))]
        [ExponentialBackoffRetry(3, "00:00:05", "00:05:00")]
        public static async Task<IActionResult> ExponentialBackoffRetry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation("ExponentialBackoffRetry throwing exception");

            var exceptionMessage = "ExponentialBackoffRetry thrown exception";
            throw new ApplicationException(exceptionMessage);
        }

        [FunctionName(nameof(RunOrchestratorRetry))]
        public static async Task RunOrchestratorRetry(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
           [DurableClient] IDurableOrchestrationClient starter,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation($"Start orchestration process.");
            var item = new Item()
            {
                Id = "1",
                Name = "Name"
            };

            var instanceId = await starter.StartNewAsync<Item>(nameof(RunOrchestrator), item);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }

        [FunctionName(nameof(RunOrchestrator))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            int maxNumberOfAttempts = 3;
            try
            {
                var item = context.GetInput<Item>();
                log.LogInformation($"Received new item in orchestration '{JsonConvert.SerializeObject(item)}'.");

                log.LogInformation($"Launch activity with retry options.");

                var retryOptions = new RetryOptions(TimeSpan.FromSeconds(5), maxNumberOfAttempts);
                await context.CallActivityWithRetryAsync(nameof(ActivityWithRetryAsync), retryOptions, item);
            }
            catch (Exception)
            {
                log.LogError($"Durable Function retried {maxNumberOfAttempts} attempts.");
            }

        }

        [FunctionName(nameof(ActivityWithRetryAsync))]
        public static async Task ActivityWithRetryAsync([ActivityTrigger] Item item)
        {
            // launch exception
            throw new ArgumentException(nameof(item));
        }


        [FunctionName(nameof(ValidateRequestBody))]
        public static async Task<IActionResult> ValidateRequestBody(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request."); 

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Item>(requestBody);

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(data, new ValidationContext(data, null, null), validationResults, true);

            string responseMessage = string.Empty;
            if (isValid)
            {
                responseMessage = "Model is valid";
                log.LogInformation(responseMessage);
                return new OkObjectResult(responseMessage);
            }
            else
            {
                responseMessage = $"Model is invalid: {string.Join(", ", validationResults.Select(s => s.ErrorMessage).ToArray())}";
                log.LogInformation(responseMessage);
                return new BadRequestObjectResult(responseMessage);
            }
        }

        [FunctionName(nameof(Idempotency))]
        public static async Task<IActionResult> Idempotency(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Item>(requestBody);

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(data, new ValidationContext(data, null, null), validationResults, true);

            string responseMessage = string.Empty;
            if (!isValid)
            { 
                responseMessage = $"Model is invalid: {string.Join(", ", validationResults.Select(s => s.ErrorMessage).ToArray())}";
                log.LogInformation(responseMessage);
                return new BadRequestObjectResult(responseMessage);
            }

            var cosmosDbConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:CosmosDb", EnvironmentVariableTarget.Process);
            var databaseName = Environment.GetEnvironmentVariable("CosmosDb:DatabaseName", EnvironmentVariableTarget.Process);
            var containerName = Environment.GetEnvironmentVariable("CosmosDb:ContainerName", EnvironmentVariableTarget.Process);

            CosmosClient client = new CosmosClient(cosmosDbConnectionString);
            var database = client.GetDatabase(databaseName);
            var container = database.GetContainer(containerName); 
            try
            { 
                ItemResponse<Item> itemResponse = await container.ReadItemAsync<Item>(data.Id, new PartitionKey(data.Id));
                responseMessage = $"Item in database with id: {itemResponse?.Resource?.Id} already exists.";
                log.LogInformation(responseMessage);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            { 
                ItemResponse<Item> itemResponse = await container.CreateItemAsync<Item>(data, new PartitionKey(data.Id));

                responseMessage = $"Created item in database with id: {itemResponse?.Resource?.Id} Operation consumed {itemResponse?.RequestCharge} RUs.";
                log.LogInformation(responseMessage);
            }

            return new OkObjectResult(responseMessage);
        }
    }
}
