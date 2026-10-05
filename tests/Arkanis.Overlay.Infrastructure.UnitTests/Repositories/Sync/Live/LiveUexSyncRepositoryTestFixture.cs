namespace Arkanis.Overlay.Infrastructure.UnitTests.Repositories.Sync.Live;

using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Abstractions.Services;
using External.UEX;
using Infrastructure.Repositories.Sync;
using Infrastructure.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

public class LiveUexSyncRepositoryTestFixture : TestBedFixture
{
    protected override ILoggingBuilder AddLoggingProvider(ILoggingBuilder loggingBuilder, ILoggerProvider loggerProvider)
    {
        loggingBuilder.SetMinimumLevel(LogLevel.Debug);
        return base.AddLoggingProvider(loggingBuilder, loggerProvider);
    }

    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
        => services
            .AddHttpClient()
            .AddSingleton(typeof(IExternalSyncCacheProvider<>), typeof(NoCacheProvider<>))
            .AddSingleton(typeof(IGameEntityRepository<>), typeof(GameEntityRepositoryMock<>))
            .AddSingleton<IGameEntityHydrationService, NoHydrationMockService>()
            .AddAllUexApiClients()
            .AddUexSyncRepositoryServices();

    protected override IEnumerable<TestAppSettings> GetTestAppSettings()
        => [];

    protected override ValueTask DisposeAsyncCore()
        => ValueTask.CompletedTask;
}
