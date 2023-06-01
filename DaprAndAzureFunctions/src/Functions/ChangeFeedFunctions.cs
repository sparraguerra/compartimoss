using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DaprAndAzureFunctions.Functions;

public class ChangeFeedFunctions
{
    private readonly ILogger logger;

    public ChangeFeedFunctions(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<ChangeFeedFunctions>();
    }

    [FunctionName(nameof(HandleChangeFeed))]
    public async Task HandleChangeFeed([CosmosDBTrigger(
        databaseName: "ToDoList",
        containerName: "Items",
        Connection = "ConnectionStrings:CosmosDb",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<MyDocument> input,
        [DurableClient] IDurableOrchestrationClient starter)
    {
        if (input != null && input.Count > 0)
        {
            logger.LogInformation("Documents modified: " + input.Count);
            logger.LogInformation("First document Id: " + input[0].Id);
            logger.LogInformation("First document PartitionKey: " + input[0].PartitionKey);
            logger.LogInformation("First document Text: " + input[0].Text);

            string instanceId = await starter.StartNewAsync("DurableFunctionsOrchestrator", null);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
        }
    }
}

public class MyDocument
{
    public string? PartitionKey { get; set; }

    public string? Id { get; set; }

    public string? Text { get; set; }

    public int Number { get; set; }

    public bool Boolean { get; set; }
}
