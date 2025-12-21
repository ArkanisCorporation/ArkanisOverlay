namespace Arkanis.Overlay.Infrastructure;

using Common;
using Common.Abstractions;
using Common.Enums;
using Common.Extensions;
using Common.Models;
using Common.Options;
using Common.Services;
using Data;
using Domain.Abstractions.Services;
using Domain.Services;
using External.Backend.Options;
using External.CitizenId;
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
using Services.External;
using Services.Hosted;
using Services.Hydration;
using Services.PriceProviders;
using UexAccountContext = Services.External.UexAccountContext;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<InfrastructureServiceOptions> configure
    )
    {
        services.AddQuartz(options => options.UseJobFactory<MicrosoftDependencyInjectionJobFactory>());
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = false);

        services.Configure(configure);
        var options = new InfrastructureServiceOptions();
        configure(options);

        if (options.HostingMode is HostingMode.Server)
        {
            services.AddServicesForInMemoryUserPreferences();

            services.AddSingleton<StaticRepositorySyncStrategy>(_ => new StaticRepositorySyncStrategy(true))
                .Alias<IRepositorySyncStrategy, StaticRepositorySyncStrategy>();
        }
        else
        {
            //! Registers hosted service for loading preferences from file - this needs to run as soon as possible
            services.AddServicesForUserPreferencesFromJsonFile();

            services.AddSingleton<RepositorySyncGameTrackedStrategy>()
                .Alias<IRepositorySyncStrategy, RepositorySyncGameTrackedStrategy>();
        }

        services
            .AddSingleton<UserConsentDialogService>()
            .Alias<IUserConsentDialogService, UserConsentDialogService>()
            .Alias<IUserConsentDialogService.IConnector, UserConsentDialogService>();

        services.AddCitizenIdAccountAuthentication(configuration);
        // TODO: Schedule credentials refresh job for Citizen ID

        services.AddSingleton<ExternalAuthenticatorProvider>();
        services
            .AddUexAccountAuthentication()
            .AddSingleton<IOptionsChangeTokenSource<UexApiOptions>, UserPreferencesBasedOptionsChangeTokenSource<UexApiOptions>>()
            .AddAllUexApiClients(provider => new ConfigureOptions<UexApiOptions>(uexApiOptions =>
                    {
                        var userPreferences = provider.GetRequiredService<IUserPreferencesProvider>();
                        var credentials = userPreferences.CurrentPreferences.GetCredentialsOrDefaultFor(ExternalService.UnitedExpress);
                        if (credentials is AccountApiTokenCredentials tokenCredentials)
                        {
                            uexApiOptions.UserToken = tokenCredentials.SecretToken;
                        }
                    }
                )
            );

        services
            .AddConfiguration<ArkanisRestBackendOptions>(configuration)
            .AddConfiguration<ArkanisGraphqlBackendOptions>(configuration)
            .AddArkanisBackend()
            .ConfigureHttpClient((serviceProvider, client) =>
                {
                    var backendOptions = serviceProvider.GetRequiredService<IOptions<ArkanisGraphqlBackendOptions>>();
                    client.BaseAddress = new Uri(backendOptions.Value.HttpClientBaseAddress);
                }
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
            .AddIconManagementServices()
            .AddUexInMemoryGameEntityServices()
            .AddPriceProviders()
            .AddUexHydrationServices();

        services.AddHostedService<InitializeServicesHostedService>();
        services.AddHostedService<JobScheduleProviderScheduler>();

        return services;
    }

    public static IServiceCollection AddCitizenIdAccountAuthentication(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddCitizenIdLinkHelper()
            .AddCitizenIdAuthenticatorServices(configuration)
            .AddSingleton<CitizenIdAccountContext>()
            .Alias<ISelfInitializable, CitizenIdAccountContext>()
            .Alias<IExternalAccountContext, CitizenIdAccountContext>();

    public static IServiceCollection AddUexAccountAuthentication(this IServiceCollection services)
        => services
            .AddUexAuthenticatorServices()
            .Alias<ExternalAuthenticator, UexAuthenticator>()
            .AddSingleton<UexAccountContext>()
            .Alias<ISelfInitializable, UexAccountContext>()
            .Alias<IExternalAccountContext, UexAccountContext>();

    public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddConfiguration<ConfigurationOptions>(configuration)
            .AddConfiguration<PostHogOptions>(configuration);
}
