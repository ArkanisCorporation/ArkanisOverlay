namespace Arkanis.Overlay.Infrastructure.Services.External;

using Common.Abstractions;
using Microsoft.Extensions.Logging;
using Overlay.External.Regolith;
using Overlay.External.Regolith.Abstractions;

public class RegolithAccountContext(
    RegolithAuthenticator authenticator,
    IUserPreferencesManager userPreferences,
    IRegolithApiClient apiClient,
    ILogger<RegolithAccountContext> logger
) : ExternalAccountContext<RegolithAuthenticator.AuthenticationTask>(authenticator, userPreferences, logger)
{
    /// <summary>
    /// Gets the API key from the current authenticated session.
    /// </summary>
    public string? ApiKey => CurrentAuthentication?.ApiKey;

    protected override Task UpdateAsyncCore(CancellationToken cancellationToken)
    {
        // Apply the API key to the shared client instance whenever authentication state changes
        apiClient.SetApiKey(ApiKey);
        return base.UpdateAsyncCore(cancellationToken);
    }
}
