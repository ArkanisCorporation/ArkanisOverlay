namespace Arkanis.Overlay.UI.Native;

using Avalonia.Controls;

public interface IWindowUtils
{
    public void EnableClickThrough(Window window);
    public void DisableClickThrough(Window window);
}
