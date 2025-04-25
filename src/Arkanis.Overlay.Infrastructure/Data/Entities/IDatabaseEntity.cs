namespace Arkanis.Overlay.Infrastructure.Data.Entities;

public interface IDatabaseEntity<T>
{
    T Id { get; init; }
}
