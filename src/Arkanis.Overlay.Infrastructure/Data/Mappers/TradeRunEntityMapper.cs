namespace Arkanis.Overlay.Infrastructure.Data.Mappers;

using Domain.Models.Game;
using Domain.Models.Trade;
using Entities;
using Riok.Mapperly.Abstractions;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal partial class TradeRunEntityMapper(UexApiDtoMapper uexMapper)
{
    private GameItem ResolveItem(UexId<GameItem> itemId)
        => uexMapper.ResolveCachedGameEntity(itemId);

    private GameCommodity ResolveCommodity(UexId<GameCommodity> commodityId)
        => uexMapper.ResolveCachedGameEntity(commodityId);

    private GameVehicle? ResolveVehicle(UexId<GameVehicle>? vehicleId)
        => vehicleId is not null
            ? uexMapper.ResolveCachedGameEntity(vehicleId)
            : null;

    private GameTerminal ResolveTerminal(UexId<GameTerminal> terminalId)
        => uexMapper.ResolveCachedGameEntity(terminalId);

    #region (To Database) Quantity Of Entity

    public partial QuantityOfEntity Map(QuantityOf quantityOf);

    #endregion

    #region (To Domain) Quantity Of Entity

    public partial QuantityOf Map(QuantityOfEntity quantityOf);

    #endregion

    #region (To Domain) Trade Run Mapping

    [MapProperty(nameof(TradeRunEntity.VehicleId), nameof(TradeRun.Vehicle), Use = nameof(ResolveVehicle))]
    public partial TradeRun Map(TradeRunEntity tradeRun);

    #endregion

    #region (To Database) Trade Run Mapping

    public partial TradeRunEntity Map(TradeRun tradeRun);

    #endregion

    #region (To Database) Ownable Entity Reference

    [UserMapping(Default = true)]
    public OwnableEntityReferenceEntity Map(OwnableEntityReference reference)
        => OwnableEntityReferenceEntity.Create(reference);

    #endregion

    #region (To Domain) Ownable Entity Reference

    [UserMapping(Default = true)]
    public OwnableEntityReference Map(OwnableEntityReferenceEntity referenceEntity)
        => referenceEntity.ToReference(uexMapper);

    #endregion

    #region (To Database) Trade Run Stage Mapping

    [UserMapping(Default = true)]
    public TradeRunEntity.AcquisitionStage Map(TradeRun.AcquisitionStage stage)
        => stage switch
        {
            TradeRun.TerminalPurchaseStage terminalPurchaseStage => Map(terminalPurchaseStage),
            _ => throw new NotSupportedException($"Database entity mapping not supported for source domain object: {stage}"),
        };

    [UserMapping(Default = true)]
    public TradeRunEntity.SaleStage Map(TradeRun.SaleStage stage)
        => stage switch
        {
            TradeRun.TerminalSaleStage terminalSaleStage => Map(terminalSaleStage),
            _ => throw new NotSupportedException($"Database entity mapping not supported for source domain object: {stage}"),
        };

    public partial TradeRunEntity.TerminalPurchaseStage Map(TradeRun.TerminalPurchaseStage stage);

    public partial TradeRunEntity.TerminalSaleStage Map(TradeRun.TerminalSaleStage stage);

    #endregion

    #region (To Domain) Trade Run Stage Mapping

    [UserMapping(Default = true)]
    public TradeRun.AcquisitionStage Map(TradeRunEntity.AcquisitionStage stage)
        => stage switch
        {
            TradeRunEntity.TerminalPurchaseStage terminalPurchaseStage => Map(terminalPurchaseStage),
            _ => throw new NotSupportedException($"Database entity mapping not supported for source domain object: {stage}"),
        };

    [UserMapping(Default = true)]
    public TradeRun.SaleStage Map(TradeRunEntity.SaleStage stage)
        => stage switch
        {
            TradeRunEntity.TerminalSaleStage terminalSaleStage => Map(terminalSaleStage),
            _ => throw new NotSupportedException($"Database entity mapping not supported for source domain object: {stage}"),
        };

    [MapProperty(nameof(TradeRunEntity.TerminalPurchaseStage.TerminalId), nameof(TradeRun.TerminalPurchaseStage.Terminal), Use = nameof(ResolveTerminal))]
    public partial TradeRun.TerminalPurchaseStage Map(TradeRunEntity.TerminalPurchaseStage stage);

    [MapProperty(nameof(TradeRunEntity.TerminalSaleStage.TerminalId), nameof(TradeRun.TerminalSaleStage.Terminal), Use = nameof(ResolveTerminal))]
    public partial TradeRun.TerminalSaleStage Map(TradeRunEntity.TerminalSaleStage stage);

    #endregion
}
