namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Abstractions.Game;
using Attributes;
using Enums;
using Search;
using Trade;

[Description("Game Vehicle Entry")]
[CacheEntryDescription("Game Vehicles")]
public abstract class GameVehicle(
    int id,
    string fullName,
    string shortName,
    GameCompany manufacturer,
    GameEntityCategory vehicleCategory
) : GameEntity(UexApiGameEntityId.Create<GameVehicle>(id), vehicleCategory), IGameManufactured, IGamePurchasable, IGameRentable
{
    public GameCompany Manufacturer
        => manufacturer;

    public override GameEntityName Name { get; } = new(
        new GameEntityName.CompanyReference(manufacturer),
        new GameEntityName.NameWithShortVariant(fullName, shortName)
    );

    public GameTerminalType TerminalType
        => GameTerminalType.Item;

    public Bounds<PriceTag> LatestPurchasePrices { get; private set; } = new(PriceTag.Unknown, PriceTag.Unknown, PriceTag.Unknown);

    public void UpdatePurchasePrices(Bounds<PriceTag> newPrices)
        => LatestPurchasePrices = newPrices;

    public Bounds<PriceTag> LatestRentPrices { get; private set; } = new(PriceTag.Unknown, PriceTag.Unknown, PriceTag.Unknown);

    public void UpdateRentPrices(Bounds<PriceTag> newPrices)
        => LatestRentPrices = newPrices;

    protected override IEnumerable<SearchableTrait> CollectSearchableTraits()
    {
        yield return new SearchableName(fullName);
        yield return new SearchableManufacturer(manufacturer);
        foreach (var searchableAttribute in base.CollectSearchableTraits())
        {
            yield return searchableAttribute;
        }
    }
}

[Description("Game Space Ship Entry")]
[CacheEntryDescription("Game Space Ships")]
public class GameSpaceShip(int id, string fullName, string shortName, GameCompany manufacturer) : GameVehicle(
    id,
    fullName,
    shortName,
    manufacturer,
    GameEntityCategory.SpaceShip
);

[Description("Game Ground Vehicle Entry")]
[CacheEntryDescription("Game Ground Vehicles")]
public class GameGroundVehicle(int id, string fullName, string shortName, GameCompany manufacturer) : GameVehicle(
    id,
    fullName,
    shortName,
    manufacturer,
    GameEntityCategory.GroundVehicle
);
