namespace Arkanis.Overlay.Domain.Models;

using Game;

public abstract record AppDataState;

public record AppDataLoaded(GameDataState DataState, DateTimeOffset CreatedAt) : AppDataState;

public sealed record AppDataCached(GameDataState DataState, DateTimeOffset CreatedAt, DateTimeOffset CachedUntil)
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
