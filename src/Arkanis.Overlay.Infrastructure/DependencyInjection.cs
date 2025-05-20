namespace Arkanis.Overlay.Infrastructure;

using Common.Enums;
using Common.Extensions;
using Data;
using External.UEX;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using Quartz;
using Quartz.Simpl;
using Repositories;
using Services;
using Services.Hosted;
using Services.Hydration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Action<InfrastructureServiceOptions> configure)
    {
        services.AddQuartz(options =>
            {
                options.UseJobFactory<MicrosoftDependencyInjectionJobFactory>();
            }
        );
        services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = false;
            }
        );

        var options = new InfrastructureServiceOptions();
        configure(options);

        services
            .AddSingleton<ServiceDependencyResolver>()
            .AddHostedService<InitializeServicesHostedService>()
            .AddAllUexApiClients()
            .AddOverlaySqliteDatabaseServices()
            .AddDatabaseExternalSyncCacheProviders()
            .AddInMemorySearchServices()
            .AddUexInMemoryGameEntityServices()
            .AddUexPriceProviders()
            .AddUexHydrationServices();

        if (options.HostingMode is HostingMode.Server)
        {
            services.AddServicesForInMemoryUserPreferences();
        }
        else
        {
            services.AddServicesForUserPreferencesFromJsonFile();
        }

        return services;
    }

    public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddConfiguration<ConfigurationOptions>(configuration);

    public class InfrastructureServiceOptions
    {
        public HostingMode HostingMode { get; set; }
    }
}
