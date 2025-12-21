namespace Arkanis.Overlay.Infrastructure.UnitTests.Helpers;

using Arkanis.Overlay.Components.Helpers;
using Arkanis.Overlay.Domain.Enums;
using Infrastructure.Services.IconManagement;
using Shouldly;

public sealed class IconPickerUnitTests
{
    private readonly IconPicker _sut;

    public IconPickerUnitTests()
    {
        var iconService = new IconService();
        _sut = new IconPicker(iconService);
    }

    [Fact]
    public void PickIconFor_GameEntityCategory_SpaceShip_ReturnsExpectedMudBlazorIcon()
    {
        // Act
        var result = _sut.PickIconFor(GameEntityCategory.SpaceShip);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Rocket);
    }

    [Fact]
    public void PickIconFor_GameEntityCategory_Commodity_ReturnsExpectedMudBlazorIcon()
    {
        // Act
        var result = _sut.PickIconFor(GameEntityCategory.Commodity);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Diamond);
    }

    [Fact]
    public void PickIconFor_GameEntityCategory_GroundVehicle_ReturnsExpectedMudBlazorIcon()
    {
        // Act
        var result = _sut.PickIconFor(GameEntityCategory.GroundVehicle);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.LocalShipping);
    }

    [Fact]
    public void PickIconFor_GameEntityCategory_Item_ReturnsExpectedMudBlazorIcon()
    {
        // Act
        var result = _sut.PickIconFor(GameEntityCategory.Item);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Category);
    }

    [Fact]
    public void PickIconFor_GameEntityCategory_ProductCategory_ReturnsExpectedMudBlazorIcon()
    {
        // Act
        var result = _sut.PickIconFor(GameEntityCategory.ProductCategory);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Topic);
    }

    [Fact]
    public void PickIconFor_GameEntityCategory_Company_ReturnsExpectedMudBlazorIcon()
    {
        // Act
        var result = _sut.PickIconFor(GameEntityCategory.Company);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Domain);
    }

    [Fact]
    public void PickIconFor_GameEntityCategory_Location_ReturnsExpectedMudBlazorIcon()
    {
        // Act
        var result = _sut.PickIconFor(GameEntityCategory.Location);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Public);
    }

    [Fact]
    public void PickIconFor_PriceType_Buy_ReturnsExpectedMudBlazorIcon()
    {
        // Act
        var result = _sut.PickIconFor(PriceType.Buy);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.AddShoppingCart);
    }

    [Fact]
    public void PickIconFor_PriceType_Sell_ReturnsExpectedMudBlazorIcon()
    {
        // Act
        var result = _sut.PickIconFor(PriceType.Sell);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.RemoveShoppingCart);
    }

    [Fact]
    public void PickIconFor_PriceType_Rent_ReturnsExpectedMudBlazorIcon()
    {
        // Act
        var result = _sut.PickIconFor(PriceType.Rent);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.CarRental);
    }

    [Fact]
    public void PickIconFor_Generic_GameEntityCategory_ReturnsExpectedIcon()
    {
        // Arrange
        GameEntityCategory value = GameEntityCategory.SpaceShip;

        // Act
        var result = _sut.PickIconFor<GameEntityCategory>(value);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Rocket);
    }

    [Fact]
    public void PickIconFor_Generic_PriceType_ReturnsExpectedIcon()
    {
        // Arrange
        PriceType value = PriceType.Buy;

        // Act
        var result = _sut.PickIconFor<PriceType>(value);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.AddShoppingCart);
    }

    [Fact]
    public void PickIconFor_UnsupportedType_ReturnsDefaultIcon()
    {
        // Arrange
        var unsupportedValue = "Some string value";

        // Act
        var result = _sut.PickIconFor(unsupportedValue);

        // Assert
        result.ShouldBe(MudBlazorIconMapping.DefaultIconString);
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Square);
    }

    [Fact]
    public void PickIconFor_UnsupportedType_Integer_ReturnsDefaultIcon()
    {
        // Arrange
        var unsupportedValue = 42;

        // Act
        var result = _sut.PickIconFor(unsupportedValue);

        // Assert
        result.ShouldBe(MudBlazorIconMapping.DefaultIconString);
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Square);
    }

    [Fact]
    public void PickIconFor_AllGameEntityCategories_ReturnValidIcons()
    {
        // Arrange
        var categories = new[]
        {
            GameEntityCategory.Commodity,
            GameEntityCategory.SpaceShip,
            GameEntityCategory.GroundVehicle,
            GameEntityCategory.Item,
            GameEntityCategory.ProductCategory,
            GameEntityCategory.Company,
            GameEntityCategory.Location
        };

        // Act & Assert
        foreach (var category in categories)
        {
            var result = _sut.PickIconFor(category);
            result.ShouldNotBeNullOrEmpty();
            result.ShouldNotBe(MudBlazorIconMapping.DefaultIconString);
        }
    }

    [Fact]
    public void PickIconFor_AllPriceTypes_ReturnValidIcons()
    {
        // Arrange
        var priceTypes = new[]
        {
            PriceType.Buy,
            PriceType.Sell,
            PriceType.Rent
        };

        // Act & Assert
        foreach (var priceType in priceTypes)
        {
            var result = _sut.PickIconFor(priceType);
            result.ShouldNotBeNullOrEmpty();
            result.ShouldNotBe(MudBlazorIconMapping.DefaultIconString);
        }
    }

    [Theory]
    [InlineData(GameEntityCategory.SpaceShip)]
    [InlineData(GameEntityCategory.Commodity)]
    [InlineData(GameEntityCategory.GroundVehicle)]
    [InlineData(GameEntityCategory.Item)]
    [InlineData(GameEntityCategory.ProductCategory)]
    [InlineData(GameEntityCategory.Company)]
    [InlineData(GameEntityCategory.Location)]
    public void PickIconFor_GameEntityCategory_Theory_ReturnsValidIcon(GameEntityCategory category)
    {
        // Act
        var result = _sut.PickIconFor(category);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldNotBe(MudBlazorIconMapping.DefaultIconString);
    }

    [Theory]
    [InlineData(PriceType.Buy)]
    [InlineData(PriceType.Sell)]
    [InlineData(PriceType.Rent)]
    public void PickIconFor_PriceType_Theory_ReturnsValidIcon(PriceType priceType)
    {
        // Act
        var result = _sut.PickIconFor(priceType);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldNotBe(MudBlazorIconMapping.DefaultIconString);
    }
}

