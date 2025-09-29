namespace Arkanis.Overlay.Domain.Models.IconSelection;

/// <summary>
/// Defines the different sources/types of icons available in the system.
/// </summary>
public enum IconType
{
    /// <summary>
    /// Material Icons (Google's original icon set).
    /// </summary>
    MaterialIcons,

    /// <summary>
    /// Material Symbols (Google's newer icon set).
    /// </summary>
    MaterialSymbols,

    /// <summary>
    /// Custom icons specific to the application.
    /// </summary>
    Custom,

    /// <summary>
    /// Unknown or unsupported icon type.
    /// </summary>
    Undefined,
}


