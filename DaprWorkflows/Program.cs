using Dapr.Client;
using Dapr.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddLogging();
        services.AddSingleton<DaprClient>(new DaprClientBuilder().Build());
        services.AddDaprWorkflow(options =>
        {
            //// Note that it's also possible to register a lambda function as the workflow
            //// or activity implementation instead of a class.
            //options.RegisterWorkflow<OrderProcessingWorkflow>();

            //// These are the activities that get invoked by the workflow(s).
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
 

lifetime.StopApplication();
await host.WaitForShutdownAsync();
