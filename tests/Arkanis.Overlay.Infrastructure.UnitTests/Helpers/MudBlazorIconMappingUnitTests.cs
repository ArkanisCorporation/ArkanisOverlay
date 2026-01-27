namespace Arkanis.Overlay.Infrastructure.UnitTests.Helpers;

using Arkanis.Overlay.Components.Helpers;
using Arkanis.Overlay.Domain.Constants;
using Arkanis.Overlay.Domain.Models.IconSelection;
using Shouldly;

public sealed class MudBlazorIconMappingUnitTests
{
    [Fact]
    public void GetIconString_NavigationSearch_ReturnsMudBlazorSearchIcon()
    {
        // Act
        var result = MudBlazorIconMapping.GetIconString(OverlayIcons.Navigation.Search);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Search);
    }

    [Fact]
    public void GetIconString_NavigationChevronLeft_ReturnsMudBlazorChevronLeftIcon()
    {
        // Act
        var result = MudBlazorIconMapping.GetIconString(OverlayIcons.Navigation.ChevronLeft);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.ChevronLeft);
    }

    [Fact]
    public void GetIconString_GameEntitySpaceShip_ReturnsMudBlazorRocketIcon()
    {
        // Act
        var result = MudBlazorIconMapping.GetIconString(OverlayIcons.GameEntity.SpaceShip);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Rocket);
    }

    [Fact]
    public void GetIconString_GameEntityCommodity_ReturnsMudBlazorDiamondIcon()
    {
        // Act
        var result = MudBlazorIconMapping.GetIconString(OverlayIcons.GameEntity.Commodity);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Diamond);
    }

    [Fact]
    public void GetIconString_TradeAddShoppingCart_ReturnsMudBlazorAddShoppingCartIcon()
    {
        // Act
        var result = MudBlazorIconMapping.GetIconString(OverlayIcons.Trade.AddShoppingCart);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.AddShoppingCart);
    }

    [Fact]
    public void GetIconString_TradeRemoveShoppingCart_ReturnsMudBlazorRemoveShoppingCartIcon()
    {
        // Act
        var result = MudBlazorIconMapping.GetIconString(OverlayIcons.Trade.RemoveShoppingCart);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.RemoveShoppingCart);
    }

    [Fact]
    public void GetIconString_SystemSettings_ReturnsMudBlazorSettingsIcon()
    {
        // Act
        var result = MudBlazorIconMapping.GetIconString(OverlayIcons.System.Settings);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Settings);
    }

    [Fact]
    public void GetIconString_UnknownIcon_ReturnsDefaultSquareIcon()
    {
        // Arrange
        var unknownIcon = "UnknownCategory.UnknownIcon";

        // Act
        var result = MudBlazorIconMapping.GetIconString(unknownIcon);

        // Assert
        result.ShouldBe(MudBlazorIconMapping.DefaultIconString);
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Square);
    }

    [Fact]
    public void GetIconString_NullOrEmpty_ReturnsDefaultIcon()
    {
        // Act
        var resultNull = MudBlazorIconMapping.GetIconString((string)null!);
        var resultEmpty = MudBlazorIconMapping.GetIconString(string.Empty);
        var resultWhitespace = MudBlazorIconMapping.GetIconString("   ");

        // Assert
        resultNull.ShouldBe(MudBlazorIconMapping.DefaultIconString);
        resultEmpty.ShouldBe(MudBlazorIconMapping.DefaultIconString);
        resultWhitespace.ShouldBe(MudBlazorIconMapping.DefaultIconString);
    }

    [Fact]
    public void GetIconString_WithCustomIcons_UsesCustomMapping()
    {
        // Arrange
        var customIcons = new Dictionary<string, string>
        {
            { OverlayIcons.Navigation.Search, OverlayIcons.System.Settings }
        };

        // Act
        var result = MudBlazorIconMapping.GetIconString(OverlayIcons.Navigation.Search, customIcons);

        // Assert - should return Settings icon instead of Search
        result.ShouldBe(MudBlazor.Icons.Material.Filled.Settings);
    }

    [Fact]
    public void GetIconString_IconSelectionObject_MaterialIcons_ReturnsCorrectIcon()
    {
        // Arrange
        var iconSelection = new IconSelectionObject
        {
            Type = IconType.MaterialIcons,
            IconName = OverlayIcons.Trade.AddShoppingCart,
            Category = IconCategory.Trade,
            Description = "Add shopping cart icon",
            IsActive = true,
            Priority = 0,
            FallbackIcons = []
        };

        // Act
        var result = MudBlazorIconMapping.GetIconString(iconSelection);

        // Assert
        result.ShouldBe(MudBlazor.Icons.Material.Filled.AddShoppingCart);
    }

    [Fact]
    public void GetIconString_IconSelectionObject_MaterialSymbols_ReturnsCorrectIcon()
    {
        // Arrange
        var iconSelection = new IconSelectionObject
        {
            Type = IconType.MaterialSymbols,
            IconName = OverlayIcons.Navigation.Search,
            Category = IconCategory.Navigation,
            Description = "Search icon",
            IsActive = true,
            Priority = 0,
            FallbackIcons = []
        };

        // Act
        var result = MudBlazorIconMapping.GetIconString(iconSelection);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        // Should return a Material Symbols icon
    }

    [Fact]
    public void GetIconString_IconSelectionObject_CustomType_ReturnsDefaultIcon()
    {
        // Arrange
        var iconSelection = new IconSelectionObject
        {
            Type = IconType.Custom,
            IconName = "CustomIcon",
            Category = IconCategory.Action,
            Description = "Custom icon",
            IsActive = true,
            Priority = 0,
            FallbackIcons = []
        };

        // Act
        var result = MudBlazorIconMapping.GetIconString(iconSelection);

        // Assert
        result.ShouldBe(MudBlazorIconMapping.DefaultIconString);
    }

    [Fact]
    public void GetIconString_AllNavigationIcons_ReturnValidIcons()
    {
        // Arrange
        var navigationIcons = new[]
        {
            OverlayIcons.Navigation.Search,
            OverlayIcons.Navigation.ChevronLeft,
            OverlayIcons.Navigation.OpenInBrowser,
            OverlayIcons.Navigation.Web
        };

        // Act & Assert
        foreach (var icon in navigationIcons)
        {
            var result = MudBlazorIconMapping.GetIconString(icon);
            result.ShouldNotBeNullOrEmpty();
            result.ShouldNotBe(MudBlazorIconMapping.DefaultIconString);
        }
    }

    [Fact]
    public void GetIconString_AllTradeIcons_ReturnValidIcons()
    {
        // Arrange
        var tradeIcons = new[]
        {
            OverlayIcons.Trade.AddShoppingCart,
            OverlayIcons.Trade.RemoveShoppingCart,
            OverlayIcons.Trade.CarRental,
            OverlayIcons.Trade.Storefront,
            OverlayIcons.Trade.Warehouse
        };

        // Act & Assert
        foreach (var icon in tradeIcons)
        {
            var result = MudBlazorIconMapping.GetIconString(icon);
            result.ShouldNotBeNullOrEmpty();
            result.ShouldNotBe(MudBlazorIconMapping.DefaultIconString);
        }
    }

    [Fact]
    public void GetIconString_AllGameEntityIcons_ReturnValidIcons()
    {
        // Arrange
        var gameEntityIcons = new[]
        {
            OverlayIcons.GameEntity.Commodity,
            OverlayIcons.GameEntity.SpaceShip,
            OverlayIcons.GameEntity.GroundVehicle,
            OverlayIcons.GameEntity.Item,
            OverlayIcons.GameEntity.ProductCategory,
            OverlayIcons.GameEntity.Company,
            OverlayIcons.GameEntity.Location,
            OverlayIcons.GameEntity.Flight,
            OverlayIcons.GameEntity.Store,
            OverlayIcons.GameEntity.GarageDoor
        };

        // Act & Assert
        foreach (var icon in gameEntityIcons)
        {
            var result = MudBlazorIconMapping.GetIconString(icon);
            result.ShouldNotBeNullOrEmpty();
            result.ShouldNotBe(MudBlazorIconMapping.DefaultIconString);
        }
    }
}

