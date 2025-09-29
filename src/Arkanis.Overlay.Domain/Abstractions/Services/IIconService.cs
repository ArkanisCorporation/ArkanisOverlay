namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models.IconSelection;

public interface IIconService
{
    /// <summary>
    /// Returns a MudBlazor-compatible icon string for the given selection object.
    /// </summary>
    string GetIconString(IconSelectionObject selection);

    /// <summary>
    /// Picks an icon string for a domain value (e.g., enums like PriceType).
    /// </summary>
    string GetIconFor<T>(T value);
}


