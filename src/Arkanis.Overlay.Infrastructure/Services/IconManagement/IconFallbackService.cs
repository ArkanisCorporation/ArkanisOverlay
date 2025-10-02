namespace Arkanis.Overlay.Infrastructure.Services.IconManagement;

using Arkanis.Overlay.Domain.Abstractions.Services;
using Arkanis.Overlay.Domain.Models.IconSelection;

public sealed class IconFallbackService : IIconFallbackService
{
    public string GetFallbackIconString(IconSelectionObject selection)
    {
        // Try provided fallbacks, then global default.
        foreach (var fallback in selection.FallbackIcons)
        {
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                // Return the fallback icon name for the UI layer to resolve
                return GetIconNameForType(selection, fallback);
            }
        }

        return "Square";
    }

    private static string GetIconNameForType(IconSelectionObject selection, string iconName)
        => selection.Type switch
        {
            IconType.MaterialSymbols => iconName,
            IconType.MaterialIcons => iconName,
            IconType.Custom => $"custom:{iconName}",
            _ => "Square",
        };
}


