namespace Arkanis.Overlay.External.MedRunner.API.Mocks;

using Abstractions;
using Models;

/// <summary>
///     Mock implementation of <see cref="IWebSocketEventProvider" /> for testing purposes.
///     Provides methods to manually trigger each event.
/// </summary>
public class MockWebSocketEventProvider : IWebSocketEventProvider
{
    public event EventHandler<Person>? PersonUpdated;
    public event EventHandler<Emergency>? EmergencyCreated;
    public event EventHandler<Emergency>? EmergencyUpdated;
    public event EventHandler<ChatMessage>? ChatMessageCreated;
    public event EventHandler<ChatMessage>? ChatMessageUpdated;
    public event EventHandler<Team>? TeamCreated;
    public event EventHandler<Team>? TeamUpdated;
    public event EventHandler<Team>? TeamDeleted;
    public event EventHandler<OrgSettings>? OrgSettingsUpdated;
    public event EventHandler<Deployment>? DeploymentCreated;

    /// <summary>
    ///     Invokes the PersonUpdated event.
    /// </summary>
    public void SendPersonUpdate(Person person)
        => PersonUpdated?.Invoke(this, person);

    /// <summary>
    ///     Invokes the EmergencyCreated event.
    /// </summary>
    public void SendNewEmergency(Emergency emergency)
        => EmergencyCreated?.Invoke(this, emergency);

    /// <summary>
    ///     Invokes the EmergencyUpdated event.
    /// </summary>
    public void SendEmergencyUpdate(Emergency emergency)
        => EmergencyUpdated?.Invoke(this, emergency);

    /// <summary>
    ///     Invokes the ChatMessageCreated event.
    /// </summary>
    public void SendNewChatMessage(ChatMessage message)
        => ChatMessageCreated?.Invoke(this, message);

    /// <summary>
    ///     Invokes the ChatMessageUpdated event.
    /// </summary>
    public void SendChatMessageUpdate(ChatMessage message)
        => ChatMessageUpdated?.Invoke(this, message);

    /// <summary>
    ///     Invokes the TeamCreated event.
    /// </summary>
    public void SendNewTeam(Team team)
        => TeamCreated?.Invoke(this, team);

    /// <summary>
    ///     Invokes the TeamUpdated event.
    /// </summary>
    public void SendTeamUpdate(Team team)
        => TeamUpdated?.Invoke(this, team);

    /// <summary>
    ///     Invokes the TeamDeleted event.
    /// </summary>
    public void SendTeamDelete(Team team)
        => TeamDeleted?.Invoke(this, team);

    /// <summary>
    ///     Invokes the OrgSettingsUpdated event.
    /// </summary>
    public void SendOrgSettingsUpdate(OrgSettings settings)
        => OrgSettingsUpdated?.Invoke(this, settings);

    /// <summary>
    ///     Invokes the DeploymentCreated event.
    /// </summary>
    public void SendNewDeployment(Deployment deployment)
        => DeploymentCreated?.Invoke(this, deployment);
}
