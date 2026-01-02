namespace Arkanis.Overlay.UI.ViewModels.Pages;

using Avalonia.Metadata;
using JetBrains.Annotations;
using ReactiveUI;
using Utils;

public class MarketPageViewModel(IScreen screen) : PageViewModelBase(screen)
{
    [PrivateApi]
    [UsedImplicitly]
    public MarketPageViewModel() : this(FakeScreen.Instance)
    {
    }

    public override string UrlPathSegment
        => "market";
}
