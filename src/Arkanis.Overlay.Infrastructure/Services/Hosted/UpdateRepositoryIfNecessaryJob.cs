namespace Arkanis.Overlay.Infrastructure.Services.Hosted;

using Abstractions;
using Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Quartz;

/// <summary>
///     This job is responsible for performing a periodical updates on a self-updatable service.
/// </summary>
/// <param name="logger">A logger</param>
internal class PerformUpdateOnSelfUpdatableServiceJob(ILogger<PerformUpdateOnSelfUpdatableServiceJob> logger) : IJob
{
    private const string JobGroupName = "ServiceUpdates";

    private const string TargetServiceDataMapKey = "TargetService";
    private const string OnlyIfNecessaryDataMapKey = "OnlyIfNecessary";

    public async Task Execute(IJobExecutionContext context)
    {
        var onlyIfNecessary = context.GetFromJobData<bool>(OnlyIfNecessaryDataMapKey);
        var targetService = context.GetFromJobData<ISelfUpdatable>(TargetServiceDataMapKey);
        if (onlyIfNecessary)
        {
            logger.LogDebug("Running scheduled update (if necessary) for {ServiceType}", targetService.GetType().ShortDisplayName());
            await targetService.UpdateIfNecessaryAsync(context.CancellationToken);
        }
        else
        {
            logger.LogDebug("Running scheduled update (forced) for {ServiceType}", targetService.GetType().ShortDisplayName());
            await targetService.UpdateAsync(context.CancellationToken);
        }
    }

    public static async Task ScheduleForAsync(IScheduler scheduler, ISelfUpdatable targetService, bool onlyIfNecessary, CancellationToken cancellationToken)
    {
        var job = CreateJob(targetService, onlyIfNecessary);
        var jobTrigger = targetService.Trigger;

        await scheduler.ScheduleJob(job, jobTrigger, cancellationToken);
    }

    private static IJobDetail CreateJob(ISelfUpdatable targetService, bool onlyIfNecessary)
        => JobBuilder.Create<PerformUpdateOnSelfUpdatableServiceJob>()
            .DisallowConcurrentExecution()
            .SetJobData(CreateDataMap(targetService, onlyIfNecessary))
            .WithIdentity(targetService.GetType().ShortDisplayName(), JobGroupName)
            .Build();

    private static JobDataMap CreateDataMap(ISelfUpdatable selfUpdatableService, bool onlyIfNecessary)
        => new()
        {
            [OnlyIfNecessaryDataMapKey] = onlyIfNecessary,
            [TargetServiceDataMapKey] = selfUpdatableService,
        };
}
