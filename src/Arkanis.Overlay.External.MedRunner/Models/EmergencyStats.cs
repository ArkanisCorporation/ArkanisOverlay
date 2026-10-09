namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents emergency statistics.
/// </summary>
public class EmergencyStats
{
    /// <summary>
    ///     The number of successful emergencies.
    /// </summary>
    public int Success { get; set; }

    /// <summary>
    ///     The number of failed emergencies.
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    ///     The number of no-contact emergencies.
    /// </summary>
    public int NoContact { get; set; }

    /// <summary>
    ///     The number of refused emergencies.
    /// </summary>
    public int Refused { get; set; }

    /// <summary>
    ///     The number of aborted emergencies.
    /// </summary>
    public int Aborted { get; set; }

    /// <summary>
    ///     The number of server error emergencies.
    /// </summary>
    public int ServerError { get; set; }

    /// <summary>
    ///     The number of canceled emergencies.
    /// </summary>
    public int Canceled { get; set; }
}
