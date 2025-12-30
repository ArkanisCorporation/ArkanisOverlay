namespace Arkanis.Overlay.UI.ViewModels.Pages;

using Avalonia.Metadata;
using JetBrains.Annotations;
using ReactiveUI;
using Utils;

public class HangarPageViewModel(IScreen screen) : PageViewModelBase(screen)
{
    [PrivateApi]
    [UsedImplicitly]
    public HangarPageViewModel() : this(FakeScreen.Instance)
    {
    }

    public override string UrlPathSegment
        => "hangar";
}
