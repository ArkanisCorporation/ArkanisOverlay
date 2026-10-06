namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents a promotional code.
/// </summary>
public class PromotionalCode
{
    /// <summary>
    ///     The ID of the redeemer.
    /// </summary>
    public required string RedeemerId { get; set; }

    /// <summary>
    ///     The type of code.
    /// </summary>
    public CodeType Type { get; set; }
}

/// <summary>
///     The type of promotional code.
/// </summary>
public enum CodeType
{
    Unknown,
    CitizenCon2954,
}
