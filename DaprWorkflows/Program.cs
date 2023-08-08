using Dapr.Client;
using Dapr.Workflow;
using DaprWorkflows.Activities.TaskChaining;
using DaprWorkflows.Workflows.TaskChaining;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")))
{
    Environment.SetEnvironmentVariable("DAPR_HTTP_PORT", "3500");
}

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT")))
{
    Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", "4001");
}

using IHost host = Host.CreateDefaultBuilder(args) 
    .ConfigureServices((context, services) =>
    {
        services.AddLogging();
        services.AddSingleton<DaprClient>(new DaprClientBuilder().Build());
        services.AddDaprWorkflow(options =>
        {
            #region TaskChaining
            options.RegisterWorkflow<TaskChainingOrchestratorWorkFlow>();

            options.RegisterActivity<TaskChaining1Activity>();
            options.RegisterActivity<TaskChaining2Activity>();
            options.RegisterActivity<TaskChaining3Activity>();
            options.RegisterActivity<TaskChaining4Activity>();
            #endregion
        });
    })
    .ConfigureLogging((context, cfg) =>
    {
        cfg.AddConsole();
    })
    .Build();

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("*** Welcome to the Dapr Workflow console app sample!");
Console.WriteLine("*** Using this app, you can place orders that start workflows.");
Console.WriteLine("*** Ensure that Dapr is running in a separate terminal window using the following command:");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("        dapr run --dapr-grpc-port 4001 --app-id wfapp");
Console.WriteLine();
Console.ResetColor();

host.Start();

using var daprClient = new DaprClientBuilder().Build();

// Wait for the sidecar to become available
while (!await daprClient.CheckHealthAsync())
{
    Thread.Sleep(TimeSpan.FromSeconds(5));
}

var scheduledOrchestrations = new List<Task>
{
SchedulingTaskChainingWorkflowAsync(host.Services.GetService<DaprWorkflowClient>()!)
};

await Task.WhenAll(scheduledOrchestrations);

while (true)
{
    Console.Read();
}

static async Task SchedulingTaskChainingWorkflowAsync(DaprWorkflowClient daprWorkflowClient)
{ 
    using var cts = new CancellationTokenSource(); 
    var input = "Execute TaskChainingOrchestratorWorkFlow";
    var instanceId = await daprWorkflowClient.ScheduleNewWorkflowAsync(name: nameof(TaskChainingOrchestratorWorkFlow), input: input);
    var workflowState = await daprWorkflowClient.WaitForWorkflowStartAsync(instanceId: instanceId, true, cts.Token);

    Console.WriteLine($"Workflow: {nameof(TaskChainingOrchestratorWorkFlow)} (ID = {instanceId}) started successfully."); 
}
