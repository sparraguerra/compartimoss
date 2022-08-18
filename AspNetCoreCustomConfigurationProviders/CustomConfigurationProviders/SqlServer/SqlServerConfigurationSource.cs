using Microsoft.Extensions.Configuration;

namespace CustomConfigurationProviders.SqlServer;

public class SqlServerConfigurationSource : IConfigurationSource
{
    public string? ConnectionString { get; set; }
    public string? CustomQuery { get; set; } =  $"select [Key], [Value] from dbo.Settings";
    public string? Schema { get; set; } = "dbo";
    public string? Table { get; set; } = "Settings";
    public string? KeyColumn { get; set; } = "Key";
    public string? ValueColumn { get; set; } = "Value";
    public string? Prefix { get; set; }
    internal ISqlServerWatcher? SqlServerWatcher { get; set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new SqlServerConfigurationProvider(this);
}
