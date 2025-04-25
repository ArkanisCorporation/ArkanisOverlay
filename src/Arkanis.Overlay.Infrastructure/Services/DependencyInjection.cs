namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using PriceProviders;
using PriceProviders.UEX;

public static class DependencyInjection
{
    public static IServiceCollection AddFakePriceProviders(this IServiceCollection services)
        => services
            .AddSingleton<IPriceProvider, FakePriceProvider>()
            .AddSingleton<IPurchasePriceProvider>(provider => provider.GetRequiredService<IPriceProvider>())
            .AddSingleton<ISalePriceProvider>(provider => provider.GetRequiredService<IPriceProvider>())
            .AddSingleton<IRentPriceProvider>(provider => provider.GetRequiredService<IPriceProvider>());

    public static IServiceCollection AddUexPriceProviders(this IServiceCollection services)
        => services
            .AddSingleton<IPriceProvider, PriceProviderAggregate>()
            .AddUexPriceProviderServices();

    public static IServiceCollection AddDatabaseExternalSyncCacheProviders(this IServiceCollection services)
        => services
            .AddSingleton(typeof(IExternalSyncCacheProvider<>), typeof(ExternalSyncDatabaseCacheProvider<>));

    public static IServiceCollection AddInMemorySearchServices(this IServiceCollection services)
        => services.AddScoped<ISearchService, InMemorySearchService>();

    public static IServiceCollection AddUserPreferencesFileManagerServices(this IServiceCollection services)
        => services
            .AddSingleton<IUserPreferencesManager, UserPreferencesJsonFileManager>()
            .AddSingleton<IUserPreferencesProvider>(provider => provider.GetRequiredService<IUserPreferencesManager>())
            .AddHostedService<UserPreferencesLoader>();
}
