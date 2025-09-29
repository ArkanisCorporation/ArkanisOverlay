namespace Arkanis.Overlay.Infrastructure.Services.IconManagement;

using Arkanis.Overlay.Domain.Abstractions.Services;
using Arkanis.Overlay.Domain.Models.IconSelection;

public sealed class IconValidationService : IIconValidationService
{
    public bool IsValid(IconSelectionObject selection)
    {
        // For now, assume all known types are valid if name is not empty.
        return !string.IsNullOrWhiteSpace(selection.IconName);
    }
}


