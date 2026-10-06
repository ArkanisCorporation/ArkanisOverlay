namespace Arkanis.Overlay.External.CitizenId;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using Overlay.Common.Extensions;
using Overlay.Common.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddCitizenIdLinkHelper(this IServiceCollection services)
        => services.AddSingleton<CitizenIdLinkHelper>();

    public static IServiceCollection AddCitizenIdAuthenticatorServices(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddConfiguration<CitizenIdOptions>(configuration)
            .AddSingleton<CitizenIdAuthenticator>()
            .Alias<ExternalAuthenticator, CitizenIdAuthenticator>()
            .AddSingleton(CitizenIdAuthenticator.CreateRefreshJobScheduleProvider());
}
