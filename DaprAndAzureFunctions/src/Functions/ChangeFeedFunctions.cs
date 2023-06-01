using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace DaprAndAzureFunctions.Functions;

public class ChangeFeedFunctions
{
    private readonly ILogger _logger;

    public ChangeFeedFunctions(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ChangeFeedFunctions>();
    }

    [FunctionName(nameof(HandleChangeFeed))]
    public void HandleChangeFeed([CosmosDBTrigger(
        databaseName: "ToDoList",
        containerName: "Items",
        Connection = "ConnectionStrings:CosmosDb",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<MyDocument> input)
    {
        if (input != null && input.Count > 0)
        {
            _logger.LogInformation("Documents modified: " + input.Count);
            _logger.LogInformation("First document Id: " + input[0].Id);
            _logger.LogInformation("First document PartitionKey: " + input[0].PartitionKey);
            _logger.LogInformation("First document Text: " + input[0].Text);
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
