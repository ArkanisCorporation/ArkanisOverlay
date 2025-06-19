namespace Arkanis.Overlay.Domain.Enums;

using System.ComponentModel;

public enum TerminalInventoryStatus
{
    Unknown = 0,

    [Description("Out of Stock (Empty)")]
    OutOfStock = 1,

    [Description("Very Low Inventory")]
    VeryLow = 2,

    [Description("Low Inventory")]
    Low = 3,

    [Description("Medium Inventory")]
    Medium = 4,

    [Description("High Inventory")]
    High = 5,

    [Description("Very High Inventory")]
    VeryHigh = 6,

    [Description("Maximum Inventory (No Demand)")]
    Maximum = 7,
}
