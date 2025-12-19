namespace Arkanis.Overlay.Infrastructure.Options;

using Common.Abstractions;

public class PostHogOptions : ISelfBindableOptions
{
    public required string ProjectApiKey { get; set; } = "phc_caOWOfDJTioxQRQqiOVZvNjLzefL65PY6GFu2buAWoy";
    public required string ProjectApiHost { get; set; } = "https://eu.i.posthog.com";
    public required string ProjectUiHost { get; set; } = "https://eu.posthog.com";

    public string SectionPath
        => "PostHog";
}
