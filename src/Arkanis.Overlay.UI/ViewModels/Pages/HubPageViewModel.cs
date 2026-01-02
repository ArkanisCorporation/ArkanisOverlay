namespace Arkanis.Overlay.UI.ViewModels.Pages;

using Avalonia.Metadata;
using JetBrains.Annotations;
using ReactiveUI;
using Utils;

public class HubPageViewModel(IScreen screen) : PageViewModelBase(screen)
{
    [PrivateApi]
    [UsedImplicitly]
    public HubPageViewModel() : this(FakeScreen.Instance)
    {
    }

    public override string UrlPathSegment
        => "hub";
}
