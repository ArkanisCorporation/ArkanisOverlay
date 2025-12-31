namespace Arkanis.Overlay.UI;

using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utils;
using ViewModels;
using Views;
using MainWindow = Windows.MainWindow;

public class App : Application
{
    private readonly IHost _hostApplication;

    [UsedImplicitly]
    public App() : this(_ => { })
    {
    }

    public App(Action<HostApplicationBuilder> configureHost)
    {
        var applicationBuilder = Host.CreateApplicationBuilder();
        configureHost(applicationBuilder);
        ConfigureHost(applicationBuilder);
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

    private void ConfigureHost(HostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddSingleton<PageViewModelFactory>();
        applicationBuilder.Services.AddSingleton(EmptySearchService.Instance);
    }
}
