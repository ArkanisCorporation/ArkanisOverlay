namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents organization settings.
/// </summary>
public class OrgSettings
{
    /// <summary>
    ///     The public organization settings.
    /// </summary>
    public required PublicOrgSettings Public { get; set; }
}

/// <summary>
///     Represents the public organization settings.
/// </summary>
public class PublicOrgSettings
{
    public ServiceStatus Status { get; set; }
    public bool EmergenciesEnabled { get; set; }
    public bool AnonymousAlertsEnabled { get; set; }
    public bool RegistrationEnabled { get; set; }
    public MessageOfTheDay? MessageOfTheDay { get; set; }
    public required LocationSettings LocationSettings { get; set; }
}

public class MessageOfTheDay
{
    public required string Message { get; set; }
    public DateRange? DateRange { get; set; }
}

public class DateRange
{
    public required DateTimeOffset StartDate { get; set; }
    public required DateTimeOffset EndDate { get; set; }
}

public class LocationSettings
{
    public required List<SpaceLocation> Locations { get; set; }
}

public class SpaceLocation
{
    public required string Name { get; set; }
    public SpaceLocationType Type { get; set; }
    public required List<SpaceLocation> Children { get; set; }
    public bool Enabled { get; set; }
}

public enum SpaceLocationType
{
    Unknown = 0,
    System = 1,
    Planet = 2,
    Moon = 3,
}

public enum ServiceStatus
{
    Unknown = 0,
    Healthy = 1,
    SlightlyDegraded = 2,
    HeavilyDegraded = 3,
    Offline = 4,
}
