namespace Arkanis.Overlay.External.MedRunner.API.Abstractions;

using Models;

public interface IWebSocketEventProvider
{
    /// <summary>
    ///     Triggers when the current user is updated.
    /// </summary>
    event EventHandler<Person>? PersonUpdated;

    /// <summary>
    ///     Triggers when an emergency is created.
    /// </summary>
    event EventHandler<Emergency>? EmergencyCreated;

    /// <summary>
    ///     Triggers when an emergency is updated.
    /// </summary>
    event EventHandler<Emergency>? EmergencyUpdated;

    /// <summary>
    ///     Triggers when a chat message is added to an emergency where the user participates.
    /// </summary>
    event EventHandler<ChatMessage>? ChatMessageCreated;

    /// <summary>
    ///     Triggers when a chat message is updated.
    /// </summary>
    event EventHandler<ChatMessage>? ChatMessageUpdated;

    /// <summary>
    ///     Triggers when a team is created.
    /// </summary>
    event EventHandler<Team>? TeamCreated;

    /// <summary>
    ///     Triggers when a team is updated.
    /// </summary>
    event EventHandler<Team>? TeamUpdated;

    /// <summary>
    ///     Triggers when a team is deleted.
    /// </summary>
    event EventHandler<Team>? TeamDeleted;

    /// <summary>
    ///     Triggers when org settings are updated.
    /// </summary>
    event EventHandler<OrgSettings>? OrgSettingsUpdated;

    /// <summary>
    ///     Triggers when a new version of the portals is available.
    /// </summary>
    event EventHandler<Deployment>? DeploymentCreated;
}
