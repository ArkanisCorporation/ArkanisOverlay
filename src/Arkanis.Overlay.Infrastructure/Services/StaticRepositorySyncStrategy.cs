namespace Arkanis.Overlay.Infrastructure.Services;

public class StaticRepositorySyncStrategy(bool defaultState = false) : IRepositorySyncStrategy
{
    public bool ShouldUpdateNow
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            ShouldUpdateNowChanged?.Invoke(this, value);
        }
    } = defaultState;

    public event EventHandler<bool>? ShouldUpdateNowChanged;
}
