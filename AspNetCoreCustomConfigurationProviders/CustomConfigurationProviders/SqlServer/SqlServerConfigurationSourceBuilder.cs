namespace CustomConfigurationProviders.SqlServer;

public interface ISqlServerConfigurationSourceBuilder
{
    ISqlServerConfigurationSourceBuilder UseConnectionString(string connectionString);
    ISqlServerConfigurationSourceBuilder UseCustomQuery(string query);
    ISqlServerConfigurationSourceBuilder WithTable(string table);
    ISqlServerConfigurationSourceBuilder WithKeyColumn(string keyColumn);
    ISqlServerConfigurationSourceBuilder WithValueColumn(string valueColumn);
    ISqlServerConfigurationSourceBuilder WithSchema(string valueColumn);
    ISqlServerConfigurationSourceBuilder WithPrefix(string prefix);
    ISqlServerConfigurationSourceBuilder ConfigureRefresh(TimeSpan refreshInterval); 
    

    SqlServerConfigurationSource Build();
}

public class SqlServerConfigurationSourceBuilder : ISqlServerConfigurationSourceBuilder
{
    public string? ConnectionString { get; private set; }
    public string? CustomQuery { get; private set; }        
    public string? Table { get; private set; }
    public string? KeyColumn { get; private set; }
    public string? ValueColumn { get; private set; }
    public string? Schema { get; private set; }
    public string? Prefix { get; private set; }
    public TimeSpan? RefreshInterval { get; private set; }

    public ISqlServerConfigurationSourceBuilder UseConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(connectionString, $"Connection string could not be null or empty!");
        }

        ConnectionString = connectionString;

        return this;
    }

    public ISqlServerConfigurationSourceBuilder UseCustomQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        { 
            throw new ArgumentNullException(query, $"Query could not be null or empty!");
        }

        CustomQuery = query;

        return this;
    }

    public ISqlServerConfigurationSourceBuilder WithTable(string table)
    {
        if (string.IsNullOrWhiteSpace(table))
        {
            throw new ArgumentNullException(table, $"Table could not be null or empty!");
        }

        Table = table;

        return this;
    }

    public ISqlServerConfigurationSourceBuilder WithKeyColumn(string keyColumn)
    {
        if (string.IsNullOrWhiteSpace(keyColumn))
        {
            throw new ArgumentNullException(keyColumn, $"Key column could not be null or empty!");
        }
        KeyColumn = keyColumn;
        return this;
    }

    public ISqlServerConfigurationSourceBuilder WithValueColumn(string valueColumn)
    {
        if (string.IsNullOrWhiteSpace(valueColumn))
        {
            throw new ArgumentNullException(valueColumn, $"Value column could not be null or empty!");
        }
        ValueColumn = valueColumn;
        return this;
    }

    public ISqlServerConfigurationSourceBuilder WithSchema(string schema)
    {
        if (string.IsNullOrWhiteSpace(schema))
        {
            throw new ArgumentNullException(schema, $"Schema could not be null or empty!");
        }

        Schema = schema;
        return this;
    }

    public ISqlServerConfigurationSourceBuilder WithPrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentNullException(prefix, $"Prefix could not be null or empty!");
        }

        Prefix = prefix;
        return this;
    }

    public ISqlServerConfigurationSourceBuilder ConfigureRefresh(TimeSpan refreshInterval)
    {
        if (refreshInterval < TimeSpan.Zero)
        {
            throw new ArgumentException($"Refresh interval must be positive.");
        }                

        RefreshInterval = refreshInterval;
        return this;
    } 

    public SqlServerConfigurationSource Build()
    {
        var instance = new SqlServerConfigurationSource { ConnectionString = ConnectionString };

        if (Table != null)
        {
            instance.Table = Table;
        }                

        if (KeyColumn != null)
        {
            instance.KeyColumn = KeyColumn;
        }                

        if (ValueColumn != null)
        {
            instance.ValueColumn = ValueColumn;
        }

        if (Schema != null)
        {
            instance.Schema = Schema;
        }

        if (Prefix != null)
        {
            instance.Prefix = Prefix;
        }

        if (CustomQuery != null)
        {
            instance.CustomQuery = CustomQuery;
        }

        if (RefreshInterval != null)
        {
            instance.SqlServerWatcher = new SqlServerWatcher(RefreshInterval.Value);
        }            

        return instance;
    }
}
