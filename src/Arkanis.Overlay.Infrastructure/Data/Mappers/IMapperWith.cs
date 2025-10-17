namespace Arkanis.Overlay.Infrastructure.Data.Mappers;

public interface IMapperWith<out T>
{
    public T Reference { get; }
}
