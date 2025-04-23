namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Mappers;

using Domain.Enums;
using Domain.Models.Game;
using Infrastructure.Data.Exceptions;
using Infrastructure.Data.Mappers;
using Infrastructure.Services.Abstractions;
using Services;
using Shouldly;
using static ExternalUexDTOFixture;

public class ExternalUexDTOMapperUnitTests
{
    private static IGameEntityHydrationService HydrationService
        => new NoHydrationMockService();

    [Fact]
    public async Task UniverseStartSystemDTO_ToGameEntity_Should_Correctly_Map_Without_Dependencies()
    {
        var mapper = new UexApiDtoMapper(HydrationService);
        var source = StarSystem;

        var result = await mapper.ToGameEntityAsync(source);
        result.Parent.ShouldBeNull();
        result.Name.ShouldHaveSingleItem()
            .ShouldBeEquivalentTo(new GameEntityName.NameWithCode(source.Name!, source.Code!));
    }

    [Fact]
    public async Task UniverseTerminalDTO_ToGameEntity_Should_Correctly_Map_And_Link_Dependencies()
    {
        var mapper = new UexApiDtoMapper(HydrationService);

        // cache dependencies, order is important
        await mapper.ToGameEntityAsync(StarSystem);
        await mapper.ToGameEntityAsync(Planet);
        await mapper.ToGameEntityAsync(Moon);
        await mapper.ToGameEntityAsync(Outpost);

        var source = OutpostCommodityTerminal;
        var result = await mapper.ToGameEntityAsync(source);

        result.Type.ShouldBe(GameTerminalType.Commodity);
        result.EntityCategory.ShouldBe(GameEntityCategory.Location);
        result.Parent.ShouldNotBeNull() // outpost
            .Parent.ShouldNotBeNull() // moon
            .Parent.ShouldNotBeNull() // planet
            .Parent.ShouldNotBeNull(); // star system
    }

    [Fact]
    public async Task UniverseTerminalDTO_ToGameEntity_Should_Throw_When_Parent_Is_Missing()
    {
        var mapper = new UexApiDtoMapper(HydrationService);

        // cache dependencies
        await mapper.ToGameEntityAsync(StarSystem);
        await mapper.ToGameEntityAsync(Planet);
        await mapper.ToGameEntityAsync(Moon);

        var source = OutpostCommodityTerminal;
        await Should.ThrowAsync<ObjectMappingMissingDependentObjectException>(async () => await mapper.ToGameEntityAsync(source));
    }

    [Fact]
    public async Task ItemDTO_ToGameEntity_Should_Correctly_Map_And_Link_Dependencies()
    {
        var mapper = new UexApiDtoMapper(HydrationService);

        // cache dependencies, order is important
        await mapper.ToGameEntityAsync(ItemCompany);
        await mapper.ToGameEntityAsync(ItemHatsCategory);

        var source = Item;
        var result = await mapper.ToGameEntityAsync(source);
        result.Manufacturer.ShouldNotBeNull();
        result.TerminalType.ShouldBe(GameTerminalType.Item);
        result.EntityCategory.ShouldBe(GameEntityCategory.Item);
    }

    [Fact]
    public async Task ItemDTO_ToGameEntity_Should_Throw_When_Parent_Is_Missing()
    {
        var mapper = new UexApiDtoMapper(HydrationService);

        // cache dependencies
        await mapper.ToGameEntityAsync(StarSystem);
        await mapper.ToGameEntityAsync(Planet);
        await mapper.ToGameEntityAsync(Moon);

        var source = Item;
        await Should.ThrowAsync<ObjectMappingMissingDependentObjectException>(async () => await mapper.ToGameEntityAsync(source));
    }
}
