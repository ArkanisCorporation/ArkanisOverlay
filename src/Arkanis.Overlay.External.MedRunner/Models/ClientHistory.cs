namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents a client's emergency history.
/// </summary>
public class ClientHistory
{
    /// <summary>
    ///     The emergency id associated with this history record.
    /// </summary>
    public required string EmergencyId { get; set; }

    /// <summary>
    ///     The client id associated with this history record.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    ///     The timestamp when the emergency was created.
    /// </summary>
    public required DateTimeOffset EmergencyCreationTimestamp { get; set; }
}
