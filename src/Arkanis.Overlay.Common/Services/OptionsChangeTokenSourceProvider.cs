namespace Arkanis.Overlay.Common.Services;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

public class OptionsChangeTokenSourceProvider<TOptions> : IOptionsChangeTokenSource<TOptions>, IDisposable
{
    private CancellationTokenSource _cts = new();

    public virtual void Dispose()
    {
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    public string Name
        => Options.DefaultName;

    public IChangeToken GetChangeToken()
        => new CancellationChangeToken(_cts.Token);

    /// <summary>
    ///     Signal a change: fires callbacks and causes IOptionsMonitor to rebuild.
    /// </summary>
    public void SignalChange()
    {
        var previous = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
        try
        {
            previous.Cancel();
        }
        finally
        {
            previous.Dispose();
        }
    }
}
