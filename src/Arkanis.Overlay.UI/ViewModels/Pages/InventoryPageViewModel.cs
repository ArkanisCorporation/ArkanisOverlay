namespace Arkanis.Overlay.UI.ViewModels.Pages;

using Avalonia.Metadata;
using JetBrains.Annotations;
using ReactiveUI;
using Utils;

public class InventoryPageViewModel(IScreen screen) : PageViewModelBase(screen)
{
    [PrivateApi]
    [UsedImplicitly]
    public InventoryPageViewModel() : this(FakeScreen.Instance)
    {
    }

    public override string UrlPathSegment
        => "inventory";
}
