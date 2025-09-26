namespace Arkanis.Overlay.Infrastructure.Data.Mappers;

using Domain.Models.Trade;
using Entities;
using Riok.Mapperly.Abstractions;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal partial class TradeRunEntityMapper(EntityReferenceMapper entityReferenceMapper, UexApiDtoMapper uexMapper)
    : EntityReferenceMapper.ICapabilities, UexApiDtoMapper.ICapabilities
{
    EntityReferenceMapper IMapperWith<EntityReferenceMapper>.Reference
        => entityReferenceMapper;

    UexApiDtoMapper IMapperWith<UexApiDtoMapper>.Reference
        => uexMapper;

    #region (To Domain) Trade Run Mapping

    [MapProperty(nameof(TradeRunEntity.VehicleId), nameof(TradeRun.Vehicle))]
    public partial TradeRun Map(TradeRunEntity tradeRun);

    #endregion

    #region (To Database) Trade Run Mapping

    public partial TradeRunEntity Map(TradeRun tradeRun);

    #endregion

    public interface ICapabilities : IMapperWith<TradeRunEntityMapper>
    {
        [UserMapping(Default = true)]
        public TradeRun Map(TradeRunEntity tradeRun)
            => Reference.Map(tradeRun);

        [UserMapping(Default = true)]
        public TradeRunEntity Map(TradeRun tradeRun)
            => Reference.Map(tradeRun);

        [UserMapping(Default = true)]
        public TradeRun.AcquisitionStage Map(TradeRunEntity.AcquisitionStage stage)
            => Reference.Map(stage);

        [UserMapping(Default = true)]
        public TradeRun.SaleStage Map(TradeRunEntity.SaleStage stage)
            => Reference.Map(stage);
    }

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

    [MapProperty(
        nameof(TradeRunEntity.TerminalPurchaseStage.TerminalId),
        nameof(TradeRun.TerminalPurchaseStage.Terminal),
        Use = nameof(UexApiDtoMapper.ICapabilities.ResolveTerminal)
    )]
    public partial TradeRun.TerminalPurchaseStage Map(TradeRunEntity.TerminalPurchaseStage stage);

    [MapProperty(
        nameof(TradeRunEntity.TerminalSaleStage.TerminalId),
        nameof(TradeRun.TerminalSaleStage.Terminal),
        Use = nameof(UexApiDtoMapper.ICapabilities.ResolveTerminal)
    )]
    public partial TradeRun.TerminalSaleStage Map(TradeRunEntity.TerminalSaleStage stage);

    #endregion
}
