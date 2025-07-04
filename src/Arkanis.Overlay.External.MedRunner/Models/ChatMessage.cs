namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents a chat message.
/// </summary>
public class ChatMessage
{
    /// <summary>
    ///     The emergency associated with the chat message.
    /// </summary>
    public required string EmergencyId { get; set; }

    /// <summary>
    ///     The user id of the message sender.
    /// </summary>
    public required string SenderId { get; set; }

    /// <summary>
    ///     The timestamp at which the message was sent in Unix seconds.
    /// </summary>
    public long MessageSentTimestamp { get; set; }

    /// <summary>
    ///     The contents of the message.
    /// </summary>
    public required string Content { get; set; }
}
