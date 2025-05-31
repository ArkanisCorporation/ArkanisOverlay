namespace Arkanis.Overlay.Infrastructure.Data.Entities.Abstractions;

public interface IDatabaseEntity : IEquatable<IDatabaseEntity>;

public interface IDatabaseEntity<T> : IDatabaseEntity where T : IEquatable<T>
{
    T Id { get; init; }

    bool IEquatable<IDatabaseEntity>.Equals(IDatabaseEntity? other)
        => other is IDatabaseEntity<T> otherEntity
           && Id.Equals(otherEntity.Id);
}
