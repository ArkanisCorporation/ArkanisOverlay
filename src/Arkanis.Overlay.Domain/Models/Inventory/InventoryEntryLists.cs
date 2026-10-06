namespace Arkanis.Overlay.Domain.Models.Inventory;

using System.ComponentModel.DataAnnotations;
using Abstractions;
using Abstractions.Game;

public record InventoryEntryListId(Guid Identity) : TypedDomainId<Guid>(Identity)
{
    public static InventoryEntryListId CreateNew()
        => new(Guid.NewGuid());
}

public class InventoryEntryListBrief : IIdentifiable
{
    public InventoryEntryListId Id { get; init; } = InventoryEntryListId.CreateNew();

    [MaxLength(60)]
    public required string Name { get; set; }

    [MaxLength(10000)]
    public string Notes { get; set; } = string.Empty;

    IDomainId IIdentifiable.Id
        => Id;
}

public class InventoryEntryList : InventoryEntryListBrief
{
    public List<InventoryEntryBase> Entries { get; init; } = [];
}
