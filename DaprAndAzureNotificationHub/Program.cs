using Dapr.Client;
using DaprAndAzureNotificationHub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(async (context, services) =>
    {  
        services.AddLogging();
        services.AddSingleton<DaprClient>(new DaprClientBuilder().Build());
        services.AddSingleton<IDaprOutputBindingService, DaprOutputBindingService>();
    })
    .ConfigureLogging((context, cfg) =>
    {
        cfg.AddConsole();
    })
    .Build();

await host.StartAsync();
 
var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

for (int i=0;i<10;i++)
{
    Console.WriteLine($"Sending message to Azure Notification Hub: 'TEST MESSAGE {i}'");
    await SendPushNotificationAsync($"TEST MESSAGE {i}", host.Services.GetService<IConfiguration>()!);
    await Task.Delay(500);
}

lifetime.StopApplication();
await host.WaitForShutdownAsync();

async Task SendPushNotificationAsync(string message, IConfiguration configuration)
{    
    var outputBindingService = host.Services.GetService<IDaprOutputBindingService>()!;  
    var connectionString = IDaprOutputBindingService.ParseConnectionStringForAzureNotificationHub(configuration["NotificationHub:ConnectionString"], configuration["NotificationHub:NotificationHubName"]);

    var metadata = new Dictionary<string, string>()
    {
        { "path", CoreConstants.PUSH_NOTIFICATION_URI_PATH },
        { CoreConstants.CONTENT_TYPE_HEADER_NAME, CoreConstants.JSON_UTF8 },
        { CoreConstants.PUSH_NOTIFICATION_FORMAT_HEADER_NAME, CoreConstants.PUSH_NOTIFICATION_FORMAT_HEADER_VALUE },
        { CoreConstants.PUSH_NOTIFICATION_TAGS_HEADER_NAME, "(follows_RedSox || follows_Cardinals) && location_Boston" },
        { CoreConstants.AUTHORIZATION_HEADER_NAME,
        IDaprOutputBindingService
            .GenerateSasTokenForAzureNotificationHub(connectionString.GetNotificationHubUri(), 5, connectionString.SasKeyName, connectionString.SasKeyValue)}
    };

    var messageText = new
    {
        message
    };

    await outputBindingService.TryPublishMessageAsync(messageText, "post", metadata, CoreConstants.BINDING_NAME_HTTP_PUSH_NOTIFICATIONS);
}
