namespace Arkanis.Overlay.Host.Desktop.Workers;

using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Services;
using Velopack;

internal class UpdateProcess(
    ArkanisOverlayUpdateManager updateManager,
    WindowsNotifications notifications,
    IStorageManager storageManager,
    ILogger<UpdateProcess> logger
) : IDisposable
{
    private WindowsNotifications.UpdatableNotification<WindowsNotifications.UpdateProgressParams>? _progressToast;

    public void Dispose()
        => _progressToast?.Hide();

    /// <summary>
    ///     Executes the update process by checking for new updates, prompting the user for confirmation,
    ///     downloading, and applying the updates, followed by restarting the application.
    /// </summary>
    /// <param name="forced">Indicates whether user confirmation is requested</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task RunAsync(bool forced, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Checking for application updates for app {AppId}, in channel {CurrentChannel}, current version {CurrentVersion}",
            updateManager.AppId,
            updateManager.CurrentChannel,
            updateManager.CurrentVersion
        );

        if (!updateManager.IsInstalled)
        {
            // do not check for updates if the application is not installed (=app is not updatable)
            logger.LogInformation("The application is not installed, skipping update check");
            return;
        }

        // check for new version
        var newVersion = await updateManager.CheckForUpdatesAsync();
        if (newVersion == null)
        {
            // no updates available
            logger.LogInformation("No updates currently available");
            return;
        }

        try
        {
            if (!forced)
            {
                logger.LogDebug("Update is not forced, prompting user for confirmation");
                if (!await notifications.ShouldUpdateNowAsync(newVersion, cancellationToken))
                {
                    // the user has chosen not to update
                    logger.LogInformation("Update refused by user");
                    return;
                }
            }

            // download new version
            ShowProgressToast(newVersion);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Pre-update user prompt has errored, continuing with the update");
        }

        logger.LogDebug("Downloading update from {CurrentVersion} to {NewVersion}", updateManager.CurrentVersion, newVersion.TargetFullRelease.Version);
        await updateManager.DownloadUpdatesAsync(newVersion, UpdateProgressToast, cancelToken: cancellationToken);

        try
        {
            logger.LogDebug("Cleaning up before update to {NewVersion}", newVersion.TargetFullRelease.Version);

            // this is necessary to properly clean up sent notifications
            Dispose();
            notifications.Dispose();

            if (newVersion.IsDowngrade)
            {
                //! breaking changes introduced from new version could make old versions incompatible, so we need to wipe the storage
                await storageManager.WipeAsync(cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Pre-update cleanup has errored, continuing with the update");
        }

        // install new version and restart app
        logger.LogInformation("Applying update to {NewVersion}", newVersion.TargetFullRelease.Version);

        //! this call uses Environment.Exit to immediately terminate the application
        updateManager.ApplyUpdatesAndRestart(newVersion);
    }

    private void ShowProgressToast(UpdateInfo newVersion)
        => _progressToast ??= notifications.ShowUpdateProgressToast(newVersion);

    private void UpdateProgressToast(int progressPercent)
        => _progressToast?.Update(new WindowsNotifications.UpdateProgressParams(progressPercent));

    internal class CheckForUpdatesJob(IServiceProvider serviceProvider) : IJob
    {
        public static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromMinutes(15);

        public static ITrigger Trigger { get; } = TriggerBuilder.Create()
            .WithIdentity(nameof(CheckForUpdatesJob))
            .StartNow()
            .WithSimpleSchedule(x => x.WithInterval(UpdateCheckInterval).RepeatForever())
            .Build();

        public async Task Execute(IJobExecutionContext context)
        {
            using var updateProcess = ActivatorUtilities.CreateInstance<UpdateProcess>(serviceProvider);
            await updateProcess.RunAsync(false, context.CancellationToken);
        }

        private static async Task ScheduleAsync(IScheduler scheduler, CancellationToken cancellationToken)
            => await scheduler.ScheduleJob(CreateJob(), Trigger, cancellationToken);

        private static IJobDetail CreateJob()
            => JobBuilder.Create<CheckForUpdatesJob>()
                .WithIdentity(nameof(CheckForUpdatesJob))
                .DisallowConcurrentExecution()
                .Build();

        public class SelfScheduleService(
            ArkanisOverlayUpdateManager updateManager,
            ISchedulerFactory schedulerFactory,
            ILogger<SelfScheduleService> logger
        ) : IHostedService
        {
            public async Task StartAsync(CancellationToken cancellationToken)
            {
                if (!updateManager.IsInstalled)
                {
                    logger.LogInformation("The application is not installed, skipping scheduling for updates");
                    return;
                }

                var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
                await ScheduleAsync(scheduler, cancellationToken);
            }

            public Task StopAsync(CancellationToken cancellationToken)
                => Task.CompletedTask;
        }
    }
}
