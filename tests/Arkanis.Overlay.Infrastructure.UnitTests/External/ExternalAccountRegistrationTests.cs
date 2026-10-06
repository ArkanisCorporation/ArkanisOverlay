namespace Arkanis.Overlay.Infrastructure.UnitTests.Services;

using Common.Enums;
using Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Overlay.External.MedRunner;
using Overlay.Infrastructure.Services;
using Shouldly;

public sealed class ExternalAccountRegistrationTests
{
    [Fact]
    public void MedRunnerCredentialsSelectTheRegisteredAuthenticatorForLinkConsent()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        services.AddMockMedRunnerApiClient();
        services.AddSingleton<ExternalAuthenticatorProvider>();
        using var provider = services.BuildServiceProvider();

        var credentials = new AccountApiTokenCredentials(Common.ExternalService.Medrunner) { SecretToken = "test-token" };
        provider.GetRequiredService<ExternalAuthenticatorProvider>().GetForCredentials(credentials)
            .ShouldBeOfType<MedRunnerAuthenticator>();
    }

    [Theory]
    [InlineData(HostingMode.Server)]
    [InlineData(HostingMode.LocalSingleUser)]
    public void InfrastructureRegistersThePreferencesManagerOnce(HostingMode hostingMode)
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(new ConfigurationBuilder().Build(), options => options.HostingMode = hostingMode);

        var preferencesType = hostingMode is HostingMode.Server
            ? typeof(InMemoryUserPreferencesManager)
            : typeof(UserPreferencesJsonFileManager);
        services.Count(descriptor => descriptor.ServiceType == preferencesType).ShouldBe(1);
    }
}
