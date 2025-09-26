namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Mappers;

using Domain.Models.Inventory;
using Entities;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Mappers;
using MoreLinq;
using Services;
using Shouldly;

public class InventoryEntityMapperUnitTests
{
    [Theory]
    [MemberData(nameof(DatabaseInventoryEntities))]
    internal void Correctly_Maps_Base_Properties_For_All_Database_Types(InventoryEntryEntityBase sourceObject)
    {
        var uexMapper = new UexApiDtoMapper(new NoHydrationMockService());
        var ownableEntityRefMapper = new EntityReferenceMapper(uexMapper);
        var tradeRunEntityMapper = new TradeRunEntityMapper(ownableEntityRefMapper, uexMapper);
        GameEntityFixture.AllEntities.ForEach(uexMapper.CacheGameEntity);

        var mapper = new InventoryEntityMapper(ownableEntityRefMapper, tradeRunEntityMapper, uexMapper);

        var result = mapper.Map(sourceObject);

        result.Id.ShouldBe(sourceObject.Id);
        result.Quantity.Reference.EntityId.ShouldBe(sourceObject.Quantity.Reference.EntityId);
        result.Quantity.Amount.ShouldBe(sourceObject.Quantity.Amount);
        result.Quantity.Unit.ShouldBe(sourceObject.Quantity.Unit);

        if (sourceObject is LocationInventoryEntryEntity locationSource)
        {
            var physicalResult = result.ShouldBeOfType<LocationInventoryEntry>();
            physicalResult.Location.Id.ShouldBe(locationSource.LocationId);
        }
    }

    [Theory]
    [MemberData(nameof(DomainInventoryEntries))]
    internal void Correctly_Maps_Base_Properties_For_All_Domain_Types(InventoryEntryBase sourceObject)
    {
        var uexMapper = new UexApiDtoMapper(new NoHydrationMockService());
        var ownableEntityRefMapper = new EntityReferenceMapper(uexMapper);
        var tradeRunEntityMapper = new TradeRunEntityMapper(ownableEntityRefMapper, uexMapper);
        var mapper = new InventoryEntityMapper(ownableEntityRefMapper, tradeRunEntityMapper, uexMapper);

        var result = mapper.Map(sourceObject);

        result.Id.ShouldBe(sourceObject.Id);
        result.Quantity.Reference.EntityId.ShouldBe(sourceObject.Quantity.Reference.EntityId);
        result.Quantity.Amount.ShouldBe(sourceObject.Quantity.Amount);
        result.Quantity.Unit.ShouldBe(sourceObject.Quantity.Unit);

        if (sourceObject is LocationInventoryEntry locationSource)
        {
            var physicalResult = result.ShouldBeOfType<LocationInventoryEntryEntity>();
            physicalResult.LocationId.ShouldBe(locationSource.Location.Id);
        }
    }

    internal static IEnumerable<InventoryEntryEntityBase[]> DatabaseInventoryEntities()
        =>
        [
            [DatabaseInventoryEntitiesFixture.LocationItem1],
            //
            [DatabaseInventoryEntitiesFixture.LocationItem2],
            //
            [DatabaseInventoryEntitiesFixture.LocationItem3],
        ];

    internal static IEnumerable<InventoryEntryBase[]> DomainInventoryEntries()
        =>
        [
            [DomainInventoryEntriesFixture.LocationItem1],
            //
            [DomainInventoryEntriesFixture.LocationItem1],
            //
            [DomainInventoryEntriesFixture.LocationItem1],
        ];
}
