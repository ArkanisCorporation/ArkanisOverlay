namespace Arkanis.Overlay.Domain.Models.IconSelection;

/// <summary>
/// Defines categories for organizing icons by their purpose in the application.
/// </summary>
public enum IconCategory
{
    /// <summary>
    /// Icons related to searching and filtering content.
    /// </summary>
    Search,

    /// <summary>
    /// Icons used for navigation and movement within the application.
    /// </summary>
    Navigation,

    /// <summary>
    /// Icons indicating status or state information.
    /// </summary>
    Status,

    /// <summary>
    /// Icons representing actions the user can perform.
    /// </summary>
    Action,

    /// <summary>
    /// Icons used to represent game entities (ships, commodities, etc.).
    /// </summary>
    GameEntity,

    /// <summary>
    /// Icons related to trading and commerce functionality.
    /// </summary>
    Trade,

    /// <summary>
    /// Icons for system and application features.
    /// </summary>
    System,

    /// <summary>
    /// Unknown or unclassified icons.
    /// </summary>
    Undefined,
}


