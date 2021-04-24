using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

[assembly: FunctionsStartup(typeof(Azbp.Async.Functions.Startup))]
namespace Azbp.Async.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Load configuration from Azure App Configuration
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            var configuration = configurationBuilder
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                                    .AddEnvironmentVariables()
                                    .Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);
            builder.Services.AddHttpClient<ISwapiClient, SwapiClient>();
            builder.Services.AddSingleton<IQueueStorageRepository, QueueStorageRepository>();
        }
    }
}
