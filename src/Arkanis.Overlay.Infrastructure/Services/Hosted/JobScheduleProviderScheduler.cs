namespace Arkanis.Overlay.Infrastructure.Services.Hosted;

using Common.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

public class JobScheduleProviderScheduler(
    ISchedulerFactory schedulerFactory,
    IEnumerable<IJobScheduleProvider> jobScheduleProviders,
    ILogger<JobScheduleProviderScheduler> logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Scheduling jobs, using {Count} total providers", jobScheduleProviders.Count());
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        foreach (var scheduleProvider in jobScheduleProviders)
        {
            logger.LogDebug("Scheduling job {JobKey}", scheduleProvider.JobDetail.Key);
            await scheduleProvider.ScheduleAsync(scheduler);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
