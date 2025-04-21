namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Enums;
using Domain.Models.Game;
using Domain.Models.Trade;
using External.UEX.Abstractions;

public class UexRentPriceProvider(IUexVehiclesApi vehiclesApi) : UexPriceProviderBase, IRentPriceProvider
{
    private Dictionary<UexApiGameEntityId, ICollection<VehicleRentalPriceBriefDTO>> _vehiclePrices = [];

    public ValueTask UpdatePriceTagAsync(IGameRentable gameEntity)
    {
        if (gameEntity.EntityCategory is GameEntityCategory.Commodity)
        {
            UpdateVehicle(gameEntity);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask<PriceTag> GetPriceTagAtAsync(IGameRentable gameEntity, IGameLocation gameLocation)
        => ValueTask.FromResult(PriceTag.Unknown);

    private void UpdateVehicle(IGameRentable gameEntity)
    {
        if (!_vehiclePrices.TryGetValue(gameEntity.Id, out var prices))
        {
            return;
        }

        var priceBounds = CreateBoundFrom(prices, price => price?.Price_rent);
        gameEntity.UpdateRentPrices(priceBounds);
    }

    protected override async Task InitializeAsyncCore(CancellationToken cancellationToken)
    {
        if (this is { _vehiclePrices.Count: > 0 })
        {
            return;
        }

        var vehiclePricesResponse = await vehiclesApi.GetVehiclesRentalsPricesAllAsync(cancellationToken).ConfigureAwait(false);
        var vehiclePrices = vehiclePricesResponse.Result.Data ?? [];
        _vehiclePrices = vehiclePrices
            .GroupBy(x => UexApiGameEntityId.Create<GameVehicle>(x.Id_vehicle ?? 0))
            .ToDictionary(UexApiGameEntityId (x) => x.Key, ICollection<VehicleRentalPriceBriefDTO> (x) => x.ToArray());
    }
}
