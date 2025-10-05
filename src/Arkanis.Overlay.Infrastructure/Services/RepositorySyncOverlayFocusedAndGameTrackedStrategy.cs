namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions.Services;

public sealed class RepositorySyncOverlayFocusedAndGameTrackedStrategy : IRepositorySyncStrategy, IDisposable
{
    private readonly IOverlayEventProvider _overlayEventProvider;

    private bool _gameIsTracked;
    private bool _overlayIsFocused;

    public RepositorySyncOverlayFocusedAndGameTrackedStrategy(IOverlayEventProvider overlayEventProvider)
    {
        _overlayEventProvider = overlayEventProvider;
        _overlayEventProvider.OverlayFocused += OnOverlayEventProviderOnOverlayFocused;
        _overlayEventProvider.OverlayBlurred += OnOverlayEventProviderOnOverlayBlurred;
        _overlayEventProvider.GameWindowFound += OnOverlayEventProviderOnGameTracked;
        _overlayEventProvider.GameWindowLost += OnOverlayEventProviderOnGameUntracked;
    }

    public void Dispose()
    {
        _overlayEventProvider.OverlayFocused -= OnOverlayEventProviderOnOverlayFocused;
        _overlayEventProvider.OverlayBlurred -= OnOverlayEventProviderOnOverlayBlurred;
        _overlayEventProvider.GameWindowFound -= OnOverlayEventProviderOnGameTracked;
        _overlayEventProvider.GameWindowLost -= OnOverlayEventProviderOnGameUntracked;
    }

    public bool ShouldUpdateNow
        => _overlayIsFocused && _gameIsTracked;

    public bool ShouldNotUpdateNow
        => !ShouldUpdateNow;

    public event EventHandler<bool>? ShouldUpdateNowChanged;

    private void OnOverlayEventProviderOnGameTracked(object? _, EventArgs e)
    {
        _gameIsTracked = true;
        ShouldUpdateNowChanged?.Invoke(this, ShouldUpdateNow);
    }

    private void OnOverlayEventProviderOnGameUntracked(object? _, EventArgs e)
    {
        _gameIsTracked = false;
        ShouldUpdateNowChanged?.Invoke(this, ShouldUpdateNow);
    }

    private void OnOverlayEventProviderOnOverlayFocused(object? _, EventArgs e)
    {
        _overlayIsFocused = true;
        ShouldUpdateNowChanged?.Invoke(this, ShouldUpdateNow);
    }

    private void OnOverlayEventProviderOnOverlayBlurred(object? _, EventArgs e)
    {
        _overlayIsFocused = false;
        ShouldUpdateNowChanged?.Invoke(this, ShouldUpdateNow);
    }
}
