namespace CustomConfigurationProviders.CosmosDb;

public class CosmosDbConfig
{  
    public string? ConnectionString { get; set; }

    public string? Endpoint { get; set; }

    public string? AuthKey { get; set; }

    public string? DatabaseName { get; set; }

    public string? ContainerName { get; set; }
}