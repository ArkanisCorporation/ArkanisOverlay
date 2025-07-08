namespace Arkanis.Overlay.Domain.Models.Game;

using Abstractions.Game;

public abstract record UexApiGameEntityId(int Identity) : TypedDomainId<int>(Identity)
{
    public override bool Equals(IDomainId? other)
        => other is UexApiGameEntityId && base.Equals(other);

    public static UexId<T> Create<T>(int identity) where T : IGameEntity
        => new(identity);

    public static UexId<T> Create<T>(double identity) where T : IGameEntity
        => new((int)identity);
}

public sealed record UexHangarEntryId(int Identity) : UexApiGameEntityId(Identity);

public sealed record UexId<T>(int Identity) : UexApiGameEntityId(Identity) where T : IGameEntity
{
    public override bool Equals(IDomainId? other)
        => other is UexId<T> && base.Equals(other);
}
