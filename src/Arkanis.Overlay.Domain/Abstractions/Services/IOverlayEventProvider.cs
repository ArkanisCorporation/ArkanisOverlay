namespace Arkanis.Overlay.Domain.Abstractions.Services;

public interface IOverlayEventProvider
{
    public event EventHandler OverlayShown;
    public event EventHandler OverlayHidden;

    public event EventHandler OverlayFocused;
    public event EventHandler OverlayBlurred;

    public event EventHandler WindowFound;
}
