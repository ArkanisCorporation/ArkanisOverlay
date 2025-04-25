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

public sealed record DataMissing : InternalDataState
{
    public static readonly InternalDataState Instance = new DataMissing();

    private DataMissing()
    {
    }
}
