namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Converters;

using Domain.Models.Game;
using Infrastructure.Data.Converters;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

public class UexApiDomainIdConverterUnitTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void Can_Serialize_And_Deserialize_Base()
    {
        var converter = new UexApiDomainIdConverter();
        var sourceId = UexApiGameEntityId.Create<GameItem>(10);

        var provider = converter.ConvertToProviderTyped(sourceId);
        outputHelper.WriteLine(provider);

        var convertedId = converter.ConvertFromProviderTyped(provider);

        convertedId.ShouldBeEquivalentTo(sourceId);
    }

    [Fact]
    public void Can_Serialize_And_Deserialize_Generic()
    {
        var converter = new UexApiDomainIdConverter<GameItem>();
        var sourceId = UexApiGameEntityId.Create<GameItem>(10);

        var provider = converter.ConvertToProviderTyped(sourceId);
        outputHelper.WriteLine(provider);

        var convertedId = converter.ConvertFromProviderTyped(provider);

        convertedId.ShouldBeEquivalentTo(sourceId);
    }
}
