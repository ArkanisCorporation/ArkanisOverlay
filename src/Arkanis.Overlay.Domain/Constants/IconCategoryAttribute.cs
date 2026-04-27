namespace Arkanis.Overlay.Domain.Constants;

using Models.IconSelection;

/// <summary>
/// Attribute used to mark icon category classes with their corresponding IconCategory enum value.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IconCategoryAttribute"/> class.
/// </remarks>
/// <param name="category">The icon category.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class IconCategoryAttribute(IconCategory category) : Attribute
{
    /// <summary>
    /// Gets the icon category that this class represents.
    /// </summary>
    public IconCategory Category { get; } = category;
}

