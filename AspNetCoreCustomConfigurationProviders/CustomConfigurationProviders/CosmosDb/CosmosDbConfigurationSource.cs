using Microsoft.Extensions.Configuration;

namespace CustomConfigurationProviders.CosmosDb;

public class CosmosDbConfigurationSource : IConfigurationSource
{
    public string? ConnectionString { get; set; }
    public string? Endpoint { get; set; }
    public string? AuthKey { get; set; }
    public string? ContainerName { get; set; } = "Settings";
    public string? DatabaseName { get; set; } = "settings";
    public string? Prefix { get; set; }
    public bool? ChangeFeed { get; set; }
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new CosmosDbConfigurationProvider(this);
}
