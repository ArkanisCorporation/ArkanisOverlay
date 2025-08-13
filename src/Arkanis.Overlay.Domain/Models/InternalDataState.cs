namespace Arkanis.Overlay.Domain.Models;

/// <summary>
///     Represents internal data state of the application modules.
/// </summary>
public abstract record InternalDataState;

public record DataLoaded(ServiceAvailableState SourcedState, DateTimeOffset CreatedAt) : InternalDataState;

public sealed record DataCached(ServiceAvailableState SourcedState, DateTimeOffset CreatedAt, DateTimeOffset CachedUntil)
    : DataLoaded(SourcedState, CreatedAt)
{
    public bool RefreshRequired { get; set; }
}

public sealed record DataProcessingErrored(Exception Exception) : InternalDataState;

public sealed record DataMissing : InternalDataState
{
    /// <summary>
    ///     Used to represent missing data.
    /// </summary>
    public static readonly InternalDataState Instance = new DataMissing();

    /// <summary>
    ///     Used to represent initially missing data allowing fast tracked initialization.
    /// </summary>
    public static readonly InternalDataState Initial = new DataMissing
    {
        FastTrackedInitialization = true,
    };

    private DataMissing()
    {
    }

    public bool FastTrackedInitialization { get; init; }
}
