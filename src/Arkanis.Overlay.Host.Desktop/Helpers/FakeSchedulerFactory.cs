namespace Arkanis.Overlay.Host.Desktop.Helpers;

using Quartz;

internal class FakeSchedulerFactory : ISchedulerFactory
{
    public Task<IReadOnlyList<IScheduler>> GetAllSchedulers(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<IScheduler>>([]);

    public Task<IScheduler> GetScheduler(CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<IScheduler?> GetScheduler(string schedName, CancellationToken cancellationToken = default)
        => Task.FromResult<IScheduler?>(null);
}
