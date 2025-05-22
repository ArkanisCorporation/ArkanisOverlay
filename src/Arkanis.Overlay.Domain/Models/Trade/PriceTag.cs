namespace Arkanis.Overlay.Domain.Models.Trade;

using Abstractions.Game;
using Game;

public abstract record PriceTag : IComparable<PriceTag>
{
    public static readonly PriceTag Unknown = UnknownPriceTag.Instance;

    public int CompareTo(PriceTag? other)
        => this is BarePriceTag priceTage && other is BarePriceTag otherPriceTag
            ? priceTage.CompareTo(otherPriceTag)
            : -1;

    public static bool operator <(PriceTag left, PriceTag right)
        => ReferenceEquals(left, null)
            ? !ReferenceEquals(right, null)
            : left.CompareTo(right) < 0;

    public static bool operator <=(PriceTag left, PriceTag right)
        => ReferenceEquals(left, null) || left.CompareTo(right) <= 0;

    public static bool operator >(PriceTag left, PriceTag right)
        => !ReferenceEquals(left, null) && left.CompareTo(right) > 0;

    public static bool operator >=(PriceTag left, PriceTag right)
        => ReferenceEquals(left, null)
            ? ReferenceEquals(right, null)
            : left.CompareTo(right) >= 0;

    public static PriceTag MissingFor(IGameLocation location)
        => new MissingPriceTag(location);
}

public sealed record UnknownPriceTag : PriceTag
{
    private UnknownPriceTag()
    {
    }

    public static UnknownPriceTag Instance { get; } = new();
}

public sealed record MissingPriceTag(IGameLocation Location) : PriceTag;

public record BarePriceTag(GameCurrency Price) : PriceTag, IComparable<BarePriceTag>
{
    public int CompareTo(BarePriceTag? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        return other is not null
            ? Price.CompareTo(other.Price)
            : 1;
    }
}

public sealed record AggregatePriceTag(GameCurrency Price) : BarePriceTag(Price);

public record KnownPriceTag(GameCurrency Price, DateTimeOffset UpdatedAt) : BarePriceTag(Price)
{
    public static KnownPriceTag Create(int price, DateTimeOffset updatedAt)
        => new(new GameCurrency(price), updatedAt);

    public static bool operator <(KnownPriceTag left, KnownPriceTag right)
        => ReferenceEquals(left, null)
            ? !ReferenceEquals(right, null)
            : left.CompareTo(right) < 0;

    public static bool operator <=(KnownPriceTag left, KnownPriceTag right)
        => ReferenceEquals(left, null) || left.CompareTo(right) <= 0;

    public static bool operator >(KnownPriceTag left, KnownPriceTag right)
        => !ReferenceEquals(left, null) && left.CompareTo(right) > 0;

    public static bool operator >=(KnownPriceTag left, KnownPriceTag right)
        => ReferenceEquals(left, null)
            ? ReferenceEquals(right, null)
            : left.CompareTo(right) >= 0;
}

public sealed record KnownPriceTagWithLocation(GameCurrency Price, IGameLocation Location, DateTimeOffset UpdatedAt)
    : KnownPriceTag(Price, UpdatedAt), IGameLocatedAt;
