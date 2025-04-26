namespace Arkanis.Overlay.Infrastructure.Services.Hosted;

using Abstractions;
using Domain.Models.Game;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

/// <summary>
///     Performs initialization of all <see cref="ISelfInitializable"/> services registered in the DI container.
/// </summary>
/// <param name="schedulerFactory"></param>
/// <param name="services"></param>
/// <param name="logger"></param>
internal class InitializeServicesHostedService(
    ISchedulerFactory schedulerFactory,
    IEnumerable<ISelfInitializable> services,
    ILogger<InitializeServicesHostedService> logger
) : BackgroundService
{
    private static readonly TimeSpan RefreshJobInterval = TimeSpan.FromMinutes(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Running initialization of services: {@Services}", services);
        var tasks = services.Select(async service =>
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

        await Task.WhenAll(tasks).ConfigureAwait(false);
        logger.LogDebug("Finished initialization of {ServiceCount} services", services.Count());

        var scheduler = await schedulerFactory.GetScheduler(stoppingToken);

        var genericJobType = typeof(GameEntityRepositorySyncManagerUpdateIfNecessaryJob<>);
        foreach (var gameEntityType in GameEntityConstants.GameEntityTypes)
        {
            var jobType = genericJobType.MakeGenericType(gameEntityType);
            var jobDetail = JobBuilder.Create(jobType)
                .WithIdentity(jobType.ShortDisplayName())
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithSimpleSchedule(schedule => schedule
                    .WithInterval(RefreshJobInterval)
                    .WithMisfireHandlingInstructionIgnoreMisfires()
                    .RepeatForever()
                )
                .StartAt(DateTimeOffset.UtcNow + RefreshJobInterval)
                .ForJob(jobDetail)
                .Build();

            await scheduler.ScheduleJob(jobDetail, trigger, stoppingToken);
        }
    }
}
