using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Dapr;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DaprAndAzureFunctions.Functions;

public class DaprFunction
{
    private readonly ILogger _logger;

    public DaprFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<DaprFunction>();
    }

    [FunctionName(nameof(ConsumeMessageFromAzureStorageQueue))]
    public void ConsumeMessageFromAzureStorageQueue(
            // Note: the value of BindingName must match the binding name in components/storage-queue.yaml
            [DaprBindingTrigger(BindingName = "storage-queue")] JObject triggerData,
            ILogger log)
    {
        log.LogInformation("Hello from AzureStorageQueue!");

        log.LogInformation($"Trigger data: {triggerData}");
    }

}
