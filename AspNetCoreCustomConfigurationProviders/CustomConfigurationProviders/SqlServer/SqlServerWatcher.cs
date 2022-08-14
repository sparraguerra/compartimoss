using Microsoft.Extensions.Primitives;

namespace CustomConfigurationProviders.SqlServer;

public interface ISqlServerWatcher : IDisposable
{
    IChangeToken Watch();
}

internal class SqlServerWatcher : ISqlServerWatcher
{
    private readonly TimeSpan refreshInterval;
    private IChangeToken? changeToken;
    private readonly Timer timer;
    private CancellationTokenSource? cancellationTokenSource;

    public SqlServerWatcher(TimeSpan refreshInterval)
    {
        this.refreshInterval = refreshInterval;
        timer = new Timer(callback: Change, null, TimeSpan.Zero, this.refreshInterval);
    }

    private void Change(object? state) => cancellationTokenSource?.Cancel();
    
    public IChangeToken Watch()
    {
        cancellationTokenSource = new CancellationTokenSource();
        changeToken = new CancellationChangeToken(cancellationTokenSource.Token);

        return changeToken;
    }

    public void Dispose()
    {
        timer?.Dispose();
        cancellationTokenSource?.Dispose();
    }
}
