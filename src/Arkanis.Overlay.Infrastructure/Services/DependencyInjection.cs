namespace Arkanis.Overlay.Infrastructure.Services;

using Abstractions;
using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using PriceProviders;

public static class DependencyInjection
{
    public static IServiceCollection AddFakePriceProviders(this IServiceCollection services)
        => services
            .AddSingleton<IPriceProvider, FakePriceProvider>()
            .AddSingleton<IPurchasePriceProvider>(provider => provider.GetRequiredService<IPriceProvider>())
            .AddSingleton<ISellPriceProvider>(provider => provider.GetRequiredService<IPriceProvider>())
            .AddSingleton<IRentPriceProvider>(provider => provider.GetRequiredService<IPriceProvider>());

    public static IServiceCollection AddInMemorySearchServices(this IServiceCollection services)
        => services.AddScoped<ISearchService, InMemorySearchService>();

    public static IServiceCollection AddEndpointManagerHostedService(this IServiceCollection services)
        => services.AddHostedService<EndpointManager>();

    public static IServiceCollection AddGameEntityPriceHydratationServices(this IServiceCollection services)
        => services
            .AddPriceProviderServices()
            .AddSingleton<IGameEntityHydratationService, UexGameEntityPriceHydratationService>();

    public static IServiceCollection AddUserPreferencesFileManagerServices(this IServiceCollection services)
        => services
            .AddSingleton<IUserPreferencesManager, UserPreferencesJsonFileManager>()
            .AddSingleton<IUserPreferencesProvider>(provider => provider.GetRequiredService<IUserPreferencesManager>())
            .AddHostedService<UserPreferencesLoader>();
}
