namespace Arkanis.Overlay.Domain.Models.Trade;

using Abstractions.Game;
using Game;

public abstract record PriceTag : IComparable<PriceTag>
{
    public static readonly PriceTag Unknown = new UnknownPriceTag();

    public int CompareTo(PriceTag? other)
        => this is PriceTagWithValue priceTage && other is PriceTagWithValue otherPriceTag
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

public sealed record UnknownPriceTag : PriceTag;

public sealed record MissingPriceTag(IGameLocation Location) : PriceTag;

public abstract record PriceTagWithValue(GameCurrency Price) : PriceTag, IComparable<PriceTagWithValue>
{
    public int CompareTo(PriceTagWithValue? other)
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

public sealed record AggregatePriceTag(GameCurrency Price) : PriceTagWithValue(Price);

public sealed record KnownPriceTag(GameCurrency Price, IGameLocation Location, DateTimeOffset UpdatedAt) : PriceTagWithValue(Price)
{
    public static KnownPriceTag Create(int price, IGameLocation location, DateTimeOffset updatedAt)
        => new(new GameCurrency(price), location, updatedAt);

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
