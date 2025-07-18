namespace Arkanis.Overlay.Components.ViewModels;

using Domain.Models.Inventory;
using MudBlazor;
using Shared.Dialogs;

public sealed class HangarViewModel : IDisposable
{
    private readonly IDialogService _dialogService;
    private readonly InventoryViewModel _inventoryViewModel;

    public HangarViewModel(IDialogService dialogService, InventoryViewModel inventoryViewModel)
    {
        _dialogService = dialogService;
        _inventoryViewModel = inventoryViewModel;
        _inventoryViewModel.OnChange += InventoryOnChange;
    }

    public void Dispose()
        => _inventoryViewModel.OnChange -= InventoryOnChange;

    public event EventHandler? OnChange;

    private void InventoryOnChange(object? _, EventArgs e)
        => OnChange?.Invoke(_, e);

    private async Task UpdateAsync()
    {
        OnChange?.Invoke(this, EventArgs.Empty);
        await Task.CompletedTask;
    }

    public async Task CreateNewAsync()
    {
        if (await HangarEntryCreateDialog.ShowAsync(_dialogService) is not null)
        {
            await UpdateAsync();
        }
    }

    public async Task EditAsync(HangarInventoryEntry entry)
    {
        if (await HangarEntryUpdateDialog.ShowAsync(_dialogService, entry) is not null)
        {
            await UpdateAsync();
        }
    }

    public Task DeleteForeverAsync(HangarInventoryEntry entry)
        => _inventoryViewModel.DeleteForeverAsync(entry);
}
