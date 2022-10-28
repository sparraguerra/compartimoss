using System.Data.Common;

namespace CustomConfigurationProviders.CosmosDb;

public interface ICosmosDbConfigurationSourceBuilder
{
    ICosmosDbConfigurationSourceBuilder UseConnectionString(string connectionString);
    ICosmosDbConfigurationSourceBuilder UseEndpoint(string endpoint);
    ICosmosDbConfigurationSourceBuilder UseAuthKey(string authKey);
    ICosmosDbConfigurationSourceBuilder UseContainer(string container);
    ICosmosDbConfigurationSourceBuilder UseDatabase(string database);
    ICosmosDbConfigurationSourceBuilder WithPrefix(string prefix);
    ICosmosDbConfigurationSourceBuilder EnableChangeFeed();
    CosmosDbConfigurationSource Build();
}

public class CosmosDbConfigurationSourceBuilder : ICosmosDbConfigurationSourceBuilder
{
    public string? ConnectionString { get; private set; }
    public string? Endpoint { get; private set; }
    public string? AuthKey { get; private set; }
    public string? ContainerName { get; private set; }
    public string? DatabaseName { get; private set; }
    public string? Prefix { get; private set; }
    public bool? ChangeFeed { get; private set; }

    public ICosmosDbConfigurationSourceBuilder UseConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(connectionString, $"Connection string could not be null or empty!");
        }

        DbConnectionStringBuilder builder = new()
        {
            ConnectionString = connectionString
        };

        if (builder.TryGetValue("AccountKey", out var key))
        {
            AuthKey = key.ToString();
        }

        if (builder.TryGetValue("AccountEndpoint", out var uri))
        {
            Endpoint =  uri.ToString();
        }

        ConnectionString = connectionString;

        return this;
    }

    public ICosmosDbConfigurationSourceBuilder UseEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentNullException(endpoint, $"Endpoint string could not be null or empty!");
        }

        Endpoint = endpoint;

        return this;
    }

    public ICosmosDbConfigurationSourceBuilder UseContainer(string container)
    {
        if (string.IsNullOrWhiteSpace(container))
        {
            throw new ArgumentNullException(container, $"Container string could not be null or empty!");
        }

        ContainerName = container;

        return this;
    }

    public ICosmosDbConfigurationSourceBuilder UseDatabase(string database)
    {
        if (string.IsNullOrWhiteSpace(database))
        {
            throw new ArgumentNullException(database, $"Database string could not be null or empty!");
        }

        DatabaseName = database;

        return this;
    }

    public ICosmosDbConfigurationSourceBuilder WithPrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentNullException(prefix, $"Prefix could not be null or empty!");
        }

        Prefix = prefix;

        return this;
    }

    public ICosmosDbConfigurationSourceBuilder EnableChangeFeed()
    {
        ChangeFeed = true;

        return this;
    }

    public ICosmosDbConfigurationSourceBuilder UseAuthKey(string authKey)
    {
        if (string.IsNullOrWhiteSpace(authKey))
        {
            throw new ArgumentNullException(authKey, $"AuthKey string could not be null or empty!");
        }

        AuthKey = authKey;

        return this;
    }

    public CosmosDbConfigurationSource Build()
    {
        var instance = new CosmosDbConfigurationSource();

        if (ConnectionString != null)
        {
            instance.ConnectionString = ConnectionString;
        }

        if (Endpoint != null)
        {
            instance.Endpoint = Endpoint;
        }

        if (AuthKey != null)
        {
            instance.AuthKey = AuthKey;
        }

        if (ContainerName != null)
        {
            instance.ContainerName = ContainerName;
        }

        if (DatabaseName != null)
        {
            instance.DatabaseName = DatabaseName;
        }

        if (Prefix != null)
        {
            instance.Prefix = Prefix;
        }

        if (ChangeFeed != null)
        {
            instance.ChangeFeedWatcher = new CosmosDbChangeFeedProcessor();
        }

        return instance;
    }
}
