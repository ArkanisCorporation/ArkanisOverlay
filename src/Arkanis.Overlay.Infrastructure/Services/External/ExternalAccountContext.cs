namespace Arkanis.Overlay.Infrastructure.Services.External;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Common.Abstractions;
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
        => AuthenticatorInfo.ServiceId;

    public virtual ExternalAuthenticatorInfo AuthenticatorInfo
        => authenticator.AuthenticatorInfo;

    protected virtual ExternalAuthenticator.AuthTaskBase? CurrentAuthentication
        => _currentAuthentication;

    public virtual void Dispose()
    {
        userPreferences.ApplyPreferences -= OnApplyPreferences;
        authenticator.RefreshRequested -= AuthenticatorOnRefreshRequested;
        _semaphoreSlim.Dispose();
        GC.SuppressFinalize(this);
    }

    public ClaimsIdentity Identity
        => CurrentAuthentication?.Identity ?? new ClaimsIdentity();

    [MemberNotNullWhen(true, nameof(CurrentAuthentication))]
    public virtual bool IsAuthenticated
        => CurrentAuthentication is { IsAuthenticated: true };

    public Result<ClaimsIdentity>? LastResult { get; private set; }

    public virtual async Task UpdateAsync(CancellationToken cancellationToken)
    {
        var serviceCredentials = userPreferences.CurrentPreferences.GetCredentialsOrDefaultFor(ServiceIdentifier);
        var validationResult = authenticator.ValidateCredentials(serviceCredentials);
        if (validationResult.IsFailed)
        {
            Logger.LogWarning("Credentials for {ServiceIdentifier} are invalid (clearing current auth): {@Errors}", ServiceIdentifier, validationResult.Errors);
            _currentAuthentication = null;
            LastResult = null;
            await OnAuthenticationStateChangedAsync(cancellationToken);
            return;
        }

        if (serviceCredentials is null)
        {
            Logger.LogDebug("No valid credentials for {ServiceIdentifier} found, clearing current auth", ServiceIdentifier);
            _currentAuthentication = null;
            LastResult = null;
            await OnAuthenticationStateChangedAsync(cancellationToken);
            return;
        }

        await AuthenticateAsync(serviceCredentials, cancellationToken);
        await OnAuthenticationStateChangedAsync(cancellationToken);
    }

    public async Task UnlinkAsync(CancellationToken cancellationToken)
    {
        var updatedPreferences = userPreferences.CurrentPreferences.RemoveCredentialsFor(ServiceIdentifier);
        await userPreferences.SaveAndApplyUserPreferencesAsync(updatedPreferences);
    }

    public virtual async Task<Result<ClaimsIdentity>> ConfigureAsync(AccountCredentials credentials, CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var previousAuthentication = _currentAuthentication;
            var authenticationResult = await AuthenticateAsync(credentials, cancellationToken);
            if (authenticationResult.IsFailed || !IsAuthenticated)
            {
                _currentAuthentication = previousAuthentication;
                return LastResult;
            }

            var updatedPreferences = userPreferences.CurrentPreferences.SetCredentials(credentials);
            await userPreferences.SaveAndApplyUserPreferencesAsync(updatedPreferences);
            await OnAuthenticationStateChangedAsync(cancellationToken);

            return LastResult;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

#pragma warning disable CS8774 // Member 'LastResult' must have a non-null value when exiting.
    [MemberNotNull(nameof(LastResult))]
    protected async Task<Result<ClaimsIdentity>> AuthenticateAsync(AccountCredentials credentials, CancellationToken cancellationToken)
    {
        _currentAuthentication = authenticator.AuthenticateAsync(credentials, cancellationToken);
        LastResult = await _currentAuthentication;
        return LastResult;
    }
#pragma warning restore CS8774

    protected virtual Task OnAuthenticationStateChangedAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    protected override async Task InitializeAsyncCore(CancellationToken cancellationToken)
    {
        userPreferences.ApplyPreferences += OnApplyPreferences;
        authenticator.RefreshRequested += AuthenticatorOnRefreshRequested;
        await UpdateAsync(cancellationToken);
    }

    private void AuthenticatorOnRefreshRequested(object? _, EventArgs args)
        => UpdateExternal();

    private void OnApplyPreferences(object? _, UserPreferences preferences)
        => UpdateExternal();

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    private async void UpdateExternal()
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
