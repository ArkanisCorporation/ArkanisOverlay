namespace Arkanis.Overlay.Domain.Abstractions.Services;

public interface IOverlayEventControls
{
    void SetFocus(bool isFocused = true);

    void SetGameConnected(bool isConnected = true);
}
