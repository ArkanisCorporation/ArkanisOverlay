namespace Arkanis.Overlay.Infrastructure.Repositories.Local;

using System.Diagnostics.CodeAnalysis;
using Domain.Models;
using Domain.Models.Game;
using External.UEX.Abstractions;
using External.UEX.Extensions;
using Microsoft.Extensions.Logging;

internal sealed class UexGameDataStateProvider(IUexStaticApi staticApi, ILogger<UexGameDataStateProvider> logger) : IDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    internal GameDataState CurrentDataState { get; set; } = MissingGameDataState.Instance;

    internal DateTimeOffset? CachedUntil { get; set; }

    public void Dispose()
        => _semaphoreSlim.Dispose();

    [SuppressMessage("ReSharper", "RedundantEmptyFinallyBlock")]
    public async Task<GameDataState> LoadCurrentDataState(CancellationToken cancellationToken)
    {
        logger.LogDebug("Trying to load current state from UEX API");
        await _semaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            if (CachedUntil > DateTimeOffset.Now)
            {
                logger.LogDebug("State is cached until {CachedUntil:s}, returning: {DataState}", CachedUntil, CurrentDataState);
                return CurrentDataState;
            }

            logger.LogDebug("Loading current state from UEX API");
            var response = await staticApi.GetDataParametersAsync(cancellationToken).ConfigureAwait(false);
            var gameVersion = StarCitizenVersion.Create(response.Result.Data?.Global?.Game_version ?? "unknown");
            var responseHeaders = response.CreateResponseHeaders();
            var requestTime = responseHeaders.GetRequestTime();

            CurrentDataState = new SyncedGameDataState(gameVersion, requestTime);
            CachedUntil = responseHeaders.GetCacheUntil();

            logger.LogDebug("Loaded and cached: {DataState}", CurrentDataState);
            return CurrentDataState;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}
