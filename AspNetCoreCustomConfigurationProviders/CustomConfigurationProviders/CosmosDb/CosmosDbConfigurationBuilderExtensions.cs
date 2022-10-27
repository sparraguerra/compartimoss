using CustomConfigurationProviders.SqlServer;
using Microsoft.Extensions.Configuration;

namespace CustomConfigurationProviders.CosmosDb;

public static class CosmosDbConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddCosmosDB(
            this IConfigurationBuilder configurationBuilder, CosmosDBClientSettings defaultSettings)
    {
        if (defaultSettings == null)
        {
            throw new ArgumentNullException(nameof(defaultSettings));
        }

        configurationBuilder.Add(new CosmosDBConfigurationProvider(defaultSettings));
        return configurationBuilder;
    }


    public static IConfigurationBuilder AddCosmosDB(this IConfigurationBuilder configurationBuilder,
        Action<CosmosDBClientSettings, IConfiguration> cosmosDbBuilderAction)
    {
        var settings = new CosmosDBClientSettings();

        setterFn.Invoke(settings, configurationBuilder.Build());

        return configurationBuilder.AddCosmosDB(settings);
    }


    public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder builder, string connectionString) =>
                                           builder.AddSqlServer(sqlBuilder => sqlBuilder.UseConnectionString(connectionString));

    public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder builder, string connectionString, TimeSpan? refreshInterval = null) =>
                                            builder.Add(new SqlServerConfigurationSource
                                            {
                                                ConnectionString = connectionString,
                                                SqlServerWatcher = refreshInterval.HasValue ?
                                                                   new SqlServerWatcher(refreshInterval.Value) :
                                                                   null
                                            });

    public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder builder, Action<ISqlServerConfigurationSourceBuilder> sqlBuilderAction)
    {
        var sqlBuilder = new SqlServerConfigurationSourceBuilder();
        sqlBuilderAction(sqlBuilder);
        var source = sqlBuilder.Build();
        return builder.Add(source);
    }
}
