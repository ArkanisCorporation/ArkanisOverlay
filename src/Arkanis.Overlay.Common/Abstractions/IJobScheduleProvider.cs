namespace Arkanis.Overlay.Common.Abstractions;

using Quartz;

public interface IJobScheduleProvider
{
    public IJobDetail JobDetail { get; }
    public ITrigger Trigger { get; }

    public async Task ScheduleAsync(IScheduler scheduler)
    {
        var existingJob = await scheduler.GetJobDetail(JobDetail.Key);
        if (existingJob is null)
        {
            await scheduler.ScheduleJob(JobDetail, Trigger);
        }
    }
}
