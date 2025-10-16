namespace Arkanis.Overlay.Infrastructure.Services.External;

using Common.Abstractions;
using Common.Services;
using Microsoft.Extensions.Logging;
using Overlay.External.CitizenId;

public class CitizenIdAccountContext(CitizenIdAuthenticator authenticator, IUserPreferencesManager userPreferences, ILogger<UexAccountContext> logger)
    : ExternalAccountContext<OidcAuthenticator.AuthenticationTask>(authenticator, userPreferences, logger);
