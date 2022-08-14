using Microsoft.Extensions.Configuration;

namespace CustomConfigurationProviders.SqlServer;

public static class SqlServerConfigurationBuilderExtensions
{
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
