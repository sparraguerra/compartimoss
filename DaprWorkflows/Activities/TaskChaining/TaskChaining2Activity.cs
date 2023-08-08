using Dapr.Workflow;

namespace DaprWorkflows.Activities.TaskChaining
{
    public class TaskChaining2Activity : WorkflowActivity<string, int>
    {
        private readonly ILogger<TaskChaining2Activity> logger;

        public TaskChaining2Activity(ILogger<TaskChaining2Activity> logger)
        {
            this.logger = logger;
        }

        public override Task<int> RunAsync(WorkflowActivityContext context, string input)
        {
            logger.LogInformation("Activity {TaskChainingActivity} started by orchestration with Id : {InstanceId}",
                                nameof(TaskChaining2Activity),
                                context.InstanceId);

            logger.LogInformation("Activity {TaskChainingActivity} Input data : {Input}",
                            nameof(TaskChaining2Activity),
                            input);

            return Task.FromResult(int.MaxValue);
        }
    }
}
