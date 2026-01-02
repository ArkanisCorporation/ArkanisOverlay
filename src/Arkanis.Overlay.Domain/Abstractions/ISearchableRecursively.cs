namespace Arkanis.Overlay.Domain.Abstractions;

public interface ISearchableRecursively : ISearchable
{
    public ISearchableRecursively? Parent { get; }
}
