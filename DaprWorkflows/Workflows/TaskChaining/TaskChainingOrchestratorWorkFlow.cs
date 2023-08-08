using Dapr.Workflow;
using DaprWorkflows.Activities.TaskChaining;
using Microsoft.DurableTask;

namespace DaprWorkflows.Workflows.TaskChaining;

public class TaskChainingOrchestratorWorkFlow : Workflow<string, string>
{
    private readonly WorkflowTaskOptions defaultActivityRetryOptions = new()
    {
        RetryPolicy = new WorkflowRetryPolicy(maxNumberOfAttempts: 3, firstRetryInterval: TimeSpan.FromSeconds(5))
    };

    public override async Task<string> RunAsync(WorkflowContext context, string input)
    {
        try
        {
            var result1 = await context.CallActivityAsync<string>(nameof(TaskChaining1Activity), input, defaultActivityRetryOptions);
            var result2 = await context.CallActivityAsync<int>(nameof(TaskChaining2Activity), result1, defaultActivityRetryOptions);
            var result3 = await context.CallActivityAsync<bool>(nameof(TaskChaining3Activity), result2, defaultActivityRetryOptions);

            var result = string.Join(", ", result1, result2, result3);
            Console.WriteLine($"Workflow: {nameof(TaskChainingOrchestratorWorkFlow)} (ID = {context.InstanceId}) execution result {result}.");

            return result;
        }
        catch (TaskFailedException) // Task failures are surfaced as TaskFailedException
        {
            // Retries expired - apply custom compensation logic
            _ = await context.CallActivityAsync<string>(nameof(TaskChaining4Activity), options: defaultActivityRetryOptions);
            throw;
        }
    }
}
