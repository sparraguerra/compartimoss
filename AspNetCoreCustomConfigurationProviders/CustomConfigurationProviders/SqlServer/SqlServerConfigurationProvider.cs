using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Data.SqlClient;

namespace CustomConfigurationProviders.SqlServer;

public class SqlServerConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly SqlServerConfigurationSource? source;
    private readonly string? query;
    private readonly string? prefix;
    private readonly IDisposable? changeTokenRegistration;

    public SqlServerConfigurationProvider(SqlServerConfigurationSource source, string? prefix)
    {
        this.source = source;
        this.prefix = prefix;
        query = this.source?.CustomQuery; 

        if (this.source?.SqlServerWatcher is not null)
        {
            changeTokenRegistration = ChangeToken.OnChange(
                                                () => this.source.SqlServerWatcher.Watch(),
                                                this.Load
            );
        }
    }

    public override void Load()
    {
        var data = new Dictionary<string, string>();

        using var connection = new SqlConnection(source?.ConnectionString);
        var query = new SqlCommand(this.query, connection);

        query.Connection.Open();
        using var reader = query.ExecuteReader();
        if (reader?.HasRows == true)
        { 
            while (reader?.Read() == true)
            {
                data.Add(!string.IsNullOrWhiteSpace(prefix) ?
                         $"{prefix}:{reader[0]}":
                         reader[0].ToString()!, reader[1].ToString()!);
            }
        }                   

        Data = data;
    }

    public void Dispose()
    {
        changeTokenRegistration?.Dispose();
        source?.SqlServerWatcher?.Dispose();
    }
}
