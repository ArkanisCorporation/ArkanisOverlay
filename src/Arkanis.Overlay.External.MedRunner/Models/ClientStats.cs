namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents client stats for a person.
/// </summary>
public class ClientStats
{
    /// <summary>
    ///     Emergency mission statistics for the client.
    /// </summary>
    public required EmergencyStats Missions { get; set; }
}
