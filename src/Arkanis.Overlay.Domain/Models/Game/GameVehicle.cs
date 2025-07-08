namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using System.Globalization;
using Abstractions.Game;
using Attributes;
using Enums;
using Humanizer;
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
    private GameEntityName? _name;

    public int CargoCapacity { get; init; }

    public GameContainerSize MaxContainerSize { get; init; }

    public GamePadSize PadSize { get; init; }

    public bool SupportsDocking { get; init; }
    public bool SupportsCargoDeck { get; init; }

    public string? ImageUrl { get; set; }

    public GameCompany Manufacturer
        => manufacturer;

    public override GameEntityName Name
        => _name ??= new GameEntityName(
            new GameEntityName.NameWithShortVariant(fullName, shortName),
            new GameEntityName.CompanyReference(manufacturer),
            new GameEntityName.PropertyCollection(
                new GameEntityName.PropertyItem("Capacity (SCU)", CargoCapacity.ToString(CultureInfo.CurrentCulture)),
                new GameEntityName.PropertyItem("Max. Container", MaxContainerSize.Humanize()),
                new GameEntityName.PropertyItem("Pad Size", PadSize.Humanize())
            )
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
