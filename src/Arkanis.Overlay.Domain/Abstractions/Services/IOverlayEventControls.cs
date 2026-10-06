namespace Arkanis.Overlay.Domain.Abstractions.Services;

public interface IOverlayEventControls
{
    public void OnFocusGained();
    public void OnFocusLost();

    public void OnOverlayWindowShown();
    public void OnOverlayWindowHidden();

    public void OnGameWindowFound();
    public void OnGameWindowLost();

    public void OnGameWindowFocused();
    public void OnGameWindowBlurred();
}
