namespace Arkanis.Overlay.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CacheEntryDescriptionAttribute(string title) : Attribute
{
    public string Title { get; } = title;
}
