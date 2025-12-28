namespace Arkanis.Overlay.UI.ViewModels.Pages;

using ReactiveUI;

public class InventoryPageViewModel(IScreen screen) : PageViewModelBase(screen)
{
    public InventoryPageViewModel() : this(FakeScreen.Instance)
    {
    }

    public override string UrlPathSegment
        => "inventory";
}
