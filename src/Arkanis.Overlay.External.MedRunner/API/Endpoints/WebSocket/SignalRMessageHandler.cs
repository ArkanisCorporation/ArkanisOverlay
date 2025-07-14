namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.WebSocket;

using System.Diagnostics.CodeAnalysis;
using Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Models;

public class SignalRMessageHandler : IWebSocketEventProvider
{
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
        _connection.On<Person>(PersonUpdatedEvent, OnPersonUpdateHandler);
        _connection.On<Emergency>(EmergencyCreatedEvent, OnEmergencyCreateHandler);
        _connection.On<Emergency>(EmergencyUpdatedEvent, OnEmergencyUpdateHandler);
        _connection.On<ChatMessage>(ChatMessageCreatedEvent, OnChatMessageCreateHandler);
        _connection.On<ChatMessage>(ChatMessageUpdatedEvent, OnChatMessageUpdateHandler);
        _connection.On<Team>(TeamCreatedEvent, OnTeamCreateHandler);
        _connection.On<Team>(TeamUpdatedEvent, OnTeamUpdateHandler);
        _connection.On<Team>(TeamDeletedEvent, OnTeamDeleteHandler);
        _connection.On<OrgSettings>(OrgSettingsUpdatedEvent, OnOrgSettingsUpdateHandler);
        _connection.On<Deployment>(DeploymentCreatedEvent, OnDeploymentCreateHandler);
    }

    private void OnPersonUpdateHandler(Person person)
        => PersonUpdated?.Invoke(this, person);

    private void OnEmergencyCreateHandler(Emergency emergency)
        => EmergencyCreated?.Invoke(this, emergency);

    private void OnEmergencyUpdateHandler(Emergency emergency)
        => EmergencyUpdated?.Invoke(this, emergency);

    private void OnChatMessageCreateHandler(ChatMessage message)
        => ChatMessageCreated?.Invoke(this, message);

    private void OnChatMessageUpdateHandler(ChatMessage message)
        => ChatMessageUpdated?.Invoke(this, message);

    private void OnTeamCreateHandler(Team team)
        => TeamCreated?.Invoke(this, team);

    private void OnTeamUpdateHandler(Team team)
        => TeamUpdated?.Invoke(this, team);

    private void OnTeamDeleteHandler(Team team)
        => TeamDeleted?.Invoke(this, team);

    private void OnOrgSettingsUpdateHandler(OrgSettings orgSettings)
        => OrgSettingsUpdated?.Invoke(this, orgSettings);

    private void OnDeploymentCreateHandler(Deployment deployment)
        => DeploymentCreated?.Invoke(this, deployment);
}
