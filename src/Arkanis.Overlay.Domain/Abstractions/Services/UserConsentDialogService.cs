namespace Arkanis.Overlay.Domain.Abstractions.Services;

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

public sealed class UserConsentDialogService(ILogger<UserConsentDialogService> logger) : IUserConsentDialogService,
    IUserConsentDialogService.IConnector,
    IDisposable
{
    private readonly ConcurrentQueue<PendingRequest> _pendingRequests = new();
    private readonly SemaphoreSlim _requestSemaphore = new(1, 1);
    private IUserConsentDialogService? _currentConnector;

    public async Task LinkAsync(IUserConsentDialogService connector)
    {
        logger.LogInformation("Linking with {Connector}", connector);
        _currentConnector = connector;

        while (_pendingRequests.TryDequeue(out var pendingRequest))
        {
            await pendingRequest.RunAsync(_currentConnector);
        }
    }

    public void Unlink(IUserConsentDialogService connector)
    {
        logger.LogInformation("Unlinking from {Connector}", connector);
        _currentConnector = null;
    }

    public void Dispose()
        => _requestSemaphore.Dispose();

    public async Task<IUserConsentDialogService.Result> RequestConsentAsync<T>(IDictionary<string, object> parameters)
        where T : ComponentBase, IUserConsentDialogService.IContent
    {
        logger.LogInformation("Requesting consent using {Type}", typeof(T).Name);

        if (_currentConnector is not null)
        {
            return await PromptConnectorAsync(_currentConnector);
        }

        var tcs = new TaskCompletionSource<IUserConsentDialogService.Result>();

        var pendingRequest = new PendingRequest(tcs)
        {
            PromptAsync = PromptConnectorAsync,
        };

        _pendingRequests.Enqueue(pendingRequest);
        return await tcs.Task;

        async Task<IUserConsentDialogService.Result> PromptConnectorAsync(IUserConsentDialogService connector)
        {
            await _requestSemaphore.WaitAsync();
            try
            {
                return await connector.RequestConsentAsync<T>(parameters);
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }
    }

    public record PendingRequest(TaskCompletionSource<IUserConsentDialogService.Result> CompletionSource)
    {
        public required Func<IUserConsentDialogService, Task<IUserConsentDialogService.Result>> PromptAsync { private get; init; }

        public async Task RunAsync(IUserConsentDialogService connector)
        {
            try
            {
                var result = await PromptAsync(connector);
                CompletionSource.SetResult(result);
            }
            catch (Exception e)
            {
                CompletionSource.SetException(e);
            }
        }
    }
}
