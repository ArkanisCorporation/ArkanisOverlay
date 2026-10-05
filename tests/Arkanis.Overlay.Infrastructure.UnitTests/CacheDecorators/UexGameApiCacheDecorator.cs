namespace Arkanis.Overlay.Infrastructure.UnitTests.CacheDecorators;

using System.Threading;
using System.Threading.Tasks;
using External.UEX.Abstractions;
using Microsoft.Extensions.Logging;

public sealed class UexGameApiCacheDecorator(IUexGameApi gameApi, ILogger<UexGameApiCacheDecorator> logger) : UexApiCacheDecorator(logger), IUexGameApi
{
    public Task<UexApiResponse<GetCategoriesOkResponse>> GetCategoriesAsync(string? type = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                type,
            },
            nameof(GetCategoriesAsync),
            x => gameApi.GetCategoriesAsync(x.type, cancellationToken)
        );

    public Task<UexApiResponse<GetCategoriesAttributesOkResponse>> GetCategoriesAttributesAsync(
        double? id_category = null,
        CancellationToken cancellationToken = default
    )
        => CacheAsync(
            new
            {
                id_category,
            },
            nameof(GetCategoriesAttributesAsync),
            x => gameApi.GetCategoriesAttributesAsync(x.id_category, cancellationToken)
        );

    public Task<UexApiResponse<GetCitiesOkResponse>> GetCitiesAsync(double? id_moon = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_moon,
            },
            nameof(GetCitiesAsync),
            x => gameApi.GetCitiesAsync(x.id_moon, cancellationToken)
        );

    public Task<UexApiResponse<GetCompaniesOkResponse>> GetCompaniesAsync(string? is_item_manufacturer = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                is_item_manufacturer,
            },
            nameof(GetCompaniesAsync),
            x => gameApi.GetCompaniesAsync(x.is_item_manufacturer, cancellationToken)
        );


    public Task<UexApiResponse<GetContactsOkResponse>> GetContactsAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetContactsAsync),
            _ => gameApi.GetContactsAsync(cancellationToken)
        );

    public Task<UexApiResponse<GetContractsOkResponse>> GetContractsAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetContractsAsync),
            _ => gameApi.GetContractsAsync(cancellationToken)
        );

    public Task<UexApiResponse<GetFactionsOkResponse>> GetFactionsAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetFactionsAsync),
            _ => gameApi.GetFactionsAsync(cancellationToken)
        );

    public Task<UexApiResponse<GetGameVersionsOkResponse>> GetGameVersionsAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetGameVersionsAsync),
            _ => gameApi.GetGameVersionsAsync(cancellationToken)
        );

    public Task<UexApiResponse<GetJumpPointsOkResponse>> GetJumpPointsAsync(double? id_orbit_origin = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_orbit_origin,
            },
            nameof(GetJumpPointsAsync),
            x => gameApi.GetJumpPointsAsync(x.id_orbit_origin, cancellationToken)
        );

    public Task<UexApiResponse<GetJurisdictionsOkResponse>> GetJurisdictionsAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetJurisdictionsAsync),
            _ => gameApi.GetJurisdictionsAsync(cancellationToken)
        );

    public Task<UexApiResponse<GetMoonsOkResponse>> GetMoonsAsync(double? id_star_system = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_star_system,
            },
            nameof(GetMoonsAsync),
            x => gameApi.GetMoonsAsync(x.id_star_system, cancellationToken)
        );

    public Task<UexApiResponse<GetOrbitsOkResponse>> GetOrbitsAsync(double? id_star_system = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_star_system,
            },
            nameof(GetOrbitsAsync),
            x => gameApi.GetOrbitsAsync(x.id_star_system, cancellationToken)
        );

    public Task<UexApiResponse> GetOrbitsDistancesAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetOrbitsDistancesAsync),
            _ => gameApi.GetOrbitsDistancesAsync(cancellationToken)
        );

    public Task<UexApiResponse<GetOutpostsOkResponse>> GetOutpostsAsync(double? id_moon = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_moon,
            },
            nameof(GetOutpostsAsync),
            x => gameApi.GetOutpostsAsync(x.id_moon, cancellationToken)
        );

    public Task<UexApiResponse<GetPlanetsOkResponse>> GetPlanetsAsync(double? id_star_system = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_star_system,
            },
            nameof(GetPlanetsAsync),
            x => gameApi.GetPlanetsAsync(x.id_star_system, cancellationToken)
        );

    public Task<UexApiResponse<GetPoiOkResponse>> GetPoiAsync(double? id_outpost = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_outpost,
            },
            nameof(GetPoiAsync),
            x => gameApi.GetPoiAsync(x.id_outpost, cancellationToken)
        );

    public Task<UexApiResponse<GetReleaseNotesOkResponse>> GetReleaseNotesAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetReleaseNotesAsync),
            _ => gameApi.GetReleaseNotesAsync(cancellationToken)
        );

    public Task<UexApiResponse<GetSpaceStationsOkResponse>> GetSpaceStationsAsync(double? id_moon = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_moon,
            },
            nameof(GetSpaceStationsAsync),
            x => gameApi.GetSpaceStationsAsync(x.id_moon, cancellationToken)
        );

    public Task<UexApiResponse<GetStarSystemsOkResponse>> GetStarSystemsAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetStarSystemsAsync),
            _ => gameApi.GetStarSystemsAsync(cancellationToken)
        );

    public Task<UexApiResponse<GetTerminalsOkResponse>> GetTerminalsAsync(double? id_outpost = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_outpost,
            },
            nameof(GetTerminalsAsync),
            x => gameApi.GetTerminalsAsync(x.id_outpost, cancellationToken)
        );

    public Task<UexApiResponse> GetTerminalsDistancesAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetTerminalsDistancesAsync),
            _ => gameApi.GetTerminalsDistancesAsync(cancellationToken)
        );

    public Task<UexApiResponse<GetVehiclesOkResponse>> GetVehiclesAsync(double? id_company = null, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_company,
            },
            nameof(GetVehiclesAsync),
            x => gameApi.GetVehiclesAsync(x.id_company, cancellationToken)
        );
}
