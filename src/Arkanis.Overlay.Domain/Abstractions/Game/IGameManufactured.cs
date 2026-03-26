namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Models.Game;

public interface IGameManufactured
{
    public GameCompany Manufacturer { get; }
}
