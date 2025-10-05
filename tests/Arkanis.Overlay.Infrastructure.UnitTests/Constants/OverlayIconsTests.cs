namespace Arkanis.Overlay.Infrastructure.UnitTests.Constants;

using System.Reflection;
using Arkanis.Overlay.Domain.Constants;
using Arkanis.Overlay.Domain.Enums;
using Shouldly;

/// <summary>
/// Tests for OverlayIcons constants - verifies that GameEntity icon names match actual game entities.
/// </summary>
public sealed class OverlayIconsTests
{
    [Fact]
    public void GameEntity_Constants_ShouldMatchGameEntityCategoryNames()
    {
        // This test verifies the requirement:
        // "GameEntity.X must be names of the actual game entities"
        
        // ARRANGE - Get the constants that exist
        var gameEntityType = typeof(OverlayIcons.GameEntity);
        var constantNames = gameEntityType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string) && f.IsLiteral)
            .Select(f => f.Name)
            .ToHashSet();

        // ACT & ASSERT - Check that constants match entity names
        constantNames.ShouldContain("Commodity", "Should have constant for Commodity entity");
        constantNames.ShouldContain("SpaceShip", "Should have constant for SpaceShip entity");
        constantNames.ShouldContain("GroundVehicle", "Should have constant for GroundVehicle entity");
        constantNames.ShouldContain("Item", "Should have constant for Item entity");
        constantNames.ShouldContain("ProductCategory", "Should have constant for ProductCategory entity");
        constantNames.ShouldContain("Company", "Should have constant for Company entity");
        constantNames.ShouldContain("Location", "Should have constant for Location entity");
    }

    [Theory]
    [InlineData("Commodity", "GameEntity.Commodity")]
    [InlineData("SpaceShip", "GameEntity.SpaceShip")]
    [InlineData("GroundVehicle", "GameEntity.GroundVehicle")]
    [InlineData("Item", "GameEntity.Item")]
    [InlineData("ProductCategory", "GameEntity.ProductCategory")]
    [InlineData("Company", "GameEntity.Company")]
    [InlineData("Location", "GameEntity.Location")]
    public void GameEntity_Constants_ShouldHaveCorrectValues(string constantName, string expectedValue)
    {
        // ARRANGE
        var gameEntityType = typeof(OverlayIcons.GameEntity);
        var field = gameEntityType.GetField(constantName, BindingFlags.Public | BindingFlags.Static);

        // ACT & ASSERT
        field.ShouldNotBeNull($"Constant {constantName} should exist");
        var value = field!.GetValue(null) as string;
        value.ShouldBe(expectedValue);
    }
}
