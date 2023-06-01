using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Dapr;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DaprAndAzureFunctions.Functions;

public class DaprFunction
{
    private readonly ILogger logger;

    public DaprFunction(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<DaprFunction>();
    }

    [FunctionName(nameof(ConsumeMessageFromAzureStorageQueue))]
    public void ConsumeMessageFromAzureStorageQueue(
            // Note: the value of BindingName must match the binding name in components/storage-queue.yaml
            [DaprBindingTrigger(BindingName = "storage-queue")] JObject triggerData)
    {
        logger.LogInformation("Hello from AzureStorageQueue!");

        logger.LogInformation($"Trigger data: {triggerData}");
    }

}
