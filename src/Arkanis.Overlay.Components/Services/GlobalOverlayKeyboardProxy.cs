namespace Arkanis.Overlay.Components.Services;

using Domain.Abstractions.Services;
using Microsoft.Extensions.Logging;

public class GlobalOverlayKeyboardProxy : KeyboardProxy
{
    private readonly IOverlayEventProvider _overlay;

    public GlobalOverlayKeyboardProxy(IOverlayEventProvider overlay, ILogger<GlobalOverlayKeyboardProxy> logger) : base(logger)
    {
        _overlay = overlay;
        _overlay.OverlayShown += OnOverlayVisibilityChanged;
        _overlay.OverlayHidden += OnOverlayVisibilityChanged;
        _overlay.OverlayFocused += OnOverlayVisibilityChanged;
        _overlay.OverlayBlurred += OnOverlayVisibilityChanged;
    }

    private void OnOverlayVisibilityChanged(object? sender, EventArgs e)
        => ShortcutBuilder.Clear();

    public override void Dispose()
    {
        _overlay.OverlayShown -= OnOverlayVisibilityChanged;
        _overlay.OverlayHidden -= OnOverlayVisibilityChanged;
        _overlay.OverlayFocused -= OnOverlayVisibilityChanged;
        _overlay.OverlayBlurred -= OnOverlayVisibilityChanged;

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
