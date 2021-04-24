using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azbp.Async.Functions
{
    public enum CommandType
    {
        CallAsyncWithActivity,
        CallAsyncWithoutActivity,
        PassingNonSerializableModel,
        CallAzureFunctionSendingMessage,
        CallAzureFunctionDirectly
    }

    public class AsyncFunctions
    {
        private readonly ISwapiClient swapiClient;
        private readonly IQueueStorageRepository queueStorageRepository;

        public AsyncFunctions(ISwapiClient swapiClient, IQueueStorageRepository queueStorageRepository)
        {
            this.swapiClient = swapiClient;
            this.queueStorageRepository = queueStorageRepository;
        }

        [FunctionName(nameof(RunOrchestratorHttp))]
        public async Task<IActionResult> RunOrchestratorHttp(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
           [DurableClient] IDurableOrchestrationClient starter,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation($"Start orchestration process.");

            var parsed = Enum.TryParse(req.Query["commandType"], true, out CommandType commandType);
            if (parsed)
            {
                var instanceId = await starter.StartNewAsync<string>(nameof(RunOrchestrator), commandType.ToString());

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
                return new OkObjectResult("success");
            }

            return new BadRequestObjectResult("Invalid value in 'commandType' QueryString");
        }

        [FunctionName(nameof(RunOrchestrator))]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            int maxNumberOfAttempts = 3;
            try
            {
                var commandType = Enum.Parse(typeof(CommandType), context.GetInput<string>());
                log.LogInformation($"Received command '{commandType}'.");

                switch (commandType)
                {
                    case CommandType.CallAsyncWithActivity:
                        log.LogInformation($"Calling async method using activity with retry options.");
                        await CallAsyncWithActivity(context, maxNumberOfAttempts);
                        log.LogInformation($"Async method using activity called.");
                        break;
                    case CommandType.CallAsyncWithoutActivity:
                        log.LogInformation($"Calling async method in orchestrator directly.");
                        await CallAsyncWithoutActivity();
                        log.LogInformation($"Async method called.");
                        break;
                    case CommandType.PassingNonSerializableModel:
                        log.LogInformation($"Passing Non Serializable parameter to Activity.");
                        await PassingNonSerializableModel(context, maxNumberOfAttempts);
                        log.LogInformation($"Async method called.");
                        break;
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Durable Function retried {maxNumberOfAttempts} attempts.");
                log.LogError($"Exception message: {ex.Message}.");
            }
        }

        [FunctionName(nameof(ActivityWithRetryAsync))]
        public async Task ActivityWithRetryAsync([ActivityTrigger] string item)
        {
            // call external Api
            await swapiClient.CallApiAsync();
        }

        [FunctionName(nameof(ActivityWithRetryAsyncNonSerializable))]
        public async Task ActivityWithRetryAsyncNonSerializable([ActivityTrigger] object item)
        {
            // call external Api
            await swapiClient.CallApiAsync();
        }

        private async Task PassingNonSerializableModel(IDurableOrchestrationContext context, int maxNumberOfAttempts)
        {
            var retryOptions = new RetryOptions(TimeSpan.FromSeconds(5), maxNumberOfAttempts);
            await context.CallActivityWithRetryAsync(nameof(ActivityWithRetryAsyncNonSerializable), retryOptions, new MemoryStream(100));
        }

        private async Task CallAsyncWithActivity(IDurableOrchestrationContext context, int maxNumberOfAttempts)
        {
            var retryOptions = new RetryOptions(TimeSpan.FromSeconds(5), maxNumberOfAttempts);
            await context.CallActivityWithRetryAsync(nameof(ActivityWithRetryAsync), retryOptions, context.GetInput<string>());
        }

        private async Task CallAsyncWithoutActivity()
        {
            // call external Api
            await swapiClient.CallApiAsync();
        }

        [FunctionName(nameof(CallAzureFunction))]
        public async Task<IActionResult> CallAzureFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var parsed = Enum.TryParse(req.Query["commandType"], true, out CommandType commandType);
            if (parsed)
            {
                switch (commandType)
                {
                    case CommandType.CallAzureFunctionDirectly:
                        log.LogInformation($"Calling async method using activity with retry options.");
                        await CallAzureFunctionDirectly(log);
                        log.LogInformation($"Async method using activity called.");
                        break;
                    case CommandType.CallAzureFunctionSendingMessage:
                        log.LogInformation($"Calling async method in orchestrator directly.");
                        await CallAzureFunctionSendingMessage(log);
                        log.LogInformation($"Async method called.");
                        break;
                }

                return new OkObjectResult("success");
            }

            return new BadRequestObjectResult("Invalid value in 'commandType' QueryString");
        }

        private async Task CallAzureFunctionSendingMessage(ILogger log)
        {
            log.LogInformation($"Sending message to queue.");

            await queueStorageRepository.CreateMessageAsync("This is a test queue message");
        }

        [FunctionName(nameof(CallAzureFunctionQueue))]
        public async Task CallAzureFunctionQueue([QueueTrigger("demo", Connection = "ConnectionStrings:QueueDemo")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            throw new NotImplementedException();
        }

        private async Task CallAzureFunctionDirectly(ILogger log)
        {
            var azureFunctionUrl = Environment.GetEnvironmentVariable("AppSettings:FunctionUrl", EnvironmentVariableTarget.Process);
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(azureFunctionUrl)
            };

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/CallAzureFunctionHttp");

            _ = await httpClient.SendAsync(request).ConfigureAwait(false);
        }

        [FunctionName(nameof(CallAzureFunctionHttp))]
        public async Task<IActionResult> CallAzureFunctionHttp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            throw new NotImplementedException();
        }
    }
}
