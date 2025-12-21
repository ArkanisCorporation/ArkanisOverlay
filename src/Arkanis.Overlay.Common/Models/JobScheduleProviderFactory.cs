namespace Arkanis.Overlay.Common.Models;

using System.Diagnostics.CodeAnalysis;
using Abstractions;
using Quartz;

public class JobScheduleProviderFactory(Func<IJobDetail> getJobDetail, Func<ITrigger> getTrigger) : IJobScheduleProvider
{
    [field: MaybeNull]
    public IJobDetail JobDetail
        => field ??= getJobDetail();

    [field: MaybeNull]
    public ITrigger Trigger
        => field ??= getTrigger();
}
