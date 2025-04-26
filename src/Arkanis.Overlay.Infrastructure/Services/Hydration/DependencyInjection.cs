namespace Arkanis.Overlay.Infrastructure.Services.Hydration;

using Abstractions;
using Common.Extensions;
using Domain.Abstractions.Game;
using Domain.Models.Game;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddUexHydrationServices(this IServiceCollection services)
        => services
            .AddHydrationServiceFor<UexGamePurchasableHydrationService, IGamePurchasable>()
            .AddHydrationServiceFor<UexGameSellableHydrationService, IGameSellable>()
            .AddHydrationServiceFor<UexGameRentableHydrationService, IGameRentable>()
            .AddHydrationServiceFor<UexGameItemTraitHydrationService, GameItem>()
            .AddSingleton<IGameEntityHydrationService, GameEntityHydrationService>();

    private static IServiceCollection AddHydrationServiceFor<TService, TEntity>(this IServiceCollection services)
        where TService : class, IHydrationServiceFor<TEntity>
        => services
            .AddSingleton<TService>()
            .Alias<IHydrationServiceFor, TService>()
            .Alias<IHydrationServiceFor<TEntity>, TService>();
}
