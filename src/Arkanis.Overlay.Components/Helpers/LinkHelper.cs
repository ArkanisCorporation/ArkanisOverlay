namespace Arkanis.Overlay.Components.Helpers;

using Domain.Models.Inventory;

public static class LinkHelper
{
    private static readonly string AssemblyName = typeof(LinkHelper).Assembly.GetName().Name ?? string.Empty;

    public static string GetPathToAsset(string relativeAssetPath)
        => $"_content/{AssemblyName}/assets/{relativeAssetPath}";

    public static string Hangar(InventoryEntryId? entryId = null)
        => entryId is not null
            ? $"/hangar/{entryId.Identity.ToString()}"
            : "/hangar";
}
