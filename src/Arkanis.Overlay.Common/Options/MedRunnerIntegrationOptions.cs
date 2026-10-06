namespace Arkanis.Overlay.Common.Options;

public sealed class MedRunnerIntegrationOptions
{
    // Keep in-app linking and credential processing disabled until the integration is ready.
    public bool AccountLinkingEnabled { get; set; } = true;
}
