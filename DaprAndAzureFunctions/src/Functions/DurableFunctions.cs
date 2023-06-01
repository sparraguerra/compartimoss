using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DaprAndAzureFunctions.Functions;

public class DurableFunctions
{
    private readonly ILogger logger;

    public DurableFunctions(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<DurableFunctions>();
    }

    [FunctionName(nameof(DurableFunctionsOrchestrator))]
    public async Task<List<string>> DurableFunctionsOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var outputs = new List<string>
        {
            // Replace "hello" with the name of your Durable Activity Function.
            await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"),
            await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"),
            await context.CallActivityAsync<string>(nameof(SayHello), "London")
        };

        // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
        return outputs;
    }

    [FunctionName(nameof(SayHello))]
    public string SayHello([ActivityTrigger] string name)
    {
        logger.LogInformation("Saying hello to {name}.", name);
        return $"Hello {name}!";
    }
}