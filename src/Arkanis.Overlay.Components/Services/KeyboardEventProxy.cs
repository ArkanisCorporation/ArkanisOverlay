namespace Arkanis.Overlay.Components.Services;

using Microsoft.AspNetCore.Components.Web;

public class KeyboardEventProxy
{
    public event EventHandler<KeyboardEventArgs>? OnKeyUp;
    public event EventHandler<KeyboardEventArgs>? OnKeyDown;

    public void RegisterKeyUp(KeyboardEventArgs e)
        => OnKeyUp?.Invoke(this, e);

    public void RegisterKeyDown(KeyboardEventArgs e)
        => OnKeyDown?.Invoke(this, e);
}
