namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents a cached message reference.
/// </summary>
public class MessageCache
{
    /// <summary>
    ///     The message ID.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    ///     The channel ID.
    /// </summary>
    public required string ChannelId { get; set; }
}
