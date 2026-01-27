namespace Arkanis.Overlay.Domain.Abstractions.Services;

public interface IIconPicker
{
    public string PickIconFor<T>(T value);
}
