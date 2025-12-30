namespace Arkanis.Overlay.UI.Utils;

using System;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ViewModels;

public class PageViewModelFactory(IServiceProvider serviceProvider)
{
    public PageViewModelBase Create<TViewModel>(IScreen screen) where TViewModel : PageViewModelBase
        => ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider, screen);
}
