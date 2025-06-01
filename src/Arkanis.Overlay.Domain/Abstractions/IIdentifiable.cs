namespace Arkanis.Overlay.Domain.Abstractions;

using Game;

public interface IIdentifiable
{
    IDomainId Id { get; }

    public class EqualityComparer : IEqualityComparer<IIdentifiable>
    {
        public static readonly EqualityComparer Default = new();

        public bool Equals(IIdentifiable? x, IIdentifiable? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            return x is not null
                   && y is not null
                   && x.Id.Equals(y.Id);
        }

        public int GetHashCode(IIdentifiable obj)
            => obj.Id.GetHashCode();
    }
}
