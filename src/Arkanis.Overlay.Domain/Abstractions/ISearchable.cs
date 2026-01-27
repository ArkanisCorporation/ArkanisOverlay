namespace Arkanis.Overlay.Domain.Abstractions;

using Models.Search;

public interface ISearchable
{
    public IEnumerable<SearchableTrait> SearchableAttributes { get; }
}
