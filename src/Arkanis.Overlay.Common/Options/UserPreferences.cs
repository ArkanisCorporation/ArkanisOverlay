namespace Arkanis.Overlay.Common.Options;

using System.Diagnostics.Contracts;
using System.Globalization;
using Models;
using Models.Keyboard;

public record UserPreferences
{
    public CultureInfo ActiveCultureInfo
        => CustomRegionInfo is not null
            ? CultureInfo.GetCultureInfo($"{ActiveCulture.TwoLetterISOLanguageName}-{CustomRegionInfo.TwoLetterISORegionName}")
            : ActiveCulture;

    private CultureInfo ActiveCulture
        => CustomCultureInfo ?? CultureInfo.CurrentCulture;

    public Guid InstallationId { get; init; } = Guid.NewGuid();

    public UpdateChannel UpdateChannel { get; set; } = UpdateChannel.Default;

    public bool AutoStartWithBoot { get; set; } = true;

    public bool TerminateOnGameExit { get; set; }

    public bool DisableAnalytics { get; set; }

    public bool BlurBackground { get; set; }

    public CultureInfo? CustomCultureInfo { get; set; } = CultureInfo.GetCultureInfo("en");

    public RegionInfo? CustomRegionInfo { get; set; }

    public KeyboardShortcut InGameLaunchShortcut { get; set; } = new([KeyboardKey.F3]);

    public KeyboardShortcut LaunchShortcut { get; set; } = new([KeyboardKey.AltLeft, KeyboardKey.ShiftLeft, KeyboardKey.KeyS]);

    public List<AccountCredentials> ExternalServiceCredentials { get; set; } = [];

    public AccountCredentials? GetCredentialsOrDefaultFor(string serviceId)
        => ExternalServiceCredentials.FirstOrDefault(x => x.ServiceId == serviceId);

    public AccountCredentials GetOrCreateCredentialsFor(string serviceId)
    {
        if (GetCredentialsOrDefaultFor(serviceId) is not { } credentials)
        {
            ExternalServiceCredentials.Add(credentials = new AccountEmptyCredentials(serviceId));
        }

        return credentials;
    }

    [Pure]
    public UserPreferences SetCredentials(AccountCredentials accountCredentials)
    {
        ExternalServiceCredentials.RemoveAll(x => x.ServiceId == accountCredentials.ServiceId);
        return this with
        {
            ExternalServiceCredentials = ExternalServiceCredentials.Append(accountCredentials).ToList(),
        };
    }

    [Pure]
    public UserPreferences RemoveCredentialsFor(string serviceId)
        => this with
        {
            ExternalServiceCredentials = ExternalServiceCredentials.Where(x => x.ServiceId != serviceId).ToList(),
        };
}
