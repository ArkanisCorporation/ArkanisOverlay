namespace Arkanis.Overlay.Infrastructure.Services;

using Abstractions;

public abstract class SelfInitializableServiceBase : ISelfInitializable
{
    private readonly TaskCompletionSource _initialization = new();

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await _initialization.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await InitializeAsyncCore(cancellationToken).ConfigureAwait(false);
            _initialization.SetResult();
        }
        catch (Exception ex)
        {
            _initialization.SetException(ex);
            throw;
        }
    }

    protected abstract Task InitializeAsyncCore(CancellationToken cancellationToken);
}
