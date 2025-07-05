namespace Arkanis.Overlay.External.MedRunner.API;

using Abstractions;
using Abstractions.Endpoints;

/// <summary>
///     The live MedRunner API client.
/// </summary>
public class MedRunnerApiClient(
    IMedRunnerTokenProvider tokenProvider,
    IEmergencyEndpoint emergencyEndpoint,
    IClientEndpoint clientEndpoint,
    IStaffEndpoint staffEndpoint,
    IOrgSettingsEndpoint orgSettingsEndpoint,
    IChatMessageEndpoint chatMessageEndpoint,
    ICodeEndpoint codeEndpoint,
    IAuthEndpoint authEndpoint,
    IWebsocketEndpoint webSocketEndpoint
) : IMedRunnerApiClient
{
    /// <inheritdoc />
    public IMedRunnerTokenProvider TokenProvider { get; } = tokenProvider;

    /// <inheritdoc />
    public IEmergencyEndpoint Emergency { get; } = emergencyEndpoint;

    /// <inheritdoc />
    public IClientEndpoint Client { get; } = clientEndpoint;

    /// <inheritdoc />
    public IStaffEndpoint Staff { get; } = staffEndpoint;

    /// <inheritdoc />
    public IOrgSettingsEndpoint OrgSettings { get; } = orgSettingsEndpoint;

    /// <inheritdoc />
    public IChatMessageEndpoint ChatMessage { get; } = chatMessageEndpoint;

    /// <inheritdoc />
    public ICodeEndpoint Code { get; } = codeEndpoint;

    /// <inheritdoc />
    public IAuthEndpoint Auth { get; } = authEndpoint;

    /// <inheritdoc />
    public IWebsocketEndpoint WebSocket { get; } = webSocketEndpoint;
}
