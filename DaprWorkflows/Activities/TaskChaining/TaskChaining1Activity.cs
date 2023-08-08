using Dapr.Workflow;

namespace DaprWorkflows.Activities.TaskChaining;

public class TaskChaining1Activity : WorkflowActivity<string, string>
{
    private readonly ILogger<TaskChaining4Activity> logger;

    public TaskChaining1Activity(ILogger<TaskChaining4Activity> logger)
    {
        this.logger = logger;
    }

    public override Task<string> RunAsync(WorkflowActivityContext context, string input)
    {
        logger.LogInformation("Activity {TaskChainingActivity} started by orchestration with Id : {InstanceId}",
                            nameof(TaskChaining1Activity),
                            context.InstanceId);

        logger.LogInformation("Activity {TaskChainingActivity} Input data : {Input}", 
                            nameof(TaskChaining1Activity), 
                            input);

        return Task.FromResult(input);
    }
}
