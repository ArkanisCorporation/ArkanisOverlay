namespace Arkanis.Overlay.Domain.Models.Game;

using Common;

public record GameCurrency(int Amount) : IComparable<GameCurrency>, IFormattable
{
    public static readonly GameCurrency Zero = new(0);

    public static string Name
        => ApplicationConstants.CurrencyName;

    public static string ShortName
        => ApplicationConstants.CurrencyAbbr;

    public static string Symbol
        => ApplicationConstants.CurrencySymbol;

    public int CompareTo(GameCurrency? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        return other is not null
            ? Amount.CompareTo(other.Amount)
            : 1;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        var stringAmount = Amount.ToString(format, formatProvider);
        return $"{ApplicationConstants.CurrencySymbol}{stringAmount}";
    }

    public string ToString(string? format)
        => ToString(format, null);

    public override string ToString()
        => ToString(null, null);

    public static bool operator <(GameCurrency left, GameCurrency right)
        => ReferenceEquals(left, null)
            ? !ReferenceEquals(right, null)
            : left.CompareTo(right) < 0;

    public static bool operator <=(GameCurrency left, GameCurrency right)
        => ReferenceEquals(left, null) || left.CompareTo(right) <= 0;

    public static bool operator >(GameCurrency left, GameCurrency right)
        => !ReferenceEquals(left, null) && left.CompareTo(right) > 0;

    public static bool operator >=(GameCurrency left, GameCurrency right)
        => ReferenceEquals(left, null)
            ? ReferenceEquals(right, null)
            : left.CompareTo(right) >= 0;

    public static GameCurrency operator +(GameCurrency left, GameCurrency right)
        => new(left.Amount + right.Amount);

    public static GameCurrency operator -(GameCurrency left, GameCurrency right)
        => new(left.Amount - right.Amount);

    public static GameCurrency operator *(GameCurrency currency, int multiplier)
        => new(currency.Amount * multiplier);

    public static GameCurrency operator /(GameCurrency currency, int divisor)
        => new(currency.Amount / divisor);

    public static double operator /(GameCurrency left, GameCurrency right)
        => left.Amount / (double)right.Amount;
}
