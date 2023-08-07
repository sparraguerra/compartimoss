using Dapr.Workflow;
using DaprWorkflows.Models;

namespace DaprWorkflows.Workflows
{
    public class MainOrchestratorWorkflow : Workflow<PaymentRequest, PaymentResult>
    {
        private readonly WorkflowTaskOptions defaultActivityRetryOptions = new()
        {
             RetryPolicy = new WorkflowRetryPolicy(maxNumberOfAttempts: 3, firstRetryInterval: TimeSpan.FromSeconds(5))
        };

        public override async Task<PaymentResult> RunAsync(WorkflowContext context, PaymentRequest input)
        { 
            return new PaymentResult(true);
        }
    }
}
