namespace Arkanis.Overlay.Infrastructure.Repositories.Local;

using Domain.Abstractions.Services;
using Domain.Models;
using External.UEX.Abstractions;
using External.UEX.Extensions;
using Microsoft.Extensions.Logging;

/// <summary>
///     Provides the current state of the UEX API.
///     Mainly to determine current game version and potential availability of the API.
/// </summary>
/// <param name="staticApi"></param>
/// <param name="logger"></param>
internal sealed class UexServiceStateProvider(IUexStaticApi staticApi, ILogger<UexServiceStateProvider> logger) : IExternalServiceStateProvider, IDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    internal ExternalServiceState CurrentDataState { get; set; } = ServiceUnavailableState.Instance;

    internal DateTimeOffset? CachedUntil { get; set; }

    public void Dispose()
        => _semaphoreSlim.Dispose();

    public async Task<ExternalServiceState> LoadCurrentServiceStateAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Trying to load current state from UEX API");
        await _semaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            if (CachedUntil > DateTimeOffset.Now)
            {
                logger.LogDebug("State is cached until {CachedUntil}, returning: {DataState}", CachedUntil, CurrentDataState);
                return CurrentDataState;
            }

            logger.LogDebug("Loading current state from UEX API");
            var response = await staticApi.GetDataParametersAsync(cancellationToken).ConfigureAwait(false);
            var gameVersion = StarCitizenVersion.Create(response.Result.Data?.Global?.Game_version ?? "unknown");
            var responseHeaders = response.CreateResponseHeaders();
            var requestTime = responseHeaders.GetRequestTime();

            CurrentDataState = new ServiceAvailableState(gameVersion, requestTime);
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
