namespace Arkanis.Overlay.Domain.Models;

/// <summary>
///     Represents the state of an external service and the data it has available.
/// </summary>
public record ExternalServiceState;

public sealed record ServiceAvailableState(StarCitizenVersion Version, DateTimeOffset UpdatedAt) : ExternalServiceState;

public sealed record ServiceUnavailableState(Exception? Exception) : ExternalServiceState;
