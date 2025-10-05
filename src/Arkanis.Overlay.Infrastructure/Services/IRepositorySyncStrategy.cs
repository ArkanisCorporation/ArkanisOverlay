namespace Arkanis.Overlay.Infrastructure.Services;

public interface IRepositorySyncStrategy
{
    public bool ShouldUpdateNow { get; }
    public bool ShouldNotUpdateNow { get; }

    public event EventHandler<bool> ShouldUpdateNowChanged;
}
