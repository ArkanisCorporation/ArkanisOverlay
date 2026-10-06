namespace Arkanis.Overlay.Domain.Abstractions.Services;

public interface IOverlayEventProvider
{
    public event EventHandler OverlayShown;
    public event EventHandler OverlayHidden;

    public event EventHandler OverlayFocused;
    public event EventHandler OverlayBlurred;

    public event EventHandler GameWindowFocused;
    public event EventHandler GameWindowBlurred;

    public event EventHandler GameWindowFound;
    public event EventHandler GameWindowLost;
}
