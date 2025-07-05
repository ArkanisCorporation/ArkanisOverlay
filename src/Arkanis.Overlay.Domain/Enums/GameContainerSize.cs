namespace Arkanis.Overlay.Domain.Enums;

using System.ComponentModel;

public enum GameContainerSize
{
    Unknown = 0,

    [Description("1 SCU")]
    One = 1,

    [Description("2 SCU")]
    Two = 2,

    [Description("4 SCU")]
    Four = 4,

    [Description("8 SCU")]
    Eight = 8,

    [Description("16 SCU")]
    Sixteen = 16,

    [Description("24 SCU")]
    TwentyFour = 24,

    [Description("32 SCU")]
    ThirtyTwo = 32,
}
