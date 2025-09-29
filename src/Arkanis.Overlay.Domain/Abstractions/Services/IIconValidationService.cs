namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models.IconSelection;

public interface IIconValidationService
{
    /// <summary>
    /// Returns true if the icon exists and can be rendered.
    /// </summary>
    bool IsValid(IconSelectionObject selection);
}


