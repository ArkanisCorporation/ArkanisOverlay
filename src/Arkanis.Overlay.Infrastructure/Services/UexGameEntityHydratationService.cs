namespace Arkanis.Overlay.Infrastructure.Services;

using Abstractions;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;

public class UexGameEntityPriceHydratationService(
    ServiceDependencyResolver dependencyResolver,
    IPurchasePriceProvider purchasePriceProvider,
    ISellPriceProvider sellPriceProvider,
    IRentPriceProvider rentPriceProvider
) : IGameEntityHydratationService
{
    public Task HydrateAsync<T>(T gameEntity, CancellationToken cancellationToken) where T : IGameEntity
        => gameEntity switch
        {
            GameItem item => HydrateAsync(item, cancellationToken).AsTask(),
            GameVehicle vehicle => HydrateAsync(vehicle, cancellationToken).AsTask(),
            GameCommodity commodity => HydrateAsync(commodity, cancellationToken).AsTask(),
            _ => Task.CompletedTask,
        };

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await dependencyResolver.DependsOn(this, purchasePriceProvider, sellPriceProvider, rentPriceProvider)
            .WaitUntilReadyAsync(cancellationToken)
            .ConfigureAwait(false);

    private async ValueTask HydrateAsync(IGamePurchasable purchasable)
        => await purchasePriceProvider.UpdatePriceTagAsync(purchasable);

    private async ValueTask HydrateAsync(IGameSellable sellable)
        => await sellPriceProvider.UpdatePriceTagAsync(sellable);

    private async ValueTask HydrateAsync(IGameRentable rentable)
        => await rentPriceProvider.UpdatePriceTagAsync(rentable);

    private async ValueTask HydrateAsync(GameCommodity commodity, CancellationToken cancellationToken)
    {
        await WaitUntilReadyAsync(cancellationToken);
        await HydrateAsync(commodity as IGamePurchasable);
        await HydrateAsync(commodity as IGameSellable);
    }

    private async ValueTask HydrateAsync(GameItem item, CancellationToken cancellationToken)
    {
        await WaitUntilReadyAsync(cancellationToken);
        await HydrateAsync(item);
    }

    private async ValueTask HydrateAsync(GameVehicle item, CancellationToken cancellationToken)
    {
        await WaitUntilReadyAsync(cancellationToken);
        await HydrateAsync(item as IGamePurchasable);
        await HydrateAsync(item as IGameRentable);
    }
}
