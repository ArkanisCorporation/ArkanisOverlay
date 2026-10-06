namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Emergency.Request;

using Models;

/// <summary>
///     Request body for creating a new emergency.
/// </summary>
public class CreateEmergencyRequest
{
    /// <summary>
    ///     The location of the emergency
    /// </summary>
    public required Location Location { get; set; }

    /// <summary>
    ///     The threat level of the emergency
    /// </summary>
    public ThreatLevel ThreatLevel { get; set; }

    /// <summary>
    ///     The rsiHandle of the client (optional)
    /// </summary>
    public string? RsiHandle { get; set; }
}

/// <summary>
///     Only real matching locations will be accepted (see /emergency/meta/locations).
/// </summary>
public class Location
{
    /// <summary>
    ///     The star system the emergency is located in
    /// </summary>
    public required string System { get; set; }

    /// <summary>
    ///     The nearest planetary body to the emergency
    /// </summary>
    public required string Subsystem { get; set; }

    /// <summary>
    ///     The nearest moon to the emergency, if applicable
    /// </summary>
    public string? TertiaryLocation { get; set; }
}
