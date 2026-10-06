namespace Arkanis.Overlay.Infrastructure.UnitTests.DomainModels;

using Arkanis.Overlay.Domain.Models.Inventory;
using Data.Entities;
using Shouldly;

/// <summary>
///     Covers <see cref="InventoryEntryExtensions.GetPlacementLocation" />, which resolves the physical game
///     location an inventory entry sits at. Vehicle-placed entries (cargo/modules) resolve to their containing
///     vehicle's location; without this the search view cannot surface them (they are not
///     <see cref="Domain.Abstractions.Game.IGameLocatedAt" /> like location/hangar entries are).
/// </summary>
public class InventoryEntryPlacementLocationTests
{
    [Fact]
    public void Location_Entry_Resolves_To_Its_Own_Location()
    {
        var entry = InventoryEntry.CreateAt(GameEntityFixture.Item1, new Quantity(1, Quantity.UnitType.Count), GameEntityFixture.SpaceStation);

        entry.GetPlacementLocation().ShouldBe(GameEntityFixture.SpaceStation);
    }

    [Fact]
    public void Hangar_Entry_Resolves_To_Its_Own_Location()
    {
        var hangarEntry = InventoryEntry.CreateAt(GameEntityFixture.SpaceShip, GameEntityFixture.SpaceStation);

        hangarEntry.GetPlacementLocation().ShouldBe(GameEntityFixture.SpaceStation);
    }

    [Fact]
    public void Vehicle_Cargo_Entry_Resolves_To_Its_Vehicle_Location()
    {
        var hangarEntry = InventoryEntry.CreateAt(GameEntityFixture.SpaceShip, GameEntityFixture.SpaceStation);
        var cargo = InventoryEntry.CreateCargo(GameEntityFixture.Item1, new Quantity(3, Quantity.UnitType.Count), hangarEntry);

        cargo.GetPlacementLocation().ShouldBe(GameEntityFixture.SpaceStation);
    }

    [Fact]
    public void Vehicle_Module_Entry_Resolves_To_Its_Vehicle_Location()
    {
        var hangarEntry = InventoryEntry.CreateAt(GameEntityFixture.SpaceShip, GameEntityFixture.SpaceStation);
        var module = InventoryEntry.CreateModule(GameEntityFixture.Item1, new Quantity(1, Quantity.UnitType.Count), hangarEntry);

        module.GetPlacementLocation().ShouldBe(GameEntityFixture.SpaceStation);
    }

    [Fact]
    public void Virtual_Entry_Has_No_Placement_Location()
    {
        var entry = InventoryEntry.Create(GameEntityFixture.Item1, new Quantity(1, Quantity.UnitType.Count));

        entry.GetPlacementLocation().ShouldBeNull();
    }
}
