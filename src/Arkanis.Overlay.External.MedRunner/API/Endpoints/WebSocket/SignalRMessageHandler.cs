namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.WebSocket;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Models;

public class SignalRMessageHandler(ILogger? logger = null) : IWebSocketEventProvider
{
    private readonly ILogger _logger = logger ?? NullLogger.Instance;
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    private const string PersonUpdatedEvent = "PersonUpdate";
    private const string EmergencyCreatedEvent = "EmergencyCreate";
    private const string EmergencyUpdatedEvent = "EmergencyUpdate";
    private const string ChatMessageCreatedEvent = "ChatMessageCreate";
    private const string ChatMessageUpdatedEvent = "ChatMessageUpdate";
    private const string TeamCreatedEvent = "TeamCreate";
    private const string TeamUpdatedEvent = "TeamUpdate";
    private const string TeamDeletedEvent = "TeamDelete";
    private const string OrgSettingsUpdatedEvent = "OrgSettingsUpdate";
    private const string DeploymentCreatedEvent = "DeploymentCreate";

    private HubConnection? _connection;

    [MemberNotNullWhen(true, nameof(_connection))]
    public bool IsConnected
        => _connection is not null;

    /// <inheritdoc />
    public event EventHandler<Person>? PersonUpdated;

    /// <inheritdoc />
    public event EventHandler<Emergency>? EmergencyCreated;

    /// <inheritdoc />
    public event EventHandler<Emergency>? EmergencyUpdated;

    /// <inheritdoc />
    public event EventHandler<ChatMessage>? ChatMessageCreated;

    /// <inheritdoc />
    public event EventHandler<ChatMessage>? ChatMessageUpdated;

    /// <inheritdoc />
    public event EventHandler<Team>? TeamCreated;

    /// <inheritdoc />
    public event EventHandler<Team>? TeamUpdated;

    /// <inheritdoc />
    public event EventHandler<Team>? TeamDeleted;

    /// <inheritdoc />
    public event EventHandler<OrgSettings>? OrgSettingsUpdated;

    /// <inheritdoc />
    public event EventHandler<Deployment>? DeploymentCreated;

    public void Connect(HubConnection connection)
    {
        _connection = connection;
        foreach (var eventName in new[]
                 {
                     PersonUpdatedEvent, EmergencyCreatedEvent, EmergencyUpdatedEvent,
                     ChatMessageCreatedEvent, ChatMessageUpdatedEvent, TeamCreatedEvent, TeamUpdatedEvent,
                     TeamDeletedEvent, OrgSettingsUpdatedEvent, DeploymentCreatedEvent,
                 })
        {
            // Bind raw JSON so a model mismatch cannot discard the payload before it is logged.
            _connection.On<JsonElement>(eventName, payload => HandleMessage(eventName, payload));
        }
    }

    public void Disconnect(HubConnection connection)
    {
        if (ReferenceEquals(_connection, connection))
        {
            connection.Remove(PersonUpdatedEvent);
            connection.Remove(EmergencyCreatedEvent);
            connection.Remove(EmergencyUpdatedEvent);
            connection.Remove(ChatMessageCreatedEvent);
            connection.Remove(ChatMessageUpdatedEvent);
            connection.Remove(TeamCreatedEvent);
            connection.Remove(TeamUpdatedEvent);
            connection.Remove(TeamDeletedEvent);
            connection.Remove(OrgSettingsUpdatedEvent);
            connection.Remove(DeploymentCreatedEvent);
            _connection = null;
        }
    }

    public void HandleMessage(string eventName, JsonElement payload)
    {
        var json = payload.GetRawText();
        _logger.LogDebug("Received Medrunner real-time JSON for {EventName}: {Json}", eventName, json);
        try
        {
            switch (eventName)
            {
                case PersonUpdatedEvent: Dispatch(payload, PersonUpdated); break;
                case EmergencyCreatedEvent: Dispatch(payload, EmergencyCreated); break;
                case EmergencyUpdatedEvent: Dispatch(payload, EmergencyUpdated); break;
                case ChatMessageCreatedEvent: Dispatch(payload, ChatMessageCreated); break;
                case ChatMessageUpdatedEvent: Dispatch(payload, ChatMessageUpdated); break;
                case TeamCreatedEvent: Dispatch(payload, TeamCreated); break;
                case TeamUpdatedEvent: Dispatch(payload, TeamUpdated); break;
                case TeamDeletedEvent: Dispatch(payload, TeamDeleted); break;
                case OrgSettingsUpdatedEvent: Dispatch(payload, OrgSettingsUpdated); break;
                case DeploymentCreatedEvent: Dispatch(payload, DeploymentCreated); break;
            }
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Failed to decode Medrunner real-time event {EventName}. Incoming JSON: {Json}", eventName, json);
        }
    }

    private void Dispatch<T>(JsonElement payload, EventHandler<T>? handler)
    {
        var message = payload.Deserialize<T>(Options);
        if (message is null)
        {
            throw new JsonException($"Expected a Medrunner {typeof(T).Name} object, received null.");
        }

        handler?.Invoke(this, message);
    }
}
