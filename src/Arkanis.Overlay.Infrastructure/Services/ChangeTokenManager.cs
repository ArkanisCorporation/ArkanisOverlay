namespace Arkanis.Overlay.Infrastructure.Services;

using Abstractions;
using Microsoft.Extensions.Primitives;

public sealed class ChangeTokenManager : IChangeTokenManager, IDisposable, IAsyncDisposable
{
    private readonly Dictionary<Type, CancellationTokenSource> _changeProviders = [];
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async ValueTask DisposeAsync()
    {
        foreach (var cancellationTokenSource in _changeProviders.Values)
        {
            await cancellationTokenSource.CancelAsync();
            cancellationTokenSource.Dispose();
        }

        _changeProviders.Clear();
    }

    public void Dispose()
    {
        foreach (var cancellationTokenSource in _changeProviders.Values)
        {
            cancellationTokenSource.Dispose();
        }

        _changeProviders.Clear();
    }

    public IChangeToken GetChangeTokenFor<T>()
        => GetChangeTokenFor(typeof(T));

    public async Task<IChangeToken> ResetChangeTokenFor<T>()
    {
        try
        {
            await _semaphore.WaitAsync();
            var targetType = typeof(T);

            if (!_changeProviders.TryGetValue(targetType, out var changeProvider))
            {
                return GetChangeTokenFor(targetType);
            }

            //! new change token must be created BEFORE announcing the change, the update handler may contain logic to refresh the change token
            //! not doing so may lead to stack overflows or deadlocks
            GetOrCreateChangeProvider(targetType, true);

            // announce the change
            await changeProvider.CancelAsync();
            changeProvider.Dispose();

            return GetChangeTokenFor(targetType);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task TriggerChangeForAsync<T>()
        => await ResetChangeTokenFor<T>();

    private CancellationChangeToken GetChangeTokenFor(Type type)
    {
        var changeProvider = GetOrCreateChangeProvider(type);
        return new CancellationChangeToken(changeProvider.Token);
    }

    private CancellationTokenSource GetOrCreateChangeProvider(Type targetType, bool recreate = false)
    {
        CancellationTokenSource? changeProvider;
        if (recreate)
        {
            changeProvider = _changeProviders[targetType] = new CancellationTokenSource();
        }
        else if (!_changeProviders.TryGetValue(targetType, out changeProvider))
        {
            changeProvider = new CancellationTokenSource();
            if (!_changeProviders.TryAdd(targetType, changeProvider))
            {
                // there is already an existing change provider
                changeProvider = _changeProviders[targetType];
            }
        }

        return changeProvider;
    }
}
