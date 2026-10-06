namespace Arkanis.Overlay.Infrastructure.Options;

using Common.Abstractions;

public class ConfigurationOptions : ISelfBindableOptions
{
    public string SectionPath
        => "Configuration";
}
