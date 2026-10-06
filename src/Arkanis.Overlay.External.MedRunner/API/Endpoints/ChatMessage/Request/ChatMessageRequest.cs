namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.ChatMessage.Request;

/// <summary>
///     Request body for creating a new chat message.
/// </summary>
public class ChatMessageRequest
{
    /// <summary>
    ///     The id of the emergency associated with the message
    /// </summary>
    public required string EmergencyId { get; set; }

    /// <summary>
    ///     The message contents
    /// </summary>
    public required string Contents { get; set; }
}
