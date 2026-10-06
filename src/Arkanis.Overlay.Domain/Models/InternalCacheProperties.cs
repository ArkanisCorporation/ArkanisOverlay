namespace Arkanis.Overlay.Domain.Models;

public class InternalCacheProperties
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DataCached DataState { get; set; }
}
