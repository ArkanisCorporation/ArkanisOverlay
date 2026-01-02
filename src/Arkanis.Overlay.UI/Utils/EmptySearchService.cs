namespace Arkanis.Overlay.UI.Utils;

using Domain.Abstractions.Services;
using Domain.Models.Search;

public class EmptySearchService : StaticResultSearchService
{
    private EmptySearchService() : base(GameEntitySearchResults.Empty)
    {
    }

    public static ISearchService Instance { get; } = new EmptySearchService();
}
