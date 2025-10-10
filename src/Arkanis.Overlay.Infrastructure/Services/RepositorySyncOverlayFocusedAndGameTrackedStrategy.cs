namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions.Services;

public class RepositorySyncOverlayFocusedAndGameTrackedStrategy : RepositorySyncGameTrackedStrategy
{
    private bool _overlayIsFocused;

    public RepositorySyncOverlayFocusedAndGameTrackedStrategy(IOverlayEventProvider overlayEventProvider) : base(overlayEventProvider)
    {
        OverlayEventProvider.OverlayFocused += OnOverlayEventProviderOnOverlayFocused;
        OverlayEventProvider.OverlayBlurred += OnOverlayEventProviderOnOverlayBlurred;
    }

    public override bool ShouldUpdateNow
        => _overlayIsFocused && base.ShouldUpdateNow;

    public override void Dispose()
    {
        OverlayEventProvider.OverlayFocused -= OnOverlayEventProviderOnOverlayFocused;
        OverlayEventProvider.OverlayBlurred -= OnOverlayEventProviderOnOverlayBlurred;
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnOverlayEventProviderOnOverlayFocused(object? _, EventArgs e)
    {
        _overlayIsFocused = true;
        EmitCurrentStatus();
    }

    private void OnOverlayEventProviderOnOverlayBlurred(object? _, EventArgs e)
    {
        _overlayIsFocused = false;
        EmitCurrentStatus();
    }
}
