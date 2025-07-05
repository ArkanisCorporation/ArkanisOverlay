namespace Arkanis.Overlay.Domain.Enums;

using System.ComponentModel;

public enum GamePadSize
{
    Unknown,

    [Description("Extra Small")]
    Xs,

    [Description("Small")]
    S,

    [Description("Medium")]
    M,

    [Description("Large")]
    L,

    [Description("Extra Large")]
    Xl,
}
