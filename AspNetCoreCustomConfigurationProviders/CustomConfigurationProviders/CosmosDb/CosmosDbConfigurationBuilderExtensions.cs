using CustomConfigurationProviders.SqlServer;
using Microsoft.Extensions.Configuration;

namespace CustomConfigurationProviders.CosmosDb;

public static class CosmosDbConfigurationBuilderExtensions
{
    //public static IConfigurationBuilder AddCosmosDb(
    //        this IConfigurationBuilder configurationBuilder, CosmosDBClientSettings defaultSettings)
    //{
    //    if (defaultSettings == null)
    //    {
    //        throw new ArgumentNullException(nameof(defaultSettings));
    //    }

    //    configurationBuilder.Add(new CosmosDBConfigurationProvider(defaultSettings));
    //    return configurationBuilder;
    //}


    //public static IConfigurationBuilder AddCosmosDb(this IConfigurationBuilder configurationBuilder,
    //    Action<CosmosDBClientSettings, IConfiguration> cosmosDbBuilderAction)
    //{
    //    var settings = new CosmosDBClientSettings();

    //    setterFn.Invoke(settings, configurationBuilder.Build());

    //    return configurationBuilder.AddCosmosDB(settings);
    //}


    public static IConfigurationBuilder AddCosmosDb(this IConfigurationBuilder builder, CosmosDbConfig cosmosDbConfig)
    {
        if (cosmosDbConfig == null)
        {
            throw new ArgumentNullException(nameof(cosmosDbConfig));
        }

        return builder.AddCosmosDb(cosmosDbBuilder =>
        {
            if (!string.IsNullOrWhiteSpace(cosmosDbConfig.ConnectionString))
            {
                cosmosDbBuilder.UseConnectionString(cosmosDbConfig.ConnectionString);
            }
            if (!string.IsNullOrWhiteSpace(cosmosDbConfig.Endpoint))
            {
                cosmosDbBuilder.UseEndpoint(cosmosDbConfig.Endpoint);
            }
            if (!string.IsNullOrWhiteSpace(cosmosDbConfig.AuthKey))
            {
                cosmosDbBuilder.UseAuthKey(cosmosDbConfig.AuthKey);
            }
            if (!string.IsNullOrWhiteSpace(cosmosDbConfig.DatabaseName))
            {
                cosmosDbBuilder.UseDatabase(cosmosDbConfig.DatabaseName);
            }
            if (!string.IsNullOrWhiteSpace(cosmosDbConfig.ContainerName))
            {
                cosmosDbBuilder.UseContainer(cosmosDbConfig.ContainerName);
            }
        });
    }
                                           

    public static IConfigurationBuilder AddCosmosDb(this IConfigurationBuilder builder, CosmosDbConfig cosmosDbConfig, bool? enableChangeFeed = null) =>
                                            builder.Add(new CosmosDbConfigurationSource
                                            {
                                                ConnectionString = cosmosDbConfig.ConnectionString,
                                                AuthKey = cosmosDbConfig.AuthKey,  
                                                Endpoint = cosmosDbConfig.Endpoint,
                                                DatabaseName = cosmosDbConfig.DatabaseName,
                                                ContainerName = cosmosDbConfig.ContainerName
                                                //SqlServerWatcher = enableChangeFeed.HasValue ?
                                                //                   new SqlServerWatcher(refreshInterval.Value) :
                                                //                   null
                                            });

    public static IConfigurationBuilder AddCosmosDb(this IConfigurationBuilder builder, Action<ICosmosDbConfigurationSourceBuilder> cosmosDbBuilderAction)
    {
        var cosmosDbBuilder = new CosmosDbConfigurationSourceBuilder();
        cosmosDbBuilderAction(cosmosDbBuilder);
        var source = cosmosDbBuilder.Build();
        return builder.Add(source);
    }
}
