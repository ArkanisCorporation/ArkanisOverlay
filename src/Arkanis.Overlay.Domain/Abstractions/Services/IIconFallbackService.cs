namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models.IconSelection;

public interface IIconFallbackService
{
    /// <summary>
    /// Provides a fallback icon string for a selection when the primary is unavailable.
    /// </summary>
    string GetFallbackIconString(IconSelectionObject selection);
}


