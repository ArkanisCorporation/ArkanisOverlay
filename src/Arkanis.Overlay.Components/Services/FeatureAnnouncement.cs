namespace Arkanis.Overlay.Components.Services;

using System.Globalization;

public sealed record FeatureAnnouncement
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required Type ContentComponentType { get; init; }
    public int Revision { get; init; } = 1;
    public int Priority { get; init; }
    public bool Enabled { get; init; } = true;
    public DateTimeOffset? AvailableFrom { get; init; }
    public DateTimeOffset? AvailableUntil { get; init; }

    public string DismissalKey => $"{Id}:{Revision.ToString(CultureInfo.InvariantCulture)}";

    public bool IsActive(DateTimeOffset now)
        => Enabled && (AvailableFrom is null || AvailableFrom <= now) && (AvailableUntil is null || now < AvailableUntil);
}
