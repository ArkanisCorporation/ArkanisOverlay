namespace Arkanis.Overlay.Infrastructure.UnitTests.Services.IconManagement;

using Arkanis.Overlay.Domain.Constants;
using Arkanis.Overlay.Domain.Enums;
using Arkanis.Overlay.Domain.Models.IconSelection;
using Infrastructure.Services.IconManagement;
using Shouldly;

public sealed class IconServiceUnitTests
{
    private readonly IconService _sut;

    public IconServiceUnitTests()
    {
        _sut = new IconService();
    }

    [Fact]
    public void GetIconSelectionFor_PriceType_Buy_ReturnsAddShoppingCartIcon()
    {
        // Act
        var result = _sut.GetIconSelectionFor(PriceType.Buy);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe(OverlayIcons.Trade.AddShoppingCart);
        result.Category.ShouldBe(IconCategory.Trade);
        result.Type.ShouldBe(IconType.MaterialIcons);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelectionFor_PriceType_Sell_ReturnsRemoveShoppingCartIcon()
    {
        // Act
        var result = _sut.GetIconSelectionFor(PriceType.Sell);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe(OverlayIcons.Trade.RemoveShoppingCart);
        result.Category.ShouldBe(IconCategory.Trade);
        result.Type.ShouldBe(IconType.MaterialIcons);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelectionFor_PriceType_Rent_ReturnsCarRentalIcon()
    {
        // Act
        var result = _sut.GetIconSelectionFor(PriceType.Rent);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe(OverlayIcons.Trade.CarRental);
        result.Category.ShouldBe(IconCategory.Trade);
        result.Type.ShouldBe(IconType.MaterialIcons);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelectionFor_GameEntityCategory_SpaceShip_ReturnsSpaceShipIcon()
    {
        // Act
        var result = _sut.GetIconSelectionFor(GameEntityCategory.SpaceShip);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe(OverlayIcons.GameEntity.SpaceShip);
        result.Category.ShouldBe(IconCategory.GameEntity);
        result.Type.ShouldBe(IconType.MaterialIcons);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelectionFor_GameEntityCategory_Commodity_ReturnsCommodityIcon()
    {
        // Act
        var result = _sut.GetIconSelectionFor(GameEntityCategory.Commodity);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe(OverlayIcons.GameEntity.Commodity);
        result.Category.ShouldBe(IconCategory.GameEntity);
        result.Type.ShouldBe(IconType.MaterialIcons);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelectionFor_GameEntityCategory_GroundVehicle_ReturnsGroundVehicleIcon()
    {
        // Act
        var result = _sut.GetIconSelectionFor(GameEntityCategory.GroundVehicle);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe(OverlayIcons.GameEntity.GroundVehicle);
        result.Category.ShouldBe(IconCategory.GameEntity);
        result.Type.ShouldBe(IconType.MaterialIcons);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelectionFor_GameEntityCategory_Item_ReturnsItemIcon()
    {
        // Act
        var result = _sut.GetIconSelectionFor(GameEntityCategory.Item);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe(OverlayIcons.GameEntity.Item);
        result.Category.ShouldBe(IconCategory.GameEntity);
        result.Type.ShouldBe(IconType.MaterialIcons);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelectionFor_GameEntityCategory_ProductCategory_ReturnsProductCategoryIcon()
    {
        // Act
        var result = _sut.GetIconSelectionFor(GameEntityCategory.ProductCategory);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe(OverlayIcons.GameEntity.ProductCategory);
        result.Category.ShouldBe(IconCategory.GameEntity);
        result.Type.ShouldBe(IconType.MaterialIcons);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelectionFor_GameEntityCategory_Company_ReturnsCompanyIcon()
    {
        // Act
        var result = _sut.GetIconSelectionFor(GameEntityCategory.Company);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe(OverlayIcons.GameEntity.Company);
        result.Category.ShouldBe(IconCategory.GameEntity);
        result.Type.ShouldBe(IconType.MaterialIcons);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelectionFor_GameEntityCategory_Location_ReturnsLocationIcon()
    {
        // Act
        var result = _sut.GetIconSelectionFor(GameEntityCategory.Location);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe(OverlayIcons.GameEntity.Location);
        result.Category.ShouldBe(IconCategory.GameEntity);
        result.Type.ShouldBe(IconType.MaterialIcons);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelection_ValidIdentifier_Navigation_ReturnsCorrectIconSelection()
    {
        // Arrange
        var identifier = OverlayIcons.Navigation.Search;

        // Act
        var result = _sut.GetIconSelection(identifier);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe("Search");
        result.Category.ShouldBe(IconCategory.Navigation);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelection_ValidIdentifier_Trade_ReturnsCorrectIconSelection()
    {
        // Arrange
        var identifier = OverlayIcons.Trade.AddShoppingCart;

        // Act
        var result = _sut.GetIconSelection(identifier);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe("AddShoppingCart");
        result.Category.ShouldBe(IconCategory.Trade);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelection_ValidIdentifier_System_ReturnsCorrectIconSelection()
    {
        // Arrange
        var identifier = OverlayIcons.System.Settings;

        // Act
        var result = _sut.GetIconSelection(identifier);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe("Settings");
        result.Category.ShouldBe(IconCategory.System);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelection_UnknownIdentifier_ReturnsDefaultIcon()
    {
        // Arrange
        var identifier = "Unknown.UnknownIcon";

        // Act
        var result = _sut.GetIconSelection(identifier);

        // Assert
        result.ShouldNotBeNull();
        result.Category.ShouldBe(IconCategory.Status);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void GetIconSelectionFor_UnsupportedType_ReturnsDefaultIcon()
    {
        // Arrange
        var unsupportedValue = "Some string value";

        // Act
        var result = _sut.GetIconSelectionFor(unsupportedValue);

        // Assert
        result.ShouldNotBeNull();
        result.IconName.ShouldBe(OverlayIcons.Status.Square);
        result.Category.ShouldBe(IconCategory.Status);
        result.Type.ShouldBe(IconType.MaterialIcons);
        result.IsActive.ShouldBeTrue();
    }
}

