namespace Arkanis.Overlay.Components.Helpers;

using Microsoft.Extensions.Logging;

public sealed class DebounceHelper : IDisposable, IAsyncDisposable
{
    private readonly Func<Task> _debounceHandler;
    private readonly TimeSpan _interval;
    private readonly ILogger _logger;
    private readonly Timer _timer;

    public DebounceHelper(Func<Task> debounceHandler, ILogger logger, TimeSpan? interval = null)
    {
        _debounceHandler = debounceHandler;
        _logger = logger;
        _interval = interval ?? TimeSpan.FromMilliseconds(150);
        _timer = new Timer(HandleTimeout, debounceHandler, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public async ValueTask DisposeAsync()
        => await _timer.DisposeAsync();

    public void Dispose()
        => _timer.Dispose();

    public void RequestDebounced()
    {
        _logger.LogTrace("Scheduling debounced action");
        _timer.Change(_interval, Timeout.InfiniteTimeSpan);
    }

    public void Cancel()
    {
        _logger.LogTrace("Cancelling debounced action");
        _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    private async void HandleTimeout(object? handler)
    {
        try
        {
            await _debounceHandler();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Debounce handler processing has failed");
        }
    }
}
