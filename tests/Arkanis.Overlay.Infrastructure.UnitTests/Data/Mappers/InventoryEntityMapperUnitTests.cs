namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Mappers;

using Domain.Models.Inventory;
using Entities;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Mappers;
using MoreLinq;
using Services;
using Shouldly;

#pragma warning disable xUnit1016 // MemberData must reference a public member. Add or change the visibility of the data member to public.
#pragma warning disable xUnit1019 // MemberData must reference a data type assignable to 'System.Collections.Generic.IEnumerable<object[]>'.
public class InventoryEntityMapperUnitTests
{
    [Theory]
    [MemberData(nameof(DatabaseInventoryEntities))]
    internal void Correctly_Maps_Base_Properties_For_All_Database_Types(InventoryEntryEntityBase sourceObject)
    {
        var uexMapper = new UexApiDtoMapper(new NoHydrationMockService());
        GameEntityFixture.AllEntities.ForEach(uexMapper.CacheGameEntity);

        var mapper = new InventoryEntityMapper(uexMapper);

        var result = mapper.Map(sourceObject);

        result.Id.ShouldBe(sourceObject.Id);
        result.Quantity.ShouldBe(sourceObject.Quantity);

        if (sourceObject is ItemInventoryEntryEntityBase itemSource)
        {
            var itemResult = result.ShouldBeAssignableTo<ItemInventoryEntry>();
            var item = itemResult.Entity.ShouldNotBeNull();
            item.Id.ShouldBe(itemSource.ItemId);

            if (itemSource is PhysicalItemInventoryEntryEntity physicalItemSource)
            {
                var physicalResult = result.ShouldBeOfType<PhysicalItemInventoryEntry>();
                physicalResult.Location.Id.ShouldBe(physicalItemSource.LocationId);
            }
        }
    }

    [Theory]
    [MemberData(nameof(DomainInventoryEntries))]
    internal void Correctly_Maps_Base_Properties_For_All_Domain_Types(InventoryEntryBase sourceObject)
    {
        var uexMapper = new UexApiDtoMapper(new NoHydrationMockService());
        var mapper = new InventoryEntityMapper(uexMapper);

        var result = mapper.Map(sourceObject);

        result.Id.ShouldBe(sourceObject.Id);
        result.Quantity.ShouldBe(sourceObject.Quantity);

        if (sourceObject is ItemInventoryEntry itemSource)
        {
            var itemResult = result.ShouldBeAssignableTo<ItemInventoryEntryEntityBase>();
            itemResult.ItemId.ShouldBe(itemSource.Item.Id);

            if (itemSource is PhysicalItemInventoryEntry physicalSource)
            {
                var physicalResult = result.ShouldBeOfType<PhysicalItemInventoryEntryEntity>();
                physicalResult.LocationId.ShouldBe(physicalSource.Location.Id);
            }
        }
    }

    internal static IEnumerable<InventoryEntryEntityBase[]> DatabaseInventoryEntities()
        =>
        [
            [DatabaseInventoryEntitiesFixture.PhysicalItem1],
            //
            [DatabaseInventoryEntitiesFixture.PhysicalItem2],
            //
            [DatabaseInventoryEntitiesFixture.PhysicalItem3],
        ];

    internal static IEnumerable<InventoryEntryBase[]> DomainInventoryEntries()
        =>
        [
            [DomainInventoryEntriesFixture.PhysicalItem1],
            //
            [DomainInventoryEntriesFixture.PhysicalItem2],
            //
            [DomainInventoryEntriesFixture.PhysicalItem3],
        ];
}
#pragma warning restore xUnit1019
#pragma warning restore xUnit1016
