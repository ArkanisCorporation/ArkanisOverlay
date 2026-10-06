namespace Arkanis.Overlay.Domain.Models.Analytics;

public abstract record AnalyticsEvent(string EventName, string ModuleName);

public record BuiltInAnalyticsEvent(string EventName) : AnalyticsEvent(EventName, "user_behaviour");

#region Events with custom values (must also be explicitly registered in global analytics manager to work properly)

public record FeatureUsageStateChangedEvent(string EventName, bool IsEnabled) : AnalyticsEvent(EventName, "feature_usage_state_changed")
{
    public static FeatureUsageStateChangedEvent TerminateWithGame(bool enabled = true)
        => new("overlay__app__terminate", enabled);

    public static FeatureUsageStateChangedEvent AutoStart(bool enabled = true)
        => new("overlay__app__autostart", enabled);

    public static FeatureUsageStateChangedEvent Analytics(bool enabled = true)
        => new("overlay__app__analytics", enabled);

    public static FeatureUsageStateChangedEvent BlurBackground(bool enabled = true)
        => new("overlay__blur", enabled);
}

public sealed record SearchEvent(string Query) : BuiltInAnalyticsEvent("search");

public sealed record DialogOpenedEvent(string DialogId) : BuiltInAnalyticsEvent("dialog_opened");

#endregion

#region Inventory events

public static class InventoryEvents
{
    public static InventoryAddItem AddItem()
        => new();

    public static InventoryRemoveItem RemoveItem()
        => new();

    public static InventoryTransferItem AssignLocation()
        => new();

    public static InventoryAddList AddList()
        => new();

    public static InventoryRemoveList RemoveList()
        => new();

    public static InventoryAssignList AssignList()
        => new();
}

public sealed record InventoryAddList() : BuiltInAnalyticsEvent("inventory_list_add");

public sealed record InventoryRemoveList() : BuiltInAnalyticsEvent("inventory_list_remove");

public sealed record InventoryAssignList() : BuiltInAnalyticsEvent("inventory_list_assign");

public sealed record InventoryAddItem() : BuiltInAnalyticsEvent("inventory_item_add");

public sealed record InventoryRemoveItem() : BuiltInAnalyticsEvent("inventory_item_remove");

public sealed record InventoryTransferItem() : BuiltInAnalyticsEvent("inventory_item_transfer");

#endregion

#region Trade Run events

public static class TradeRunEvents
{
    public static TradeRunCreate CreateTradeRun()
        => new();

    public static TradeRunStageCreate CreateTradeRunStage()
        => new();

    public static TradeRunStageFinalize FinalizeTradeRunStage()
        => new();

    public static TradeRunFinalize FinalizeTradeRun()
        => new();

    public static TradeRunAbandon AbandonTradeRun()
        => new();
}

public sealed record TradeRunCreate() : BuiltInAnalyticsEvent("trade_run-create");

public sealed record TradeRunStageCreate() : BuiltInAnalyticsEvent("trade_run-stage_create");

public sealed record TradeRunStageFinalize() : BuiltInAnalyticsEvent("trade_run-stage_finalize");

public sealed record TradeRunFinalize() : BuiltInAnalyticsEvent("trade_run-finalize");

public sealed record TradeRunAbandon() : BuiltInAnalyticsEvent("trade_run-abandon");

#endregion

#region Regular events

public sealed record SearchCalculationEvent() : BuiltInAnalyticsEvent("search_calculate");

public sealed record OverlayShownEvent() : BuiltInAnalyticsEvent("overlay_shown");

public sealed record OverlayHiddenEvent() : BuiltInAnalyticsEvent("overlay_hidden");

#endregion
