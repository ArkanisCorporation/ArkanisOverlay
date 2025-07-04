namespace Arkanis.Overlay.External.MedRunner.API;

using Abstractions;
using Endpoints.Auth;
using Endpoints.ChatMessage;
using Endpoints.Client;
using Endpoints.Code;
using Endpoints.Emergency;
using Endpoints.OrgSettings;
using Endpoints.Staff;
using Endpoints.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

/// <summary>
///     An API client for basic client interactions with the Medrunner API.
/// </summary>
public class MedRunnerApiClient(
    ApiConfig config,
    IMedRunnerTokenProvider tokenProvider,
    IMemoryCache cache,
    ILogger<MedRunnerApiClient> logger
) : IMedRunnerApiClient
{
    public IMedRunnerTokenProvider TokenProvider { get; } = tokenProvider;

    public EmergencyEndpoint Emergency { get; } = new(config, tokenProvider, cache, logger);

    public ClientEndpoint Client { get; } = new(config, tokenProvider, cache, logger);

    public StaffEndpoint Staff { get; } = new(config, tokenProvider, cache, logger);

    public OrgSettingsEndpoint OrgSettings { get; } = new(config, tokenProvider, cache, logger);

    public ChatMessageEndpoint ChatMessage { get; } = new(config, tokenProvider, cache, logger);

    public CodeEndpoint Code { get; } = new(config, tokenProvider, cache, logger);

    public AuthEndpoint Auth { get; } = new(config, tokenProvider, cache, logger);

    public WebsocketEndpoint WebSocket { get; } = new(config, tokenProvider, cache, logger);
}
