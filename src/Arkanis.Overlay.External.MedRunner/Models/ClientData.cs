namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents client data for an emergency.
/// </summary>
public class ClientData
{
    public required string RsiHandle { get; set; }
    public required string RsiProfileLink { get; set; }
    public bool GotClientData { get; set; }
    public bool RedactedOrgOnProfile { get; set; }
    public bool Reported { get; set; }
}
