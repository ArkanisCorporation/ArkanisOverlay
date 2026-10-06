namespace Arkanis.Overlay.Infrastructure.Services;

public interface IRepositorySyncStrategy
{
    public bool ShouldUpdateNow { get; }

    public bool ShouldNotUpdateNow
        => !ShouldUpdateNow;

    public event EventHandler<bool> ShouldUpdateNowChanged;
}
