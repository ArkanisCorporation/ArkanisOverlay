namespace Arkanis.Overlay.Infrastructure.Services.External;

using Domain.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Overlay.External.UEX;

public class UexAccountContext(UexAuthenticator authenticator, IUserPreferencesManager userPreferences, ILogger<UexAccountContext> logger)
    : ExternalAccountContext<UexAuthenticator.AuthenticationTask>(authenticator, userPreferences, logger);
