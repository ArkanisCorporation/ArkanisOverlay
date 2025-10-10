namespace Arkanis.Overlay.Infrastructure.Services;

public class FakeRepositorySyncStrategy : IRepositorySyncStrategy
{
    public bool ShouldUpdateNow
        => false;

#pragma warning disable CS0067 // Event is required by interface but will never be used in this implementation
    public event EventHandler<bool>? ShouldUpdateNowChanged;
#pragma warning restore CS0067
}
