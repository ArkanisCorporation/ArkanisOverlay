namespace Arkanis.Overlay.Host.Desktop.UnitTests;

using Common.UnitTests;
using Infrastructure.Data;
using Infrastructure.Data.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;
using Xunit.v3;

public class DesktopHostTestFixture : TestBedFixture
{
    protected override void AddUserSecrets(IConfigurationBuilder configurationBuilder)
        => configurationBuilder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:OverlayDatabase"] = $"Data Source={Path.GetRandomFileName()}",
            }
        );

    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddSingleton<IHostEnvironment, UnitTestHostEnvironment>()
            .AddSingleton(configuration)
            .AddAllDesktopHostServices(configuration);
    }

    protected override IEnumerable<TestAppSettings> GetTestAppSettings()
        => [];

    protected override async ValueTask DisposeAsyncCore()
    {
        var serviceProvider = GetServiceProvider(new TestOutputHelper());
        await serviceProvider.DropDatabaseAsync<OverlayDbContext>();
    }
}
