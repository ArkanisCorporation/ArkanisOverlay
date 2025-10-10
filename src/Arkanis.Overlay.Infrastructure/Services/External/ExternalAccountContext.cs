namespace Arkanis.Overlay.Infrastructure.Services.External;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Common.Models;
using Common.Options;
using Common.Services;
using Domain.Abstractions.Services;
using FluentResults;
using Microsoft.Extensions.Logging;

public class ExternalAccountContext(
    ExternalAuthenticator authenticator,
    IUserPreferencesManager userPreferences,
    ILogger logger
) : SelfInitializableServiceBase, IExternalAccountContext, IDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private ExternalAuthenticator.AuthTaskBase? _currentAuthentication;

    protected ILogger Logger { get; } = logger;

    protected string ServiceIdentifier
        => authenticator.AuthenticatorInfo.Identifier;

    protected virtual ExternalAuthenticator.AuthTaskBase? CurrentAuthentication
        => _currentAuthentication;

    public void Dispose()
    {
        _semaphoreSlim.Dispose();
        GC.SuppressFinalize(this);
    }

    public ClaimsIdentity Identity
        => CurrentAuthentication?.Identity ?? new ClaimsIdentity();

    [MemberNotNullWhen(true, nameof(CurrentAuthentication))]
    public virtual bool IsAuthenticated
        => CurrentAuthentication is { IsAuthenticated: true };

    public Result<ClaimsIdentity>? LastResult { get; private set; }

    public async Task UpdateAsync(CancellationToken cancellationToken)
    {
        var serviceCredentials = userPreferences.CurrentPreferences.GetCredentialsOrDefaultFor(ServiceIdentifier);
        var validationResult = authenticator.ValidateCredentials(serviceCredentials);
        if (validationResult.IsFailed)
        {
            Logger.LogWarning("Credentials for {ServiceIdentifier} are invalid (clearing current auth): {@Errors}", ServiceIdentifier, validationResult.Errors);
            _currentAuthentication = null;
            await UpdateAsyncCore(cancellationToken);
            return;
        }

        if (serviceCredentials is null)
        {
            Logger.LogDebug("No valid credentials for {ServiceIdentifier} found, clearing current auth", ServiceIdentifier);
            _currentAuthentication = null;
            await UpdateAsyncCore(cancellationToken);
            return;
        }

        await UpdateAsync(serviceCredentials, cancellationToken);
        //? regardless of the result, we do not update stored credentials here
    }

    public async Task UnlinkAsync(CancellationToken cancellationToken)
    {
        var updatedPreferences = userPreferences.CurrentPreferences.RemoveCredentialsFor(ServiceIdentifier);
        await userPreferences.SaveAndApplyUserPreferencesAsync(updatedPreferences);
    }

    public async Task<Result<ClaimsIdentity>> ConfigureAsync(AccountCredentials credentials, CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            await UpdateAsync(credentials, cancellationToken);
            if (!IsAuthenticated)
            {
                return LastResult;
            }

            //? authentication was successful, save the credentials
            var updatedPreferences = userPreferences.CurrentPreferences.SetCredentials(credentials);
            await userPreferences.SaveAndApplyUserPreferencesAsync(updatedPreferences);

            return LastResult;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

#pragma warning disable CS8774 // Member 'LastResult' must have a non-null value when exiting.
    [MemberNotNull(nameof(LastResult))]
    protected async Task UpdateAsync(AccountCredentials credentials, CancellationToken cancellationToken)
    {
        _currentAuthentication = authenticator.AuthenticateAsync(credentials, cancellationToken);
        LastResult = await _currentAuthentication;
        await UpdateAsyncCore(cancellationToken);
    }
#pragma warning restore CS8774

    protected Task UpdateAsyncCore(CancellationToken cancellationToken)
        => Task.CompletedTask;

    protected override async Task InitializeAsyncCore(CancellationToken cancellationToken)
    {
        userPreferences.ApplyPreferences += OnApplyPreferences;
        await UpdateAsync(cancellationToken);
    }

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    private async void OnApplyPreferences(object? _, UserPreferences preferences)
    {
        if (_semaphoreSlim.CurrentCount < 1)
        {
            //? an update is already in progress, skip this one
            Logger.LogDebug("Skipping apply preferences update as an update is already in progress");
            return;
        }

        await _semaphoreSlim.WaitAsync();
        try
        {
            await UpdateAsync(CancellationToken.None);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to update external account status");
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}

public abstract class ExternalAccountContext<T>(
    ExternalAuthenticator<T> authenticator,
    IUserPreferencesManager userPreferences,
    ILogger logger
) : ExternalAccountContext(authenticator, userPreferences, logger)
    where T : ExternalAuthenticator.AuthTaskBase
{
    protected override T? CurrentAuthentication
        => base.CurrentAuthentication as T;

    [MemberNotNullWhen(true, nameof(CurrentAuthentication))]
    public override bool IsAuthenticated
        => CurrentAuthentication is { IsAuthenticated: true };
}
