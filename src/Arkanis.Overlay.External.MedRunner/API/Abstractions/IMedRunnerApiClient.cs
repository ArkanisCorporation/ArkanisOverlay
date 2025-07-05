namespace Arkanis.Overlay.External.MedRunner.API.Abstractions;

using Endpoints;

/// <summary>
///     The MedRunner API client interface.
/// </summary>
public interface IMedRunnerApiClient
{
    IMedRunnerTokenProvider TokenProvider { get; }

    /// <summary>
    ///     Gets the authentication endpoint.
    /// </summary>
    IAuthEndpoint Auth { get; }

    /// <summary>
    ///     Gets the chat message endpoint.
    /// </summary>
    IChatMessageEndpoint ChatMessage { get; }

    /// <summary>
    ///     Gets the client endpoint.
    /// </summary>
    IClientEndpoint Client { get; }

    /// <summary>
    ///     Gets the code endpoint.
    /// </summary>
    ICodeEndpoint Code { get; }

    /// <summary>
    ///     Gets the emergency endpoint.
    /// </summary>
    IEmergencyEndpoint Emergency { get; }

    /// <summary>
    ///     Gets the organization settings endpoint.
    /// </summary>
    IOrgSettingsEndpoint OrgSettings { get; }

    /// <summary>
    ///     Gets the staff endpoint.
    /// </summary>
    IStaffEndpoint Staff { get; }

    /// <summary>
    ///     Gets the websocket endpoint.
    /// </summary>
    IWebSocketEndpoint WebSocket { get; }
}
