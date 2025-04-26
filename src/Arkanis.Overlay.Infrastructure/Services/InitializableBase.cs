namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions;

public abstract class InitializableBase : IDependable
{
    private TaskCompletionSource _initialization = new();

    public bool IsReady
        => _initialization.Task.IsCompletedSuccessfully;

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await _initialization.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

    protected void Initialized()
        => _initialization.TrySetResult();

    protected void InitializationErrored(Exception ex)
    {
        _initialization.TrySetException(ex);
        InitializationReset();
    }

    private void InitializationReset()
    {
        if (_initialization.Task.IsCompleted)
        {
            _initialization = new TaskCompletionSource();
        }
    }
}
