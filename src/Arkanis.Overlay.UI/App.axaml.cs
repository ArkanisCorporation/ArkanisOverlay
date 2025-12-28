using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Arkanis.Overlay.UI;

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ViewModels;
using Views;
using MainWindow = Windows.MainWindow;

public partial class App : Application
{
    [UsedImplicitly]
    public App() : this(_ => { })
    {
    }

    private readonly IHost _hostApplication;

    public App(Action<HostApplicationBuilder> configureHost)
    {
        var applicationBuilder = Host.CreateApplicationBuilder();
        configureHost(applicationBuilder);
        _hostApplication = applicationBuilder.Build();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _hostApplication.Start();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var viewModel = new MainViewModel
        {
            Services = _hostApplication.Services,
        };
        var hostApplicationLifetime = _hostApplication.Services.GetRequiredService<IHostApplicationLifetime>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = viewModel };

            desktop.ShutdownRequested += (_, _) => hostApplicationLifetime.StopApplication();
            hostApplicationLifetime.ApplicationStopping.Register(() => desktop.Shutdown());
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView { DataContext = viewModel };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
