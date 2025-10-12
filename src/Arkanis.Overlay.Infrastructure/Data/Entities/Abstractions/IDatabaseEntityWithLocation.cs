namespace Arkanis.Overlay.Infrastructure.Data.Entities.Abstractions;

using Domain.Models.Game;

internal interface IDatabaseEntityWithLocation
{
    public UexApiGameEntityId LocationId { get; set; }
}
