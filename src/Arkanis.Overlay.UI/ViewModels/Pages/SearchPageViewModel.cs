namespace Arkanis.Overlay.UI.ViewModels.Pages;

using ReactiveUI;

public class SearchPageViewModel(IScreen screen) : PageViewModelBase(screen)
{
    public SearchPageViewModel() : this(FakeScreen.Instance)
    {
    }

    public override string UrlPathSegment
        => "search";
}
