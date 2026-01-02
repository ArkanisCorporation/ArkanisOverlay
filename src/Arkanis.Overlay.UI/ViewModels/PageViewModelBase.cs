namespace Arkanis.Overlay.UI.ViewModels;

using System.Reactive.Disposables;
using ReactiveUI;

public abstract class PageViewModelBase : ViewModelBase, IRoutableViewModel, IActivatableViewModel
{
    protected PageViewModelBase(IScreen screen)
    {
        HostScreen = screen;
        this.WhenActivated(InitBindings);
    }

    public ViewModelActivator Activator { get; } = new();
    public IScreen HostScreen { get; }

    public abstract string UrlPathSegment { get; }

    protected virtual void InitBindings(CompositeDisposable disposable)
    {
    }
}
