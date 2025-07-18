namespace Arkanis.Overlay.Components.ViewModels;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Analytics;
using Domain.Models.Game;
using Domain.Models.Inventory;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared;
using Shared.Dialogs;

public class InventoryViewModel(
    IDialogService dialogService,
    IInventoryManager inventoryManager,
    IAnalyticsEventReporter eventReporter
)
{
    public event EventHandler? OnChange;

    private async Task UpdateAsync()
    {
        OnChange?.Invoke(this, EventArgs.Empty);
        await Task.CompletedTask;
    }

    #region Entries

    public async Task CreateNewEntryAsync(IGameEntity? entity = null, IGameLocation? location = null, HangarInventoryEntry? hangarEntry = null)
    {
        if (await InventoryEntryCreateDialog.ShowAsync(dialogService, entity, location, hangarEntry) is not null)
        {
            await UpdateAsync();
        }
    }

    public async Task UpdateAsync(InventoryEntryBase entry, IGameLocation? targetLocation)
    {
        if (await InventoryEntryUpdateDialog.ShowEditAsync(dialogService, entry, targetLocation) is not null)
        {
            await UpdateAsync();
        }
    }

    public async Task TransferAsync(InventoryEntryBase entry, IGameLocation? targetLocation)
    {
        if (await InventoryEntryUpdateDialog.ShowTransferAsync(dialogService, entry, targetLocation) is not null)
        {
            await UpdateAsync();
        }
    }

    public async Task DeleteForeverAsync(InventoryEntryBase entry)
    {
        var atLocationPart = GetDisplayLocation(entry) is { Length: > 0 } content
            ? $" {content}"
            : string.Empty;
        var options = new MessageBoxOptions
        {
            Title = "Are you sure?",
            MarkupMessage = new MarkupString(
                $"Do you really want to permanently remove {entry.Quantity} <b>{GetDisplayName(entry, false)}</b>{atLocationPart} from your inventory?"
            ),
            YesText = "Remove",
            CancelText = "Cancel",
        };
        if (await dialogService.ShowMessageBox(options) == true)
        {
            await inventoryManager.DeleteEntryAsync(entry.Id);
            await eventReporter.TrackEventAsync(InventoryEvents.RemoveItem());
            await UpdateAsync();
        }
    }

    #endregion

    #region Entries (Bulk Operations)

    public async Task TransferAsync(ICollection<InventoryEntryBase> selectedEntries, InventoryEntryFilters.Context? currentContext = null)
    {
        // TODO: Allow selection of vehicle as well
        if ((currentContext?.Location ?? await SelectGameLocationDialog.ShowAsync(dialogService)) is not { } location)
        {
            return;
        }

        var configuration = new BulkOperationDialog<InventoryEntryBase>.Configuration
        {
            PerformOperation = async entry =>
            {
                await inventoryManager.AddOrUpdateEntryAsync(entry.TransferTo(location));
                await eventReporter.TrackEventAsync(InventoryEvents.AssignLocation());
            },
            Description = InventoryViewModelRendering.TransferEntryDescription(selectedEntries, location),
        };
        var options = new BulkOperationDialog<InventoryEntryBase>.Options
        {
            SubmitColor = Color.Success,
            SubmitLabel = "Transfer",
        };

        await BulkOperationDialog<InventoryEntryBase>.ShowAsync(dialogService, selectedEntries, configuration, options);
        await UpdateAsync();
    }

    public async Task TransferModuleToAsync(HangarInventoryEntry entry, GameProductCategory? category)
    {
        if (await InventoryEntryUpdateDialog.ShowTransferModuleToAsync(dialogService, entry, category) is not null)
        {
            await UpdateAsync();
        }
    }

    public async Task TransferCargoToAsync(HangarInventoryEntry entry)
    {
        if (await InventoryEntryUpdateDialog.ShowTransferCargoToAsync(dialogService, entry) is not null)
        {
            await UpdateAsync();
        }
    }

    public async Task AssignToListAsync(ICollection<InventoryEntryBase> selectedEntries, InventoryEntryFilters.Context? currentContext = null)
    {
        if ((currentContext?.InventoryList ?? await SelectInventoryListDialog.ShowAsync(dialogService)) is not { } list)
        {
            return;
        }

        var configuration = new BulkOperationDialog<InventoryEntryBase>.Configuration
        {
            PerformOperation = async entry =>
            {
                entry.List = list;
                await inventoryManager.AddOrUpdateEntryAsync(entry);
                await eventReporter.TrackEventAsync(InventoryEvents.AssignList());
            },
            Description = InventoryViewModelRendering.AssignToListDescription(selectedEntries, list),
        };
        var options = new BulkOperationDialog<InventoryEntryBase>.Options
        {
            SubmitColor = Color.Success,
            SubmitLabel = "Assign",
        };

        await BulkOperationDialog<InventoryEntryBase>.ShowAsync(dialogService, selectedEntries, configuration, options);
        await UpdateAsync();
    }

    public async Task DeleteForeverAsync(ICollection<InventoryEntryBase> selectedEntries)
    {
        var configuration = new BulkOperationDialog<InventoryEntryBase>.Configuration
        {
            PerformOperation = async entry =>
            {
                await inventoryManager.DeleteEntryAsync(entry.Id);
                await eventReporter.TrackEventAsync(InventoryEvents.RemoveItem());
            },
            Description = InventoryViewModelRendering.DeleteForeverDescription(selectedEntries),
        };
        var options = new BulkOperationDialog<InventoryEntryBase>.Options
        {
            SubmitColor = Color.Error,
            SubmitLabel = "Permanently Remove",
        };

        await BulkOperationDialog<InventoryEntryBase>.ShowAsync(dialogService, selectedEntries, configuration, options);
        await UpdateAsync();
    }

    #endregion

    #region Lists

    public async Task CreateNewListAsync()
    {
        if (await InventoryListDialog.ShowAsync(dialogService) is not null)
        {
            await UpdateAsync();
        }
    }

    public async Task EditListAsync(InventoryEntryList list)
    {
        if (await InventoryListDialog.ShowAsync(dialogService, list) is not null)
        {
            await UpdateAsync();
        }
    }

    public async Task DeleteForeverAsync(InventoryEntryList list)
    {
        var options = new MessageBoxOptions
        {
            Title = "Are you sure?",
            MarkupMessage = new MarkupString(
                $"Do you really want to permanently remove inventory list <b>{list.Name}</b>? Assigned items themselves will <u>not</u> be removed."
            ),
            YesText = "Remove",
            CancelText = "Cancel",
        };
        if (await dialogService.ShowMessageBox(options) == true)
        {
            await inventoryManager.DeleteListAsync(list.Id);
            await eventReporter.TrackEventAsync(InventoryEvents.RemoveList());
            await UpdateAsync();
        }
    }

    #endregion

    #region Utilities

    public string? GetDisplayName<T>(T? value, bool includeLocation = true) where T : InventoryEntryBase
    {
        var entityName = value switch
        {
            HangarInventoryEntry vehicle => GetDisplayName(vehicle),
            VehicleModuleEntry module => GetDisplayName(module.ItemReference),
            VehicleInventoryEntry cargo => GetDisplayName(cargo.Quantity.Reference.Entity),
            _ => GetDisplayName(value?.Entity),
        };

        return value switch
        {
            _ when includeLocation => $"{entityName} {GetDisplayLocation(value)}".Trim(),
            _ => entityName,
        };
    }

    public string? GetDisplayLocation<T>(T? value) where T : InventoryEntryBase
        => value switch
        {
            IGameLocatedAt locatedAt => $"at {GetDisplayName(locatedAt.Location)}",
            IVehicleInventory entry => $"from {GetDisplayName(entry.HangarEntry)} {GetDisplayLocation(entry.HangarEntry)}",
            _ => null,
        };

    public string? GetDisplayName(HangarInventoryEntry vehicle)
        => vehicle switch
           {
               { NameTag: { Length: > 0 } nameTag } => $"\"{nameTag}\" ({GetDisplayName(vehicle.VehicleReference)})",
               _ => GetDisplayName(vehicle.VehicleReference),
           };

    public string? GetDisplayName(IGameEntity? gameEntity)
        => gameEntity?.Name.MainContent.FullName;

    public static bool AcceptLocationForInventoryEntry(IGameEntity entity)
        => entity is IGameLocation location && AcceptLocationForInventoryEntry(location);

    public static bool AcceptLocationForInventoryEntry(IGameLocation location)
        => location is GameSpaceStation or GameCity or GameOutpost;

    #endregion
}
