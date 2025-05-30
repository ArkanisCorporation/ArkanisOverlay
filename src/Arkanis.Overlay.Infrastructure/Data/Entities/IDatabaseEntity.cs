namespace Arkanis.Overlay.Infrastructure.Data.Entities;

public interface IDatabaseEntity<T> where T : IEquatable<T>
{
    T Id { get; init; }
}
