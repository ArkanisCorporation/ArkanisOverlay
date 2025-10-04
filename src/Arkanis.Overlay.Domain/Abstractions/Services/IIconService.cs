namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models.IconSelection;

public interface IIconService
{
    /// <summary>
    /// Returns an IconSelectionObject for the given unique icon identifier.
    /// </summary>
    IconSelectionObject GetIconSelection(string iconIdentifier);

    /// <summary>
    /// Returns an IconSelectionObject for a domain value (e.g., enums like PriceType).
    /// </summary>
    IconSelectionObject GetIconSelectionFor<T>(T value);
}


