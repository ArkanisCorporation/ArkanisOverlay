namespace Arkanis.Overlay.Infrastructure.UnitTests.External.MedRunner;

using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Arkanis.Overlay.Common.Models;
using Arkanis.Overlay.Common.Options;
using Arkanis.Overlay.Common.Services;
using Arkanis.Overlay.Domain.Abstractions.Services;
using Arkanis.Overlay.External.MedRunner;
using Arkanis.Overlay.External.MedRunner.API;
using Arkanis.Overlay.External.MedRunner.API.Abstractions;
using Arkanis.Overlay.External.MedRunner.API.Abstractions.Endpoints;
using Arkanis.Overlay.External.MedRunner.API.Mocks;
using Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;
using Arkanis.Overlay.External.MedRunner.Models;
using Arkanis.Overlay.Infrastructure.Services.External;
using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

public sealed class MedRunnerAccountContextTests
{
    [Fact]
    public async Task AuthenticatedContextWithoutAccountMetadataDoesNotAdvertiseServiceAccess()
    {
        var authentication = new MedRunnerTokenAuthentication
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "KronnY")], "MedRunner"),
        };
        var tokenProvider = new StaticTokenProvider(authentication);
        var authenticator = new MedRunnerAuthenticator(tokenProvider);
        var authenticationTask = authenticator.AuthenticateAsync(
            new AccountApiTokenCredentials("MedRunner")
            {
                SecretToken = "api-token",
            },
            CancellationToken.None
        );
        await authenticationTask;

        var context = new MedRunnerAccountContext(authenticator, tokenProvider, null!, null!, NullLogger<MedRunnerAccountContext>.Instance);
        typeof(ExternalAccountContext)
            .GetField("_currentAuthentication", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(context, authenticationTask);

        context.IsAuthenticated.ShouldBeTrue();
        context.CanClientUseServices.ShouldBeFalse();
    }

    [Fact]
    public async Task InitializesPersistedCredentialsBeforeStartingRealTimeUpdates()
    {
        var authentication = new MedRunnerTokenAuthentication
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "KronnY")], "MedRunner"),
        };
        var tokenProvider = new StaticTokenProvider(authentication);
        var webSocket = new AuthenticatedWebSocketEndpoint(tokenProvider);
        var apiClient = new MedRunnerApiClient(
            tokenProvider,
            null!,
            new MockClientEndpoint(tokenProvider),
            null!,
            new MockOrgSettingsEndpoint(tokenProvider),
            null!,
            null!,
            null!,
            webSocket
        );
        var context = new MedRunnerAccountContext(
            new MedRunnerAuthenticator(tokenProvider),
            tokenProvider,
            apiClient,
            new StaticPreferencesManager(
                new UserPreferences
                {
                    ExternalServiceCredentials =
                    [
                        new AccountApiTokenCredentials("MedRunner")
                        {
                            SecretToken = "persisted-api-token",
                        },
                    ],
                }
            ),
            NullLogger<MedRunnerAccountContext>.Instance
        );

        await context.InitializeAsync(CancellationToken.None);

        context.IsAuthenticated.ShouldBeTrue();
        webSocket.Initialized.ShouldBeTrue();
        webSocket.Disconnected.ShouldBeTrue();
    }

    [Fact]
    public async Task IncompleteAccountMetadataReportsALocalProcessingRestriction()
    {
        var context = await CreateAuthenticatedContextAsync();

        context.ServiceAccessState.CanUseServices.ShouldBeFalse();
        context.ServiceAccessState.Restriction.ShouldBe(MedRunnerServiceAccessRestriction.LocalProcessing);
    }

    [Fact]
    public async Task BlockedAccountReportsAnAccountRestriction()
    {
        var context = await CreateAuthenticatedContextAsync(
            ActivePerson(),
            new ClientBlockedStatus
            {
                Blocked = true,
            }
        );

        context.ServiceAccessState.CanUseServices.ShouldBeFalse();
        context.ServiceAccessState.Restriction.ShouldBe(MedRunnerServiceAccessRestriction.Account);
    }

    [Fact]
    public async Task DisabledEmergencyRequestsReportAnOrganizationRestriction()
    {
        var context = await CreateAuthenticatedContextAsync(
            ActivePerson(),
            new ClientBlockedStatus(),
            new PublicOrgSettings
            {
                Status = ServiceStatus.Healthy,
                EmergenciesEnabled = false,
                AnonymousAlertsEnabled = false,
                RegistrationEnabled = false,
                LocationSettings = new LocationSettings
                {
                    Locations = [],
                },
            }
        );

        context.ServiceAccessState.CanUseServices.ShouldBeFalse();
        context.ServiceAccessState.Restriction.ShouldBe(MedRunnerServiceAccessRestriction.Organization);
    }

    private sealed class StaticTokenProvider(MedRunnerTokenAuthentication authentication) : IMedRunnerTokenProvider
    {
        public ClaimsIdentity? Identity { get; private set; }

        public bool IsAuthenticated
            => Identity is not null;

        public Task<string?> GetAccessTokenAsync()
            => Task.FromResult(IsAuthenticated ? authentication.AccessToken : null);

        public Task<string?> GetAccessTokenAsync(string source)
            => GetAccessTokenAsync();

        public Task<Result<MedRunnerTokenAuthentication>> AuthenticateApiTokenAsync(string apiToken, CancellationToken cancellationToken)
            => Task.FromResult(Result.Ok(authentication));

        public void ApplyAuthentication(MedRunnerTokenAuthentication authentication)
        {
            Identity = authentication.Identity;
        }

        public void ClearAuthentication()
        {
            Identity = null;
        }
    }

    private sealed class StaticPreferencesManager(UserPreferences currentPreferences) : IUserPreferencesManager
    {
        public UserPreferences CurrentPreferences { get; private set; } = currentPreferences;

        public event EventHandler<UserPreferences>? ApplyPreferences;

        public event EventHandler<UserPreferences>? UpdatePreferences;

        public Task LoadUserPreferencesAsync()
            => Task.CompletedTask;

        public Task SaveAndApplyUserPreferencesAsync(UserPreferences userPreferences)
        {
            CurrentPreferences = userPreferences;
            ApplyPreferences?.Invoke(this, userPreferences);
            return Task.CompletedTask;
        }
    }

    private sealed class AuthenticatedWebSocketEndpoint(StaticTokenProvider tokenProvider) : IWebSocketEndpoint
    {
        public IWebSocketEventProvider Events { get; } = new MockWebSocketEventProvider();

        public bool Initialized { get; private set; }

        public bool Disconnected { get; private set; }

        public Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            tokenProvider.IsAuthenticated.ShouldBeTrue();
            Initialized = true;
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken)
        {
            Disconnected = true;
            return Task.CompletedTask;
        }
    }

    private static async Task<MedRunnerAccountContext> CreateAuthenticatedContextAsync(
        Person? clientInfo = null,
        ClientBlockedStatus? clientStatus = null,
        PublicOrgSettings? publicSettings = null
    )
    {
        var authentication = new MedRunnerTokenAuthentication
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "KronnY")], "MedRunner"),
        };
        var authenticator = new MedRunnerAuthenticator(new StaticTokenProvider(authentication));
        var authenticationTask = authenticator.AuthenticateAsync(
            new AccountApiTokenCredentials("MedRunner")
            {
                SecretToken = "api-token",
            },
            CancellationToken.None
        );
        await authenticationTask;

        var context = new MedRunnerAccountContext(authenticator, null!, null!, null!, NullLogger<MedRunnerAccountContext>.Instance);
        typeof(ExternalAccountContext)
            .GetField("_currentAuthentication", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(context, authenticationTask);
        typeof(MedRunnerAccountContext)
            .GetProperty(nameof(MedRunnerAccountContext.ClientInfo))!
            .SetValue(context, clientInfo);
        typeof(MedRunnerAccountContext)
            .GetProperty(nameof(MedRunnerAccountContext.ClientStatus))!
            .SetValue(context, clientStatus);
        typeof(MedRunnerAccountContext)
            .GetProperty(nameof(MedRunnerAccountContext.PublicSettings))!
            .SetValue(context, publicSettings ?? AvailablePublicSettings());

        return context;
    }

    private static PublicOrgSettings AvailablePublicSettings()
        => new()
        {
            Status = ServiceStatus.Healthy,
            EmergenciesEnabled = true,
            AnonymousAlertsEnabled = false,
            RegistrationEnabled = false,
            LocationSettings = new LocationSettings
            {
                Locations = [],
            },
        };

    private static Person ActivePerson()
        => new()
        {
            Id = "test-client",
            DiscordId = "test-discord",
            ClientStats = new ClientStats
            {
                Missions = new EmergencyStats(),
            },
            ClientPortalPreferences = JsonDocument.Parse("{}"),
            Active = true,
        };
}
