namespace Arkanis.Overlay.Infrastructure.Services;

using Common.Extensions;
using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using PriceProviders;

public static class DependencyInjection
{
    public static IServiceCollection AddFakePriceProviders(this IServiceCollection services)
        => services
            .AddSingleton<FakePriceProvider>()
            .Alias<IPriceProvider, FakePriceProvider>()
            .Alias<IMarketPriceProvider, FakePriceProvider>()
            .Alias<IPurchasePriceProvider, FakePriceProvider>()
            .Alias<ISalePriceProvider, FakePriceProvider>()
            .Alias<IRentPriceProvider, FakePriceProvider>();

    public static IServiceCollection AddDatabaseExternalSyncCacheProviders(this IServiceCollection services)
        => services
            .AddSingleton(typeof(IExternalSyncCacheProvider<>), typeof(ExternalSyncDatabaseCacheProvider<>));

    public static IServiceCollection AddInMemorySearchServices(this IServiceCollection services)
        => services.AddScoped<ISearchService, InMemorySearchService>();

    public static IServiceCollection AddLocalInventoryManagementServices(this IServiceCollection services)
        => services.AddScoped<IInventoryManager, LocalDatabaseInventoryManager>();

    public static IServiceCollection AddLocalTradeRunManagementServices(this IServiceCollection services)
        => services.AddScoped<ITradeRunManager, LocalDatabaseTradeRunManager>();

    public static IServiceCollection AddServicesForUserPreferencesFromJsonFile(this IServiceCollection services)
        => services.AddUserPreferencesServices<UserPreferencesJsonFileManager>();

    public static IServiceCollection AddServicesForInMemoryUserPreferences(this IServiceCollection services)
        => services.AddUserPreferencesServices<InMemoryUserPreferencesManager>();

    public static IServiceCollection AddUserPreferencesServices<T>(this IServiceCollection services) where T : class, IUserPreferencesManager
        => services
            .AddSingleton<T>()
            .Alias<IUserPreferencesManager, T>()
            .Alias<IUserPreferencesProvider, T>()
            .AddHostedService<AutoStartUserPreferencesUpdater>()
            .AddHostedService<UserPreferencesLoader>();
}
