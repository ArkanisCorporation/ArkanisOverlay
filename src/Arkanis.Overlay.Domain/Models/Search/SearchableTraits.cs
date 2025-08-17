namespace Arkanis.Overlay.Domain.Models.Search;

using Abstractions.Game;
using Enums;
using Game;

/// <summary>
///     Searchable traits represent unique characteristics of the subject that can be easily used for searching.
///     This model simplifies how the search is performed based on the trait type.
/// </summary>
public abstract record SearchableTrait;

public sealed record UnknownTrait : SearchableTrait
{
    private UnknownTrait()
    {
    }

    public static UnknownTrait Instance { get; } = new();
}

public abstract record SearchableTextTrait(string Content) : SearchableTrait
{
    public string NormalizedContent { get; } = Content.ToLowerInvariant();
}

public sealed record SearchableName(string Name) : SearchableTextTrait(Name)
{
    public SearchableName(GameEntityName Name) : this(Name.MainContent.FullName)
    {
    }
}

public sealed record SearchableCode(string Code) : SearchableTextTrait(Code);

public sealed record SearchableLocation(IGameLocation Location) : SearchableTrait;

public sealed record SearchableManufacturer(GameCompany Manufacturer) : SearchableTrait;

public sealed record SearchableProductCategory(GameProductCategory Category) : SearchableTrait;

public sealed record SearchableEntityCategory(GameEntityCategory Category) : SearchableTrait;
