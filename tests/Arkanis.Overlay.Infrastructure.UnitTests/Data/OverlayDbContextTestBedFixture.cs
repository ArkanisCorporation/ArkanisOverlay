namespace Arkanis.Overlay.Infrastructure.UnitTests.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.UnitTests;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

public class OverlayDbContextTestBedFixture : TestBedFixture
{
    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddSingleton<IHostEnvironment, UnitTestHostEnvironment>()
            .AddSingleton(configuration)
            .AddOverlaySqliteDatabaseServices();
    }

    protected override ILoggingBuilder AddLoggingProvider(ILoggingBuilder loggingBuilder, ILoggerProvider loggerProvider)
        => base.AddLoggingProvider(loggingBuilder.SetMinimumLevel(LogLevel.Debug), loggerProvider);

    protected override void AddUserSecrets(IConfigurationBuilder configurationBuilder)
        => base.AddUserSecrets(
            configurationBuilder.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"ConnectionStrings:{ClientOverlayDbContextFactory.ConnectionName}"] = $"Data Source={Path.GetRandomFileName()}",
                }
            )
        );

    protected override IEnumerable<TestAppSettings> GetTestAppSettings()
        => [];

    protected override ValueTask DisposeAsyncCore()
        => ValueTask.CompletedTask;
}
