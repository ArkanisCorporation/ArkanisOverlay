namespace Arkanis.Overlay.Domain.Options;

using System.Globalization;
using Common.Models;
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

    public KeyboardShortcut LaunchShortcut { get; set; } = new([KeyboardKey.AltLeft, KeyboardKey.ShiftLeft, KeyboardKey.KeyS]);

    public List<Credentials> ExternalServiceCredentials { get; set; } = [];

    public Credentials GetOrCreateCredentialsFor(string serviceId)
    {
        if (ExternalServiceCredentials.FirstOrDefault(x => x.ServiceId == serviceId) is not { } credentials)
        {
            ExternalServiceCredentials.Add(credentials = new Credentials(serviceId));
        }

        return credentials;
    }

    public class Credentials(string serviceId)
    {
        public string ServiceId { get; init; } = serviceId;

        public string? UserIdentifier { get; set; }
        public string? SecretToken { get; set; }
    }
}
