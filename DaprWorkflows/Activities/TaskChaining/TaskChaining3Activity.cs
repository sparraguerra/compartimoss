using Dapr.Workflow;

namespace DaprWorkflows.Activities.TaskChaining
{
    public class TaskChaining3Activity : WorkflowActivity<int, bool>
    {
        private readonly ILogger<TaskChaining3Activity> logger;

        public TaskChaining3Activity(ILogger<TaskChaining3Activity> logger)
        {
            this.logger = logger;
        }

        public override Task<bool> RunAsync(WorkflowActivityContext context, int input)
        {
            logger.LogInformation("Activity {TaskChainingActivity} started by orchestration with Id : {InstanceId}",
                                nameof(TaskChaining3Activity),
                                context.InstanceId);

            logger.LogInformation("Activity {TaskChainingActivity} Input data : {Input}",
                            nameof(TaskChaining3Activity),
                            input);

            return Task.FromResult(true);
        }
    }
}
