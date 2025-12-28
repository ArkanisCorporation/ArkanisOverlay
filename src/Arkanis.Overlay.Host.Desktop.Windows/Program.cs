using System;
using Avalonia;
using ReactiveUI.Avalonia;

namespace Arkanis.Overlay.Host.Desktop.Windows;

using Microsoft.Extensions.Hosting;
using Services;
using UI;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
        => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure(() => new App(ConfigureServices))
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();

    public static void ConfigureServices(IHostApplicationBuilder builder)
    {
        builder.Services.AddNativeServices();
    }
}
