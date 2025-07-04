namespace Arkanis.Overlay.External.MedRunner.Models;

using System.ComponentModel;

/// <summary>
///     Enum for threat levels.
/// </summary>
public enum ThreatLevel
{
    [Description("Unknown threat")]
    Unknown,

    [Description("Low threat")]
    Low,

    [Description("PvE threat")]
    Medium,

    [Description("PvP threat")]
    High,
}
