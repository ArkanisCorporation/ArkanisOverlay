namespace Arkanis.Overlay.UI.ViewModels.Pages;

using Avalonia.Metadata;
using JetBrains.Annotations;
using ReactiveUI;
using Utils;

public class MiningPageViewModel(IScreen screen) : PageViewModelBase(screen)
{
    [PrivateApi]
    [UsedImplicitly]
    public MiningPageViewModel() : this(FakeScreen.Instance)
    {
    }

    public override string UrlPathSegment
        => "mining";
}
