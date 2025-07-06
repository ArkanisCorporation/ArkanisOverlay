namespace Arkanis.Overlay.Infrastructure.Services.Hosted;

using Abstractions;
using Common.Abstractions.Services;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

/// <summary>
///     Performs initialization of all <see cref="ISelfInitializable" /> services registered in the DI container.
///     Once services are initialized, all <see cref="ISelfUpdatable" /> services are schedule to be automatically updated.
/// </summary>
/// <param name="schedulerFactory">A factory for a job scheduler</param>
/// <param name="initializableServices">A collection of registered self-initializable services</param>
/// <param name="updatableServices">A collection of registered self-updatable services</param>
/// <param name="logger">A logger</param>
internal class InitializeServicesHostedService(
    ISchedulerFactory schedulerFactory,
    IEnumerable<ISelfInitializable> initializableServices,
    IEnumerable<ISelfUpdatable> updatableServices,
    ILogger<InitializeServicesHostedService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Running initialization of {ServiceCount} services", initializableServices.Count());
        var executionCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var initProcesses = initializableServices.Select(service => new Process(service.GetType(), StartInit(service))).ToList();

        var initWatcherCancellation = CancellationTokenSource.CreateLinkedTokenSource(executionCancellation.Token);
        var initWatcherTask = Task.Run(WatchInitProcessCompletion, initWatcherCancellation.Token);

        await Task.WhenAll(initProcesses.Select(x => x.ProcessTask)).ConfigureAwait(false);
        await initWatcherCancellation.CancelAsync();
        await initWatcherTask;
        initWatcherTask.Dispose();

        logger.LogDebug("Finished initialization of {ServiceCount} services", initializableServices.Count());

        var scheduler = await schedulerFactory.GetScheduler(executionCancellation.Token);
        foreach (var updatableService in updatableServices)
        {
            logger.LogDebug("Scheduling service update job for {Service}", updatableService);
            await PerformUpdateOnSelfUpdatableServiceJob.ScheduleForAsync(scheduler, updatableService, true, executionCancellation.Token);
        }

        logger.LogDebug("Finished scheduling updates for {ServiceCount} services", updatableServices.Count());
        return;

        async Task? WatchInitProcessCompletion()
        {
            while (!initWatcherCancellation.IsCancellationRequested)
            {
                await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(15), initWatcherCancellation.Token)).ConfigureAwait(false);
                foreach (var process in initProcesses.Where(process => !process.ProcessTask.IsCompleted))
                {
                    logger.LogWarning("Service {ServiceType} is still initializing", process.ServiceType.ShortDisplayName());
                }
            }
        }

        async Task StartInit(ISelfInitializable service)
        {
            try
            {
                logger.LogDebug("Starting initialization: {ServiceType}", service);
                await service.InitializeAsync(executionCancellation.Token);
                logger.LogDebug("Successfully initialized: {ServiceType}", service);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An error has been encountered while initializing service {ServiceType}", service);
            }
        }
    }

    private record Process(Type ServiceType, Task ProcessTask);
}
