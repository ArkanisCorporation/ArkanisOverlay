namespace Arkanis.Overlay.External.MedRunner.API;

using Abstractions;
using Abstractions.Endpoints;
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
///     The MedRunner API client.
/// </summary>
public class MedRunnerApiClient(
    ApiConfig config,
    IMedRunnerTokenProvider tokenProvider,
    IMemoryCache cache,
    ILogger<MedRunnerApiClient> logger
) : IMedRunnerApiClient
{
    public IMedRunnerTokenProvider TokenProvider { get; } = tokenProvider;

    public IEmergencyEndpoint Emergency { get; } = new EmergencyEndpoint(config, tokenProvider, cache, logger);

    public IClientEndpoint Client { get; } = new ClientEndpoint(config, tokenProvider, cache, logger);

    public IStaffEndpoint Staff { get; } = new StaffEndpoint(config, tokenProvider, cache, logger);

    public IOrgSettingsEndpoint OrgSettings { get; } = new OrgSettingsEndpoint(config, tokenProvider, cache, logger);

    public IChatMessageEndpoint ChatMessage { get; } = new ChatMessageEndpoint(config, tokenProvider, cache, logger);

    public ICodeEndpoint Code { get; } = new CodeEndpoint(config, tokenProvider, cache, logger);

    public IAuthEndpoint Auth { get; } = new AuthEndpoint(config, tokenProvider, cache, logger);

    public IWebsocketEndpoint WebSocket { get; } = new WebSocketEndpoint(config, tokenProvider, cache, logger);
}
