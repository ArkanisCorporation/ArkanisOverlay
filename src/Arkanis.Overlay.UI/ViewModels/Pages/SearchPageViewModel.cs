namespace Arkanis.Overlay.UI.ViewModels.Pages;

using Avalonia.Metadata;
using JetBrains.Annotations;
using ReactiveUI;
using Utils;

public class SearchPageViewModel(IScreen screen) : PageViewModelBase(screen)
{
    [PrivateApi]
    [UsedImplicitly]
    public SearchPageViewModel() : this(FakeScreen.Instance)
    {
    }

    public override string UrlPathSegment
        => "search";
}
