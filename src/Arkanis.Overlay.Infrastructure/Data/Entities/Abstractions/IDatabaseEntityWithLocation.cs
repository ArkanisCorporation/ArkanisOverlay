namespace Arkanis.Overlay.Infrastructure.Data.Entities.Abstractions;

using Domain.Models.Game;

internal interface IDatabaseEntityWithLocation
{
    UexApiGameEntityId LocationId { get; set; }
}
