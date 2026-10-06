namespace Arkanis.Overlay.External.Backend;

using DTO;
using Riok.Mapperly.Abstractions;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class BackendMapper
{
    [MapDerivedType<IGetActiveSubscribers_Subscribers_AnonymousSubscriber, SubscriberBrief>]
    [MapDerivedType<IGetActiveSubscribers_Subscribers_DiscordSubscriber, SubscriberDiscordIdentity>]
    public static partial SubscriberBrief MapToDTO(this IGetActiveSubscribers_Subscribers subscriber);

    public static partial SubscriptionTier MapToDTO(this SubscriptionTier subTier);
}
