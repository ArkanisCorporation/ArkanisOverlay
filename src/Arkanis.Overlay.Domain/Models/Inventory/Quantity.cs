namespace Arkanis.Overlay.Domain.Models.Inventory;

using System.ComponentModel;

public record Quantity(long Amount, Quantity.Type Unit)
{
    public enum Type
    {
        Undefined,

        [Description("cSCU")]
        CentiStandardCargoUnit,

        [Description("SCU")]
        StandardCargoUnit,
        Count,
    }

    public long Amount { get; set; } = Amount;
}
