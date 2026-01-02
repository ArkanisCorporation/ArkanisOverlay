namespace Arkanis.Overlay.UI.ViewModels.Pages;

using Avalonia.Metadata;
using JetBrains.Annotations;
using ReactiveUI;
using Utils;

public class TradePageViewModel(IScreen screen) : PageViewModelBase(screen)
{
    [PrivateApi]
    [UsedImplicitly]
    public TradePageViewModel() : this(FakeScreen.Instance)
    {
    }

    public override string UrlPathSegment
        => "trade";
}
