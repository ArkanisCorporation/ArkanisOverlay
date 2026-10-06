namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions.Services;

public class RepositorySyncGameTrackedStrategy : IRepositorySyncStrategy, IDisposable
{
    private bool _gameIsTracked;

    public RepositorySyncGameTrackedStrategy(IOverlayEventProvider overlayEventProvider)
    {
        OverlayEventProvider = overlayEventProvider;
        OverlayEventProvider.GameWindowFound += OnOverlayEventProviderOnGameTracked;
        OverlayEventProvider.GameWindowLost += OnOverlayEventProviderOnGameUntracked;
    }

    protected IOverlayEventProvider OverlayEventProvider { get; }

    public virtual void Dispose()
    {
        OverlayEventProvider.GameWindowFound -= OnOverlayEventProviderOnGameTracked;
        OverlayEventProvider.GameWindowLost -= OnOverlayEventProviderOnGameUntracked;
        GC.SuppressFinalize(this);
    }

    public virtual bool ShouldUpdateNow
        => _gameIsTracked;

    public event EventHandler<bool>? ShouldUpdateNowChanged;

    protected void EmitCurrentStatus()
        => ShouldUpdateNowChanged?.Invoke(this, ShouldUpdateNow);

    private void OnOverlayEventProviderOnGameTracked(object? _, EventArgs e)
    {
        _gameIsTracked = true;
        EmitCurrentStatus();
    }

    private void OnOverlayEventProviderOnGameUntracked(object? _, EventArgs e)
    {
        _gameIsTracked = false;
        EmitCurrentStatus();
    }
}
