namespace Arkanis.Overlay.Infrastructure.Data.Mappers;

public interface IMapperWith<out T>
{
    T Reference { get; }
}
