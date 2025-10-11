namespace Arkanis.Overlay.External.Backend.DTO;

public class SubscriberBrief
{
    public required string SubscriptionTierName { get; set; }
    public required DateOnly SubscribedAt { get; set; }
}

public class SubscriberDiscordIdentity : SubscriberBrief
{
    public required string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
}
