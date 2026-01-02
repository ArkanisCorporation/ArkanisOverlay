namespace Arkanis.Overlay.UI.ViewModels.Pages;

using Avalonia.Metadata;
using JetBrains.Annotations;
using ReactiveUI;
using Utils;

public class OrgPageViewModel(IScreen screen) : PageViewModelBase(screen)
{
    [PrivateApi]
    [UsedImplicitly]
    public OrgPageViewModel() : this(FakeScreen.Instance)
    {
    }

    public override string UrlPathSegment
        => "org";
}
