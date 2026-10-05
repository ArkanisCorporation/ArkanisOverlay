namespace Arkanis.Overlay.Infrastructure.UnitTests.CacheDecorators;

using System.Threading;
using System.Threading.Tasks;
using External.UEX.Abstractions;
using Microsoft.Extensions.Logging;

public sealed class UexItemsApiCacheDecorator(IUexItemsApi itemsApi, ILogger<UexItemsApiCacheDecorator> logger) : UexApiCacheDecorator(logger), IUexItemsApi
{
    public Task<UexApiResponse<GetItemsOkResponse>> GetItemsByCategoryAsync(string id_category, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_category,
            },
            nameof(GetItemsByCategoryAsync),
            x => itemsApi.GetItemsByCategoryAsync(x.id_category, cancellationToken)
        );

    public Task<UexApiResponse<GetItemsOkResponse>> GetItemsByCompanyAsync(string id_company, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_company,
            },
            nameof(GetItemsByCompanyAsync),
            x => itemsApi.GetItemsByCompanyAsync(x.id_company, cancellationToken)
        );

    public Task<UexApiResponse> GetItemsBySizeAsync(string size, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                size,
            },
            nameof(GetItemsBySizeAsync),
            x => itemsApi.GetItemsBySizeAsync(x.size, cancellationToken)
        );

    public Task<UexApiResponse<GetItemsOkResponse>> GetItemsByUuidAsync(string uuid, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                uuid,
            },
            nameof(GetItemsByUuidAsync),
            x => itemsApi.GetItemsByUuidAsync(x.uuid, cancellationToken)
        );

    public Task<UexApiResponse<GetItemsAttributesOkResponse>> GetItemsAttributesByCategoryAsync(
        string id_category,
        CancellationToken cancellationToken = default
    )
        => CacheAsync(
            new
            {
                id_category,
            },
            nameof(GetItemsAttributesByCategoryAsync),
            x => itemsApi.GetItemsAttributesByCategoryAsync(x.id_category, cancellationToken)
        );

    public Task<UexApiResponse<GetItemsAttributesOkResponse>> GetItemsAttributesByItemAsync(string id_item, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_item,
            },
            nameof(GetItemsAttributesByItemAsync),
            x => itemsApi.GetItemsAttributesByItemAsync(x.id_item, cancellationToken)
        );

    public Task<UexApiResponse<GetItemsAttributesOkResponse>> GetItemsAttributesByUuidAsync(string uuid, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                uuid,
            },
            nameof(GetItemsAttributesByUuidAsync),
            x => itemsApi.GetItemsAttributesByUuidAsync(x.uuid, cancellationToken)
        );

    public Task<UexApiResponse<GetItemsPricesOkResponse>> GetItemsPricesByCategoryAsync(string id_category, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_category,
            },
            nameof(GetItemsPricesByCategoryAsync),
            x => itemsApi.GetItemsPricesByCategoryAsync(x.id_category, cancellationToken)
        );

    public Task<UexApiResponse<GetItemsPricesOkResponse>> GetItemsPricesByItemAsync(string id_item, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_item,
            },
            nameof(GetItemsPricesByItemAsync),
            x => itemsApi.GetItemsPricesByItemAsync(x.id_item, cancellationToken)
        );

    public Task<UexApiResponse<GetItemsPricesOkResponse>> GetItemsPricesByTerminalAsync(string id_terminal, CancellationToken cancellationToken = default)
        => CacheAsync(
            new
            {
                id_terminal,
            },
            nameof(GetItemsPricesByTerminalAsync),
            x => itemsApi.GetItemsPricesByTerminalAsync(x.id_terminal, cancellationToken)
        );

    public Task<UexApiResponse<GetItemsPricesAllOkResponse>> GetItemsPricesAllAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetItemsPricesAllAsync),
            _ => itemsApi.GetItemsPricesAllAsync(cancellationToken)
        );
}
