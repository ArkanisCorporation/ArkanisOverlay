namespace Arkanis.Overlay.Components.Extensions;

using Common.Models.Keyboard;
using Microsoft.AspNetCore.Components.Web;

public static class KeyboardEventArgsExtensions
{
    public static KeyboardKey GetKey(this KeyboardEventArgs @this)
        => JavaScriptKeyMap.ToEnum(@this.Code);
}
