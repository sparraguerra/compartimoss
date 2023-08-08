using Dapr.Workflow;

namespace DaprWorkflows.Activities.TaskChaining;

public class TaskChaining4Activity : WorkflowActivity<string, string>
{
    private readonly ILogger<TaskChaining4Activity> logger;

    public TaskChaining4Activity(ILogger<TaskChaining4Activity> logger)
    {
        this.logger = logger;
    }

    public override Task<string> RunAsync(WorkflowActivityContext context, string input)
    {
        logger.LogInformation("Activity {TaskChainingActivity} started by orchestration with Id : {InstanceId}",
                            nameof(TaskChaining4Activity),
                            context.InstanceId);

        return Task.FromResult("Called from TaskFailedException");
    }
}
