namespace Arkanis.Overlay.Infrastructure.Services.Hosted;

using Abstractions;
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
        logger.LogDebug("Running initialization of services: {@Services}", updatableServices);
        var initializeTasks = initializableServices.Select(async service =>
            {
                try
                {
                    logger.LogDebug("Starting initialization: {ServiceType}", service);
                    await service.InitializeAsync(stoppingToken);
                    logger.LogDebug("Successfully initialized: {ServiceType}", service);
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "An error has been encountered while initializing service {ServiceType}", service);
                }
            }
        );

        await Task.WhenAll(initializeTasks).ConfigureAwait(false);
        logger.LogDebug("Finished initialization of {ServiceCount} services", initializableServices.Count());

        var scheduler = await schedulerFactory.GetScheduler(stoppingToken);
        foreach (var updatableService in updatableServices)
        {
            logger.LogDebug("Scheduling service update job for {Service}", updatableService);
            await PerformUpdateOnSelfUpdatableServiceJob.ScheduleForAsync(scheduler, updatableService, true, stoppingToken);
        }

        logger.LogDebug("Finished scheduling updates for {ServiceCount} services", updatableServices.Count());
    }
}
