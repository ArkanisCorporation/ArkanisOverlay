namespace Arkanis.Overlay.Infrastructure.UnitTests.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services;
using Shouldly;
using Xunit;

public sealed class DependencyResolverUnitTests : IDisposable
{
    private readonly CancellationTokenSource _cancellation = new(TimeSpan.FromSeconds(5));

    public void Dispose()
        => _cancellation.Dispose();

    [Fact]
    public async Task SimpleDependency_Test()
    {
        var dependency = FulfillableDependency.Create();

        var context = DependencyResolver.DependsOn(this, [dependency]);
        var task = context.WaitUntilReadyAsync(_cancellation.Token);

        await Should.ThrowAsync<TimeoutException>(async () => await task.WaitAsync(TimeSpan.FromMilliseconds(100)));

        dependency.Fulfill();
        await task;
    }

    [Fact]
    public async Task ChainedDependency_Test()
    {
        var dependency = FulfillableDependency.Create();
        var chainedDependency = ChainedDependency.Create(dependency);

        var context = DependencyResolver.DependsOn(this, [chainedDependency]);
        var task = context.WaitUntilReadyAsync(_cancellation.Token);

        await Should.ThrowAsync<TimeoutException>(async () => await task.WaitAsync(TimeSpan.FromMilliseconds(100)));

        dependency.Fulfill();
        await task;
    }

    [Fact]
    public async Task ChainedDependency_With_Parent_Test()
    {
        var dependency = FulfillableDependency.Create();
        var chainedDependency = ChainedDependency.Create(dependency);

        var context = DependencyResolver.DependsOn(this, [chainedDependency, dependency]);
        var task = context.WaitUntilReadyAsync(_cancellation.Token);

        await Should.ThrowAsync<TimeoutException>(async () => await task.WaitAsync(TimeSpan.FromMilliseconds(100)));

        dependency.Fulfill();
        await task;
    }

    [Fact]
    public async Task MultipleChainedDependencies()
    {
        var dependency = FulfillableDependency.Create();
        var chainedDependency1 = ChainedDependency.Create(dependency);
        var chainedDependency2 = ChainedDependency.Create(chainedDependency1);
        var chainedDependency3 = ChainedDependency.Create(chainedDependency1);

        var context = DependencyResolver.DependsOn(this, [chainedDependency1, chainedDependency2, chainedDependency3]);
        var task = context.WaitUntilReadyAsync(_cancellation.Token);

        await Should.ThrowAsync<TimeoutException>(async () => await task.WaitAsync(TimeSpan.FromMilliseconds(100)));

        dependency.Fulfill();
        await task;
    }

    [Fact]
    public async Task ChainedContexts()
    {
        var dependency = FulfillableDependency.Create();
        var context1 = DependencyResolver.DependsOn(this, [dependency]);
        var context2 = DependencyResolver.DependsOn(this, [context1]);
        var context3 = DependencyResolver.DependsOn(this, [context2]);

        var task = context3.WaitUntilReadyAsync(_cancellation.Token);

        await Should.ThrowAsync<TimeoutException>(async () => await task.WaitAsync(TimeSpan.FromMilliseconds(100)));

        dependency.Fulfill();
        await task;
    }
}
