namespace Arkanis.Overlay.Infrastructure.Services;

using Abstractions;

public abstract class SelfInitializableServiceBase : InitializableBase, ISelfInitializable
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await InitializeAsyncCore(cancellationToken).ConfigureAwait(false);
            Initialized();
        }
        catch (Exception ex)
        {
            InitializationErrored(ex);
            throw;
        }
    }

    protected abstract Task InitializeAsyncCore(CancellationToken cancellationToken);
}
