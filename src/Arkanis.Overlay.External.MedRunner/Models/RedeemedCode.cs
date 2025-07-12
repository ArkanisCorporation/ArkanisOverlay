namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents a redeemed code for a person.
/// </summary>
public class RedeemedCode
{
    /// <summary>
    ///     The code value.
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    ///     The type of code.
    /// </summary>
    public CodeType Type { get; set; }
}
