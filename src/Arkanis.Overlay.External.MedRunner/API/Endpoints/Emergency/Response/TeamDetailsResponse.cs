namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Emergency.Response;

using Models;

/// <summary>
///     Details about a team responding to an alert.
/// </summary>
public class TeamDetailsResponse
{
    /// <summary>
    ///     Details about each individual responder.
    /// </summary>
    public required List<ResponderDetails> Stats { get; set; }

    /// <summary>
    ///     The aggregated mission success rate from all responders, appropriately weighted by number of missions.
    /// </summary>
    public double AggregatedSuccessRate { get; set; }
}

/// <summary>
///     Details about an alert responder.
/// </summary>
public class ResponderDetails
{
    /// <summary>
    ///     The responder's id.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    ///     The responder's level.
    /// </summary>
    public Level Level { get; set; }

    /// <summary>
    ///     The success rate of all prior missions this staff member has responded to.
    /// </summary>
    public double MissionSuccessRate { get; set; }

    /// <summary>
    ///     The success rate of all prior missions this staff member has acted as a dispatcher for.
    /// </summary>
    public double DispatchSuccessRate { get; set; }
}
