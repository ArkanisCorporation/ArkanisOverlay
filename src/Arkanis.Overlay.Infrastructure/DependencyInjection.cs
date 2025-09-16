namespace Arkanis.Overlay.Infrastructure;

using Common.Enums;
using Common.Extensions;
using Data;
using Domain;
using Domain.Abstractions.Services;
using External.UEX;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using Quartz;
using Quartz.Simpl;
using Repositories;
using Services;
using Services.External;
using Services.Hosted;
using Services.Hydration;
using Services.PriceProviders;

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
            .AddSingleton<IStorageManager, StorageManager>()
            .AddSingleton<ServiceDependencyResolver>()
            .AddHostedService<InitializeServicesHostedService>()
            .AddSingleton<UexAccountContext>()
            .AddAllUexApiClients(provider =>
                {
                    var userPreferences = provider.GetRequiredService<IUserPreferencesProvider>();
                    var credentials = userPreferences.CurrentPreferences.GetOrCreateCredentialsFor(ExternalService.UnitedExpress);
                    return new UexApiOptions
                    {
                        UserToken = credentials.SecretToken,
                    };
                }
            )
            .AddCommonInfrastructureServices()
            .AddOverlaySqliteDatabaseServices()
            .AddDatabaseExternalSyncCacheProviders()
            .AddInMemorySearchServices()
            .AddLocalInventoryManagementServices()
            .AddLocalTradeRunManagementServices()
            .AddUexInMemoryGameEntityServices()
            .AddPriceProviders()
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
