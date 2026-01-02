namespace Arkanis.Overlay.UI.ViewModels;

using ReactiveUI;

public abstract class PageViewModelBase(IScreen screen) : ViewModelBase, IRoutableViewModel
{
    public IScreen HostScreen { get; } = screen;

    public abstract string UrlPathSegment { get; }
}
