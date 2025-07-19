namespace Arkanis.Overlay.Components.Helpers;

using Domain.Models.Trade;
using Microsoft.AspNetCore.Http;
using Views;
using Views.Trade;

public static class LinkHelper
{
    private static readonly string AssemblyName = typeof(LinkHelper).Assembly.GetName().Name ?? string.Empty;

    public static string GetPathToAsset(string relativeAssetPath)
        => $"_content/{AssemblyName}/assets/{relativeAssetPath}";

    public static string ToTradeLedger(TradeRunId? tradeRunId)
    {
        var queryString = tradeRunId is not null
            ? QueryString.Create(nameof(TradeLedgerView.TradeRunId), tradeRunId.Identity.ToString()).ToString()
            : string.Empty;

        return $"/trade/{TradeView.Tab.Ledger:G}{queryString}";
    }
}
