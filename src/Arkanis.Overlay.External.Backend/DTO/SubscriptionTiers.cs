namespace Arkanis.Overlay.External.Backend.DTO;

public class SubscriptionTier
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int Order { get; set; }
}
