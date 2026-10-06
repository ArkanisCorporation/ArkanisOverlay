namespace Arkanis.Overlay.External.MedRunner.Models;

using System.Text.Json.Serialization;

/// <summary>
///     Represents a chat message.
/// </summary>
public class ChatMessage : ModelBase
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
    ///     The ISO date and time at which the message was sent.
    /// </summary>
    public DateTimeOffset MessageSentTimestamp { get; set; }

    public DateTimeOffset SentAt
        => MessageSentTimestamp;

    /// <summary>
    ///     The contents of the message.
    /// </summary>
    [JsonPropertyName("contents")]
    public required string Content { get; set; }
}
