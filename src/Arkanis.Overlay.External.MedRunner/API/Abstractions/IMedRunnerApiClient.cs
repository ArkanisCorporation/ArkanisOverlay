namespace Arkanis.Overlay.External.MedRunner.API.Abstractions;

using Endpoints.Auth;
using Endpoints.ChatMessage;
using Endpoints.Client;
using Endpoints.Code;
using Endpoints.Emergency;
using Endpoints.OrgSettings;
using Endpoints.Staff;
using Endpoints.WebSocket;

/// <summary>
///     Interface for the Medrunner API client.
/// </summary>
public interface IMedRunnerApiClient
{
    EmergencyEndpoint Emergency { get; }
    ClientEndpoint Client { get; }
    StaffEndpoint Staff { get; }
    OrgSettingsEndpoint OrgSettings { get; }
    ChatMessageEndpoint ChatMessage { get; }
    CodeEndpoint Code { get; }
    AuthEndpoint Auth { get; }
    WebsocketEndpoint WebSocket { get; }
}
