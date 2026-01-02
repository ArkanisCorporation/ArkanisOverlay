namespace Arkanis.Overlay.UI.ViewModels.Pages;

using Avalonia.Metadata;
using JetBrains.Annotations;
using ReactiveUI;
using Utils;

public class SettingsPageViewModel(IScreen screen) : PageViewModelBase(screen)
{
    [PrivateApi]
    [UsedImplicitly]
    public SettingsPageViewModel() : this(FakeScreen.Instance)
    {
    }

    public override string UrlPathSegment
        => "settings";
}
