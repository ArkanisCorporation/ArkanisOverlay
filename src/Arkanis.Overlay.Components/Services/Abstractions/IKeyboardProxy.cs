namespace Arkanis.Overlay.Components.Services.Abstractions;

using Common.Models.Keyboard;
using Microsoft.AspNetCore.Components.Web;

public interface IKeyboardProxy
{
    public event EventHandler<KeyboardKey> OnKeyUp;
    public event EventHandler<KeyboardKey> OnKeyDown;
    public event EventHandler<KeyboardShortcut> OnKeyboardShortcut;

    public void RegisterKeyUp(KeyboardEventArgs keyboardEvent);
    public void RegisterKeyDown(KeyboardEventArgs keyboardEvent);
    public void Clear();
}
