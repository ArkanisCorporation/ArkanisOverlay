namespace Arkanis.Overlay.Infrastructure.Data.Mappers;

using Domain.Models.Trade;
using Entities;
using Riok.Mapperly.Abstractions;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal partial class EntityReferenceMapper(UexApiDtoMapper uexMapper)
{
    protected readonly UexApiDtoMapper UEXMapper = uexMapper;

    #region (To Database) Quantity Of Entity

    public partial QuantityOfEntity Map(QuantityOf quantityOf);

    #endregion

    #region (To Domain) Quantity Of Entity

    public partial QuantityOf Map(QuantityOfEntity quantityOf);

    #endregion

    #region (To Database) Ownable Entity Reference

    [UserMapping(Default = true)]
    public OwnableEntityReferenceEntity Map(OwnableEntityReference reference)
        => OwnableEntityReferenceEntity.Create(reference);

    #endregion

    #region (To Domain) Ownable Entity Reference

    [UserMapping(Default = true)]
    public OwnableEntityReference Map(OwnableEntityReferenceEntity referenceEntity)
        => referenceEntity.ToReference(UEXMapper);

    #endregion

    public interface ICapabilities : IMapperWith<EntityReferenceMapper>
    {
        [UserMapping(Default = true)]
        public OwnableEntityReferenceEntity Map(OwnableEntityReference reference)
            => Reference.Map(reference);

        [UserMapping(Default = true)]
        public OwnableEntityReference Map(OwnableEntityReferenceEntity referenceEntity)
            => Reference.Map(referenceEntity);
    }
}
