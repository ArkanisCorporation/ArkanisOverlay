namespace Arkanis.Overlay.Domain.Models.Inventory;

using System.ComponentModel;
using Humanizer;

public record Quantity(int Amount, Quantity.UnitType Unit) : IComparable, IComparable<Quantity>
{
    public enum UnitType
    {
        Undefined,

        [Description("cSCU")]
        CentiStandardCargoUnit,

        [Description("SCU")]
        StandardCargoUnit,
        Count,
    }

    public int Amount { get; set; } = Amount;

    public UnitType Unit { get; set; } = Unit;

    public int CompareTo(object? other)
        => other is Quantity otherQuantity
            ? CompareTo(otherQuantity)
            : 0;

    public int CompareTo(Quantity? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return 1;
        }

        var unitComparison = Unit.CompareTo(other.Unit);
        return unitComparison != 0
            ? unitComparison
            : Amount.CompareTo(other.Amount);
    }

    public override string ToString()
        => $"{Amount.ToMetric(MetricNumeralFormats.WithSpace, 3)} {GetUnitString(Unit)}";

    public static string GetUnitString(UnitType unit)
        => unit switch
        {
            UnitType.Count => "\u00d7", // multiplication sign
            _ => unit.Humanize(),
        };

    public static bool operator <(Quantity left, Quantity right)
        => ReferenceEquals(left, null)
            ? !ReferenceEquals(right, null)
            : left.CompareTo(right) < 0;

    public static bool operator <=(Quantity left, Quantity right)
        => ReferenceEquals(left, null) || left.CompareTo(right) <= 0;

    public static bool operator >(Quantity left, Quantity right)
        => !ReferenceEquals(left, null) && left.CompareTo(right) > 0;

    public static bool operator >=(Quantity left, Quantity right)
        => ReferenceEquals(left, null)
            ? ReferenceEquals(right, null)
            : left.CompareTo(right) >= 0;
}
