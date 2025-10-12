namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Microsoft.Extensions.Primitives;
using Models.Inventory;

public interface IInventoryManager
{
    public IChangeToken ChangeToken { get; }

    public Task<int> GetUnassignedCountAsync(CancellationToken cancellationToken = default);

    public Task<ICollection<InventoryEntryBase>> GetEntriesForAsync(
        IDomainId domainId,
        InventoryEntryBase.EntryType entryType,
        CancellationToken cancellationToken = default
    );

    public Task<ICollection<InventoryEntryBase>> GetEntriesForAsync(IDomainId domainId, CancellationToken cancellationToken = default);

    public Task<ICollection<InventoryEntryBase>> GetAllEntriesAsync(CancellationToken cancellationToken = default);

    public Task AddOrUpdateEntryAsync(InventoryEntryBase entry, CancellationToken cancellationToken = default);

    public Task DeleteEntryAsync(InventoryEntryId entryId, CancellationToken cancellationToken = default);

    public Task<InventoryEntryList?> GetListAsync(InventoryEntryListId listId, CancellationToken cancellationToken = default);

    public Task<ICollection<InventoryEntryList>> GetAllListsAsync(CancellationToken cancellationToken = default);

    public Task AddOrUpdateListAsync(InventoryEntryList list, CancellationToken cancellationToken = default);

    public Task DeleteListAsync(InventoryEntryListId listId, CancellationToken cancellationToken = default);
}
