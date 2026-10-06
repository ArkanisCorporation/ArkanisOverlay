namespace Arkanis.Overlay.Host.Desktop.Services;

using Velopack;
using Velopack.Locators;
using Velopack.Sources;

public class ArkanisOverlayUpdateManager(
    IUpdateSource source,
    UpdateOptions? options = null,
    IVelopackLocator? locator = null
) : UpdateManager(source, options, locator)
{
    public string CurrentChannel
        => Channel;
}
