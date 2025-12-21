namespace Arkanis.Overlay.Domain.Constants;

/// <summary>
/// Attribute used to provide descriptions for icon constants.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IconDescriptionAttribute"/> class.
/// </remarks>
/// <param name="description">The icon description.</param>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class IconDescriptionAttribute(string description) : Attribute
{
    /// <summary>
    /// Gets the description of the icon.
    /// </summary>
    public string Description { get; } = description;
}

