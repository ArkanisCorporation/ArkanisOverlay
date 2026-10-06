namespace Arkanis.Overlay.Domain.Models.Trade;

using Enums;
using Inventory;

public class TerminalData
{
    public bool UserConfirmed { get; set; }
    public GameContainerSize MaxContainerSize { get; set; }
    public TerminalInventoryStatus StockStatus { get; set; }
    public Quantity Stock { get; set; } = Quantity.FromScu(0);
}
