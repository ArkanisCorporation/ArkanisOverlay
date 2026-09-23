namespace Arkanis.Overlay.Host.Server.UnitTests;

using System.Net;
using Arkanis.Common.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;

public sealed class ServerHealthEndpointTests(OverlayServerFactory factory) : IClassFixture<OverlayServerFactory>
{
    [Theory]
    [InlineData("/healthz/alive")]
    [InlineData("/healthz/ready")]
    [InlineData("/healthz/startup")]
    public async Task HealthEndpoints_ReturnSuccess(string path)
    {
        using var client = factory.CreateClient();

        (await client.GetAsync(path, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public void DatabaseHealthCheck_UsesDependencyReadinessAndStartupTags()
    {
        var healthCheckOptions = factory.Services.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = healthCheckOptions.Value.Registrations.Single(candidate => candidate.Name == "overlay-database");

        registration.Tags.ShouldContain(HealthCheckTags.Category.Readiness);
        registration.Tags.ShouldContain(HealthCheckTags.Category.Startup);
        registration.Tags.ShouldContain(HealthCheckTags.Type.Dependency);
        registration.Tags.ShouldContain(HealthCheckTags.Dependency.Database);
    }

}

public sealed class OverlayServerFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"arkanis-overlay-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder
            .UseEnvironment(Environments.Development)
            .UseSetting("ConnectionStrings:OverlayDatabase", $"Data Source={_databasePath};Pooling=False");

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            File.Delete(_databasePath);
        }
    }
}
