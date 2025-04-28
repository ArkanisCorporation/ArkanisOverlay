namespace Arkanis.Overlay.Components.Services;

using Domain.Models.Keyboard;

public class KeyboardProxy
{
    public event EventHandler<KeyboardKey>? OnKeyUp;
    public event EventHandler<KeyboardKey>? OnKeyDown;

    public void RegisterKeyUp(KeyboardKey keyboardKey)
        => OnKeyUp?.Invoke(this, keyboardKey);

    public void RegisterKeyDown(KeyboardKey keyboardKey)
        => OnKeyDown?.Invoke(this, keyboardKey);
}
