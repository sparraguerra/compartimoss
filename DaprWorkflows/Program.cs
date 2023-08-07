using Dapr.Client;
using Dapr.Workflow;
using DaprWorkflows.Models;
using DaprWorkflows.Workflows;

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
            options.RegisterWorkflow<MainOrchestratorWorkflow>();

            //options.RegisterActivity<NotifyActivity>();
            //options.RegisterActivity<ReserveInventoryActivity>();
            //options.RegisterActivity<RequestApprovalActivity>();
            //options.RegisterActivity<ProcessPaymentActivity>();
            //options.RegisterActivity<UpdateInventoryActivity>();
        });
    })
    .ConfigureLogging((context, cfg) =>
    {
        cfg.AddConsole();
    })
    .Build();

await host.StartAsync();

var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();


using var daprClient = new DaprClientBuilder().Build();

// Wait for the sidecar to become available
while (!await daprClient.CheckHealthAsync())
{
    Thread.Sleep(TimeSpan.FromSeconds(5));
}

for (int i = 0; i < 10; i++)
{
    Console.WriteLine($"Initiating Workflow '{i}'");
    await SchedulingMainWorkflowAsync($"TEST MESSAGE {i}", host.Services.GetService<DaprWorkflowClient>()!);
    await Task.Delay(500);
}

lifetime.StopApplication();
await host.WaitForShutdownAsync();

static async Task SchedulingMainWorkflowAsync(string message, DaprWorkflowClient daprClient)
{ 
    using var cts = new CancellationTokenSource();
    var customerId = Guid.NewGuid().ToString();
    var data = new PaymentRequest(customerId, $"Payment for customer {customerId}", 100, "USD");
    var instanceId = await daprClient.ScheduleNewWorkflowAsync(
                                name: nameof(MainOrchestratorWorkflow),
                                input: data);

    var workflowState = await daprClient.WaitForWorkflowStartAsync(instanceId: instanceId, true, cts.Token);

    Console.WriteLine($"Workflow: {nameof(MainOrchestratorWorkflow)} (ID = {instanceId}) started successfully."); 
}
