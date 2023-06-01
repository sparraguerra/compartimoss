using DaprAndAzureFunctions;
using DaprAndAzureFunctions.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]
namespace DaprAndAzureFunctions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        ILoggerFactory loggerFactory =  LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        builder.Services.AddSingleton(loggerFactory);
         
        builder.Services.AddSingleton<IDaprOutputBindingService, DaprOutputBindingService>();
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {  
        builder.ConfigurationBuilder
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
            .Build();
         
    }
}
