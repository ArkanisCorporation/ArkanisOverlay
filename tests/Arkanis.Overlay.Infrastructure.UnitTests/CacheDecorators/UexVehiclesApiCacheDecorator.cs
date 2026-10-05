namespace Arkanis.Overlay.Infrastructure.UnitTests.CacheDecorators;

using System.Threading;
using System.Threading.Tasks;
using External.UEX.Abstractions;
using Microsoft.Extensions.Logging;

public sealed class UexVehiclesApiCacheDecorator(IUexVehiclesApi vehiclesApi, ILogger<UexVehiclesApiCacheDecorator> logger)
    : UexApiCacheDecorator(logger), IUexVehiclesApi
{
    public Task<UexApiResponse<GetVehiclesLoanersOkResponse>> GetVehiclesLoanersAsync(double? id_vehicle = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_vehicle,
            },
            nameof(GetVehiclesLoanersAsync),
            x => vehiclesApi.GetVehiclesLoanersAsync(x.id_vehicle, cancellationToken)
        );

    public Task<UexApiResponse<GetVehiclesPricesOkResponse>> GetVehiclesPricesAsync(double? id_vehicle = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_vehicle,
            },
            nameof(GetVehiclesPricesAsync),
            x => vehiclesApi.GetVehiclesPricesAsync(x.id_vehicle, cancellationToken)
        );

    public Task<UexApiResponse<GetVehiclesPurchasesPricesOkResponse>> GetVehiclesPurchasesPricesByTerminalAsync(
        string id_terminal,
        CancellationToken cancellationToken = default
    )
        => CacheAsync(
            new
            {
                id_terminal,
            },
            nameof(GetVehiclesPurchasesPricesByTerminalAsync),
            x => vehiclesApi.GetVehiclesPurchasesPricesByTerminalAsync(x.id_terminal, cancellationToken)
        );

    public Task<UexApiResponse<GetVehiclesPurchasesPricesAllOkResponse>> GetVehiclesPurchasesPricesAllAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetVehiclesPurchasesPricesAllAsync),
            _ => vehiclesApi.GetVehiclesPurchasesPricesAllAsync(cancellationToken)
        );

    public Task<UexApiResponse<GetVehiclesRentalsPricesOkResponse>> GetVehiclesRentalsPricesByTerminalAsync(
        string id_terminal,
        CancellationToken cancellationToken = default
    )
        => CacheAsync(
            new
            {
                id_terminal,
            },
            nameof(GetVehiclesRentalsPricesByTerminalAsync),
            x => vehiclesApi.GetVehiclesRentalsPricesByTerminalAsync(x.id_terminal, cancellationToken)
        );

    public Task<UexApiResponse<GetVehiclesRentalsPricesAllOkResponse>> GetVehiclesRentalsPricesAllAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetVehiclesRentalsPricesAllAsync),
            _ => vehiclesApi.GetVehiclesRentalsPricesAllAsync(cancellationToken)
        );
}
