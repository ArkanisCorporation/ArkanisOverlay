namespace Arkanis.Overlay.Domain.Abstractions;

using Game;

public interface IIdentifiable
{
    IDomainId Id { get; }

    public class EqualityComparer<T> : EqualityComparer, IEqualityComparer<T> where T : IIdentifiable
    {
        public bool Equals(T? x, T? y)
            => Equals(x as IIdentifiable, y);

        public int GetHashCode(T obj)
            => GetHashCode(obj as IIdentifiable);
    }

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

        public static EqualityComparer<T> For<T>() where T : IIdentifiable
            => new();
    }
}
