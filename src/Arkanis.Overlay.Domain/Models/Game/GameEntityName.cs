namespace Arkanis.Overlay.Domain.Models.Game;

using System.Collections;

public sealed record GameEntityName(IEnumerable<GameEntityName.Part> Parts) : IEnumerable<GameEntityName.Part>
{
    private readonly Part[] _parts = Parts.ToArray();

    public GameEntityName(params Part[] parts) : this(parts.AsEnumerable())
    {
    }

    public Name MainContent
        => _parts.OfType<Name>().First();

    public CompanyReference? Company
        => _parts.OfType<CompanyReference>().FirstOrDefault();

    public LocationReference? Location
        => _parts.OfType<LocationReference>().FirstOrDefault();

    public PropertyCollection? Properties
        => _parts.OfType<PropertyCollection>().FirstOrDefault();

    public ItemCategoryReference? ItemCategory
        => _parts.OfType<ItemCategoryReference>().FirstOrDefault();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public IEnumerator<Part> GetEnumerator()
        => Parts.GetEnumerator();

    public static Reference ReferenceTo(GameEntity entity)
        => Reference.Create(entity);

    public abstract record Part;

    public abstract record Reference(GameEntity ReferencedEntity) : Part
    {
        public GameEntityName EntityName
            => ReferencedEntity.Name;

        public static Reference Create(GameEntity referencedEntity)
            => referencedEntity switch
            {
                GameProductCategory category => new ItemCategoryReference(category),
                GameCompany company => new CompanyReference(company),
                GameLocationEntity location => new LocationReference(location),
                _ => throw new ArgumentException($"Cannot create name reference for: {referencedEntity.GetType()}"),
            };
    }

    public interface IHasCode
    {
        string Code { get; }
    }

    public interface IHasShortName
    {
        string ShortName { get; }
    }

    public sealed record ItemCategoryReference(GameProductCategory Category) : Reference(Category);

    public sealed record CompanyReference(GameCompany Company) : Reference(Company);

    public sealed record LocationReference(GameLocationEntity Location) : Reference(Location);

    public record Name(string FullName) : Part;

    public record NameWithCode(string FullName, string Code) : Name(FullName), IHasCode;

    public sealed record NameWithCodeAndShortVariant(string FullName, string Code, string ShortName) : NameWithCode(FullName, Code), IHasShortName;

    public sealed record NameWithShortVariant(string FullName, string ShortName) : Name(FullName), IHasShortName;

    public sealed record PropertyCollection(params PropertyItem[] Items) : Part, IEnumerable<PropertyItem>
    {
        public int Count
            => Items.Length;

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public IEnumerator<PropertyItem> GetEnumerator()
            => Items.AsEnumerable().GetEnumerator();

        public static PropertyCollection Create(IEnumerable<GameItemTrait> traits)
        {
            var items = traits
                .Where(trait => !string.IsNullOrWhiteSpace(trait.Content))
                .Select(trait => trait.ToNamePart())
                .OrderBy(trait => trait.Key)
                .ToArray();

            return new PropertyCollection(items);
        }

        public static PropertyCollection Create(IEnumerable<PropertyItem> previous, IEnumerable<GameItemTrait> traits, IEnumerable<PropertyItem> next)
        {
            var items = traits
                .Where(trait => !string.IsNullOrWhiteSpace(trait.Content))
                .Select(trait => trait.ToNamePart())
                .OrderBy(trait => trait.Key);

            return new PropertyCollection(previous.Concat(items).Concat(next).ToArray());
        }
    }

    public sealed record PropertyItem(string Key, string Value) : Part;
}
