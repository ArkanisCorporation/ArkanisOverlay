namespace Arkanis.Overlay.Components.Services;

using Shared.Announcements;

public static class FeatureAnnouncementCatalog
{
    public static IReadOnlyList<FeatureAnnouncement> All { get; } =
    [
        new()
        {
            Id = "emergency-services",
            Revision = 1,
            Title = "Emergency reporting & rescue requests",
            ContentComponentType = typeof(EmergencyServicesAnnouncement),
            Enabled = true,
        },
    ];
}
