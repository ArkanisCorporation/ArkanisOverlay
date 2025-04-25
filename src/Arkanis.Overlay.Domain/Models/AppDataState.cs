namespace Arkanis.Overlay.Domain.Models;

using Game;

public abstract record AppDataState;

public record AppDataLoaded(SyncedGameDataState DataState, DateTimeOffset CreatedAt) : AppDataState;

public sealed record AppDataCached(SyncedGameDataState DataState, DateTimeOffset CreatedAt, DateTimeOffset CachedUntil)
    : AppDataLoaded(DataState, CreatedAt)
{
    public bool RefreshRequired { get; set; }
}

public sealed record AppDataMissing : AppDataState
{
    public static readonly AppDataState Instance = new AppDataMissing();

    private AppDataMissing()
    {
    }
}
