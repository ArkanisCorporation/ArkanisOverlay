namespace Arkanis.Overlay.External.CitizenId;

using System.Diagnostics.CodeAnalysis;
using Common.Services;
using Duende.IdentityModel.OidcClient;
using global::CitizenId.Domain.Shared.Authorization;
using Microsoft.Extensions.Options;
using Options;
using Overlay.Common.Abstractions;
using Overlay.Common.Models;
using Overlay.Common.Services;
using Quartz;

public class CitizenIdAuthenticator(IServiceProvider serviceProvider, IOptionsMonitor<CitizenIdOptions> options) : OidcAuthenticator(serviceProvider)
{
    public override ExternalAuthenticatorInfo AuthenticatorInfo
        => CitizenIdConstants.ProviderInfo;

    public override Options CurrentOptions { get; } = new()
    {
        AvatarUrlClaimTypes = [CitizenIdClaims.User.Rsi.AvatarUrl, CitizenIdClaims.User.Discord.AvatarUrl, "picture"],
    };

    public override OidcClient OidcClient
        => new(OidcOptions);

    [field: MaybeNull]
    private OidcClientOptions OidcOptions
        => field
           ?? new OidcClientOptions
           {
               Authority = options.CurrentValue.Authority.ToString(),
               ClientId = options.CurrentValue.ClientId,
               Scope = string.Join(" ", options.CurrentValue.Scopes),
               LoadProfile = false,
           };

    public static IJobScheduleProvider CreateRefreshJobScheduleProvider()
        => new JobScheduleProviderFactory(
            () => JobBuilder.Create<OidcAuthenticatorRefreshJob<CitizenIdAuthenticator>>()
                .WithIdentity($"{nameof(CitizenIdAuthenticator)}-RefreshJob")
                .WithDescription("Refreshes the Citizen iD credentials before their expiration.")
                .SetJobData(OidcAuthenticatorRefreshJob.CreateJobData(TimeSpan.FromMinutes(30)))
                .Build(),
            () => TriggerBuilder.Create()
                .WithIdentity($"{nameof(CitizenIdAuthenticator)}-RefreshJob-Trigger")
                .WithDescription("Represents the refresh interval for Citizen iD credentials.")
                .WithSimpleSchedule(x => x
                    .WithInterval(TimeSpan.FromMinutes(20))
                    .RepeatForever()
                )
                .StartNow()
                .Build()
        );
}
