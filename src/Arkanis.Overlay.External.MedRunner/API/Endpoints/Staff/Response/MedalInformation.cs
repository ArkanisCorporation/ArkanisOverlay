namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Staff.Response;

using Models;

/// <summary>
///     Information about a medal.
/// </summary>
public class MedalInformation
{
    /// <summary>
    ///     The level linked to the medal.
    /// </summary>
    public Level Level { get; set; }

    /// <summary>
    ///     The number of successful missions required to earn the medal.
    /// </summary>
    public int SuccessfulMissions { get; set; }
}
