namespace Arkanis.Overlay.Infrastructure.Services;

using Abstractions;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;

public class UexGameEntityPriceHydratationService(
    IPurchasePriceProvider purchasePriceProvider,
    ISellPriceProvider sellPriceProvider,
    IRentPriceProvider rentPriceProvider
) : IGameEntityHydratationService
{
    public async Task HydrateAsync<T>(T gameEntity) where T : IGameEntity
    {
        await EnsureReadyAsync();
        await (gameEntity switch
        {
            GameCommodity commodity => HydrateAsync(commodity),
            GameItem item => HydrateAsync(item),
            GameVehicle vehicle => HydrateAsync(vehicle),
            _ => ValueTask.CompletedTask,
        });
    }

    private async Task EnsureReadyAsync()
        => await Task.WhenAll(
                purchasePriceProvider.WaitUntilReadyAsync(),
                sellPriceProvider.WaitUntilReadyAsync(),
                rentPriceProvider.WaitUntilReadyAsync()
            )
            .ConfigureAwait(false);

    private async ValueTask HydrateAsync(IGamePurchasable purchasable)
        => await purchasePriceProvider.UpdatePriceTagAsync(purchasable);

    private async ValueTask HydrateAsync(IGameSellable sellable)
        => await sellPriceProvider.UpdatePriceTagAsync(sellable);

    private async ValueTask HydrateAsync(IGameRentable rentable)
        => await rentPriceProvider.UpdatePriceTagAsync(rentable);

    private async ValueTask HydrateAsync(GameCommodity commodity)
    {
        await HydrateAsync(commodity as IGamePurchasable);
        await HydrateAsync(commodity as IGameSellable);
    }

    private async ValueTask HydrateAsync(GameItem item)
        => await HydrateAsync(item as IGamePurchasable);

    private async ValueTask HydrateAsync(GameVehicle item)
    {
        await HydrateAsync(item as IGamePurchasable);
        await HydrateAsync(item as IGameRentable);
    }
}
