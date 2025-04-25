namespace Arkanis.Overlay.Infrastructure.UnitTests;

using Domain.Models;

public static class TestFixtures
{
    public static readonly StarCitizenVersion GameVersion = StarCitizenVersion.Create("4.1.0");

    public static readonly ServiceAvailableState ServiceAvailableState = new(GameVersion, DateTimeOffset.UtcNow);

    public static readonly DataLoaded DataLoaded = new(ServiceAvailableState, DateTimeOffset.UtcNow);

    public static readonly DataCached DataCached = new(ServiceAvailableState, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow + TimeSpan.FromMinutes(15));
}
