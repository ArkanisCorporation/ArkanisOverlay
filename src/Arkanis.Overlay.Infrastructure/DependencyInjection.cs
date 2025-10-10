namespace Arkanis.Overlay.Infrastructure;

using Common;
using Common.Enums;
using Common.Extensions;
using Data;
using Domain.Abstractions.Services;
using External.UEX;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Options;
using Quartz;
using Quartz.Simpl;
using Repositories;
using Services;
using Services.Abstractions;
using Services.Hosted;
using Services.Hydration;
using Services.PriceProviders;
using UexAccountContext = Services.External.UexAccountContext;

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

        if (options.HostingMode is HostingMode.Server)
        {
            services.AddServicesForInMemoryUserPreferences();
        }
        else
        {
            //! Registers hosted service for loading preferences from file - this needs to run as soon as possible
            services.AddServicesForUserPreferencesFromJsonFile();
        }

        services
            .AddSingleton<UexAccountContext>()
            .Alias<ISelfInitializable, UexAccountContext>();

        services
            .AddSingleton<UserConsentDialogService>()
            .Alias<IUserConsentDialogService, UserConsentDialogService>()
            .Alias<IUserConsentDialogService.IConnector, UserConsentDialogService>();

        services
            .AddUexAccountAuthentication()
            .AddSingleton<IOptionsChangeTokenSource<UexApiOptions>, UserPreferencesBasedOptionsChangeTokenSource<UexApiOptions>>()
            .AddAllUexApiClients(provider => new ConfigureOptions<UexApiOptions>(uexApiOptions =>
                    {
                        var userPreferences = provider.GetRequiredService<IUserPreferencesProvider>();
                        var credentials = userPreferences.CurrentPreferences.GetCredentialsOrDefaultFor(ExternalService.UnitedExpress);
                        uexApiOptions.UserToken = credentials?.SecretToken;
                    }
                )
            );

        services
            .AddSingleton<IStorageManager, StorageManager>()
            .AddSingleton<ServiceDependencyResolver>()
            .AddCommonInfrastructureServices()
            .AddOverlaySqliteDatabaseServices()
            .AddDatabaseExternalSyncCacheProviders()
            .AddInMemorySearchServices()
            .AddLocalInventoryManagementServices()
            .AddLocalTradeRunManagementServices()
            .AddUexInMemoryGameEntityServices()
            .AddPriceProviders()
            .AddUexHydrationServices();

        services.AddHostedService<InitializeServicesHostedService>();

        return services;
    }

    public static IServiceCollection AddUexAccountAuthentication(this IServiceCollection services)
        => services
            .AddSingleton<UexAuthenticator>()
            .AddSingleton<UexAccountContext>()
            .Alias<IExternalAccountContext, UexAccountContext>();

    public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddConfiguration<ConfigurationOptions>(configuration);

    public class InfrastructureServiceOptions
    {
        public HostingMode HostingMode { get; set; }
    }
}
