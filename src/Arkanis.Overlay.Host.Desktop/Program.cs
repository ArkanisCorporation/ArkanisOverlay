namespace Arkanis.Overlay.Host.Desktop;

using System.Globalization;
using System.Security.AccessControl;
using System.Security.Principal;
using Common;
using Common.Abstractions;
using Common.Enums;
using Common.Extensions;
using Components.Helpers;
using Components.Services;
using Dapplo.Microsoft.Extensions.Hosting.Wpf;
using Domain.Abstractions.Services;
using Exceptions;
using Helpers;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.Extensions;
using Infrastructure.Services;
using Infrastructure.Services.Abstractions;
using LocalLink.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using Quartz;
using Serilog;
using Services;
using Services.Factories;
using UI;
using UI.Windows;
using Velopack;
using Velopack.Sources;
using Windows.Win32;
using Workers;

// based on:
// https://github.com/dapplo/Dapplo.Microsoft.Extensions.Hosting/blob/master/samples/Dapplo.Hosting.Sample.WpfDemo/Program.cs#L48
public static class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        // WPF Applications do not allocate a console by default, nor do they attach to the parent console if one exists.
        // To get proper console output, we need to attach to the parent console.
        // This is safe to use even if there is no parent console or process - it just won't have any effect.
        PInvoke.AttachConsole(PInvoke.ATTACH_PARENT_PROCESS);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        await HandleInstallationBehaviourAsync(args);

        var cliSwitchMappings = ApplicationConstants.Args.All
            .Select(x => KeyValuePair.Create($"--{x}", ApplicationConstants.Args.Config.GetKeyFor(x)))
            .ToDictionary();

        var hostBuilder = Host.CreateDefaultBuilder(args)
            .UseCommonServices((context, options) =>
                {
                    options.UseFileLogging = true;
                    options.UseSeqLogging = context.IsDevelopment();
                }
            )
            .ConfigureAppConfiguration(config => config.AddCommandLine(args, cliSwitchMappings))
            .ConfigureServices((context, services) => services.AddAllDesktopHostServices(context.Configuration))
            .ConfigureWpf(options =>
                {
                    options.UseApplication<App>();

                    // Windows will be registered as singletons
                    options.UseWindow<OverlayWindow>();
                    options.UseWindow<HudWindow>();
                }
            )
            .UseWpfLifetime()
            .UseConsoleLifetime();

        try
        {
            using var appMutex = new SystemAppMutexManager();
            var host = hostBuilder.Build();

            try
            {
                var preventLaunch = host.Services.GetRequiredService<IConfiguration>()
                    .GetSection(ApplicationConstants.Args.Config.GetKeyFor(ApplicationConstants.Args.PreventLaunch))
                    .Exists();

                if (!appMutex.TryAcquire() || preventLaunch)
                {
                    Log.Warning("Another application instance is already running");
                    var hostApplicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                    var protocolCallHandler = host.Services.GetRequiredService<NamedPipeCommandCallForwarder>();

                    if (await protocolCallHandler.TryProcessCustomProtocolCallFromConfigurationAsync(hostApplicationLifetime.ApplicationStopping))
                    {
                        // custom protocol invocation was successfully processed, do not continue to launch
                        return;
                    }
                }

                if (preventLaunch)
                {
                    return;
                }

                //? force acquire or throw
                appMutex.Acquire();
                await host.MigrateDatabaseAsync<OverlayDbContext>().ConfigureAwait(false);
                await host.RunAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {
                Log.Warning(e, "Host terminated");
            }
            catch (ApplicationAlreadyRunningException e)
            {
                Log.Fatal(e, "Another instance is already running");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            await Console.Error.WriteLineAsync($"An error occurred during app startup: {ex.Message}");
            throw;
        }
    }

    private static async Task HandleInstallationBehaviourAsync(string[] args)
    {
        var launchHostBuilder = Host.CreateDefaultBuilder(args)
            .UseCommonServices((context, options) =>
                {
                    options.UseFileLogging = true;
                    options.UseSeqLogging = context.IsDevelopment();
                }
            )
            .ConfigureServices((_, services) => services
                .AddVelopackServices()
                .AddFakeAnalyticsServices()
                .AddSingleton<IStorageManager, StorageManager>()
                .AddSingleton<ISystemAutoStartStateProvider, NoSystemAutoStartStateProvider>()
                .AddSingleton<ISchedulerFactory, FakeSchedulerFactory>()
                .AddSingleton<WindowsNotifications>()
                .AddServicesForUserPreferencesFromJsonFile()
            );

        using var launchApp = launchHostBuilder.Build();
        using var loggerFactory = launchApp.Services.GetRequiredService<ILoggerFactory>();

        var preferencesManager = launchApp.Services.GetRequiredService<IUserPreferencesManager>();
        await preferencesManager.LoadUserPreferencesAsync();

        var logger = loggerFactory.CreateLogger(typeof(Program));
        logger.LogDebug("Running velopack with args: '{Args}'", string.Join("', '", args));

        var userPreferences = preferencesManager.CurrentPreferences;
        VelopackApp.Build()
            .SetArgs(args)
            .OnFirstRun(_ => WindowsNotifications.ShowWelcomeToast(userPreferences))
            .OnAfterUpdateFastCallback(WindowsNotifications.ShowUpdatedToast)
            .Run();

        try
        {
            logger.LogDebug("Starting update process for channel: {UpdateChannel}", userPreferences.UpdateChannel);
            using var update = ActivatorUtilities.CreateInstance<UpdateProcess>(launchApp.Services);
            await update.RunAsync(true, CancellationToken.None);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Update process failed for channel: {UpdateChannel}", userPreferences.UpdateChannel);
        }
    }

    internal static IServiceCollection AddAllDesktopHostServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureConfiguration(configuration);

        // Data
        services
            .AddWindowsOverlayControls()
            .AddInfrastructure(configuration, options => options.HostingMode = HostingMode.LocalSingleUser);

        services.AddWpfBlazorWebView();
        services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.MaxDisplayedSnackbars = 1;

                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;

                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.ShowCloseIcon = false;
                config.SnackbarConfiguration.VisibleStateDuration = 2000;
                config.SnackbarConfiguration.HideTransitionDuration = 250;
                config.SnackbarConfiguration.ShowTransitionDuration = 250;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Outlined;
            }
        );
        services.AddSingleton(sp => sp);
        services.AddHttpClient();

        services.AddEssentialComponentServices();

        services.AddGoogleTrackingServices()
            .AddSharedComponentServices()
            .AddSingleton<SharedAnalyticsPropertyProvider, DesktopAnalyticsPropertyProvider>();

        services.AddGlobalKeyboardProxyService();
        services.AddJavaScriptEventInterop();
        services.AddIconPickerBridge();
        services.AddSingleton(typeof(WindowProvider<>));

        services.AddHostedService<WindowsCustomProtocolHandlerManager>();
        services.AddHostedService<WindowsAutoStartManager>()
            .AddSingleton<ISystemAutoStartStateProvider, WindowsAutoStartStateProvider>();

        services.AddSingleton<WindowsNotifications>();

        // Auto updater
        services.AddVelopackServices();

        // Singleton Services
        services.AddSingleton<BlurHelper>();
        services.AddSingleton(typeof(WindowControls<>));
        services.AddMemoryCache();

        // Factories
        services.AddSingleton<WindowFactory>();

        // Workers
        services.AddSingleton<GameWindowTracker>()
            .Alias<IHostedService, GameWindowTracker>();

        services.AddSingleton<GlobalKeyboardShortcutListener>()
            .Alias<IHostedService, GlobalKeyboardShortcutListener>();

        return services;
    }

    internal static IServiceCollection AddVelopackServices(this IServiceCollection services)
        => services
            .AddTransient(provider =>
                {
                    var userPreferencesProvider = provider.GetRequiredService<IUserPreferencesProvider>();
                    return UpdateHelper.CreateSourceFor(userPreferencesProvider.CurrentPreferences.UpdateChannel);
                }
            )
            .AddTransient(provider => new UpdateOptions
            {
                AllowVersionDowngrade = true,
                ExplicitChannel = provider.GetRequiredService<IUserPreferencesProvider>().CurrentPreferences.UpdateChannel.VelopackChannelId,
            }
            )
            .AddTransient(provider => ActivatorUtilities.CreateInstance<ArkanisOverlayUpdateManager>(provider))
            .AddTransient<IAppVersionProvider, VelopackAppVersionProvider>()
            .AddHostedService<UpdateProcess.CheckForUpdatesJob.SelfScheduleService>();

    private class SystemAppMutexManager : IDisposable
    {
        /// <summary>
        ///     Signalises that the attempt to acquire the mutex has completed and the result is available.
        /// </summary>
        private readonly SemaphoreSlim _acquireSemaphore = new(0, 1);

        private readonly MutexHolder _mutexHolder = new();

        /// <summary>
        ///     Signalises that the main thread is shutting down and the mutex can be disposed.
        /// </summary>
        private readonly SemaphoreSlim _releaseSemaphore = new(0, 1);

        private Thread? _thread;

        public void Dispose()
        {
            _releaseSemaphore.Release();
            _thread?.Join();
        }

        /// <summary>
        ///     Acquires the mutex. If the mutex is already acquired, throws an <see cref="ApplicationAlreadyRunningException" />.
        /// </summary>
        /// <exception cref="ApplicationAlreadyRunningException">Thrown if the mutex is already acquired</exception>
        public void Acquire()
        {
            if (!TryAcquire())
            {
                throw new ApplicationAlreadyRunningException();
            }
        }

        /// <summary>
        ///     Tries to acquire the mutex. If the mutex is already acquired, returns <c>true</c>.
        ///     If the mutex is not acquired within 1 second, returns <c>false</c>.
        ///     If the mutex is abandoned in another process, logs a warning and returns <c>true</c>.
        /// </summary>
        /// <returns><c>true</c> if the mutex is acquired, <c>false</c> otherwise.</returns>
        public bool TryAcquire()
        {
            if (_thread is null)
            {
                _thread ??= new Thread(AcquireAndDispose);
                _thread.Start();
            }

            try
            {
                // wait for the attempt to acquire the mutex to complete
                _acquireSemaphore.Wait();
                return _mutexHolder.HasHandle;
            }
            finally
            {
                _acquireSemaphore.Release();
            }
        }

        private void AcquireAndDispose()
        {
            // acquire the system-global application mutex
            _mutexHolder.TryAcquire();

            // notify the main thread that the attempt to acquire the mutex has completed
            _acquireSemaphore.Release();

            // wait for the main thread shutdown to dispose the mutex
            _releaseSemaphore.Wait();
            _mutexHolder.Dispose();
        }

        private class MutexHolder : IDisposable
        {
            // unique id for global mutex - Global prefix means it is global to the machine
            private static readonly string MutexId = string.Format(CultureInfo.InvariantCulture, @"Global\{{{0}}}", Constants.InstanceId);

            private static readonly MutexAccessRule AllowEveryoneRule = new(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                MutexRights.FullControl,
                AccessControlType.Allow
            );

            private Mutex? _applicationMutex;

            public bool HasHandle { get; private set; }

            private Mutex ApplicationMutex
                => _applicationMutex ??= CreateMutex();

            /// <summary>
            ///     Releases the mutex and disposes of it.
            /// </summary>
            /// <remarks>
            ///     If the mutex is currently held, this method will release it.
            ///     Then, it will dispose of the mutex.
            /// </remarks>
            public void Dispose()
            {
                if (HasHandle)
                {
                    lock (ApplicationMutex)
                    {
                        _applicationMutex?.ReleaseMutex();
                    }
                }

                _applicationMutex?.Dispose();
            }

            private static Mutex CreateMutex()
            {
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(AllowEveryoneRule);

                var mutex = new Mutex(false, MutexId);
                mutex.SetAccessControl(securitySettings);

                return mutex;
            }

            /// <summary>
            ///     Tries to acquire the mutex. If the mutex is already acquired, returns <c>true</c>.
            ///     If the mutex is not acquired within 1 second, returns <c>false</c>.
            ///     If the mutex is abandoned in another process, logs a warning and returns <c>true</c>.
            /// </summary>
            /// <returns><c>true</c> if the mutex is acquired, <c>false</c> otherwise.</returns>
            public bool TryAcquire()
            {
                if (HasHandle)
                {
                    return true;
                }

                try
                {
                    HasHandle = ApplicationMutex.WaitOne(TimeSpan.FromSeconds(1), false);
                }
                catch (AbandonedMutexException e)
                {
                    Log.Warning(e, "Previous Mutex was abandoned in another process");
                    HasHandle = true;
                }

                return HasHandle;
            }
        }
    }
}
