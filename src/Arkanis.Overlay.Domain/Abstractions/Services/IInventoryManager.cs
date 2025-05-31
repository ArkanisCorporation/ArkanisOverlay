namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models.Inventory;

public interface IInventoryManager
{
    Task<ICollection<InventoryEntryBase>> GetAllEntriesAsync(CancellationToken cancellationToken = default);

    Task UpdateEntryAsync(InventoryEntryBase entry, CancellationToken cancellationToken = default);

    Task DeleteEntryAsync(InventoryEntryId entryId, CancellationToken cancellationToken = default);

    Task<InventoryEntryList?> GetListAsync(InventoryEntryListId listId, CancellationToken cancellationToken = default);

    Task<ICollection<InventoryEntryList>> GetAllListsAsync(CancellationToken cancellationToken = default);

    Task UpdateListAsync(InventoryEntryList list, CancellationToken cancellationToken = default);

    Task DeleteListAsync(InventoryEntryListId listId, CancellationToken cancellationToken = default);
}
