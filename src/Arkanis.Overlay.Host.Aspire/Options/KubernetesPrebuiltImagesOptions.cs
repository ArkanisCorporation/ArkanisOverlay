namespace Arkanis.Overlay.Host.Aspire.Options;

using Microsoft.Extensions.Configuration;

public sealed class KubernetesPrebuiltImagesOptions
{
    private const string ConfigurationPath = "Kubernetes:Images";
    private const string VersionTagConfigurationKey = "VERSION_TAG";
    private const string DefaultTag = "staging-latest";

    public string Overlay { get; set; } = "ghcr.io/arkaniscorporation/arkanisoverlay";

    public string Tag { get; set; } = DefaultTag;

    public string ImagePullPolicy
        => Tag.EndsWith("latest", StringComparison.OrdinalIgnoreCase) ? "Always" : "IfNotPresent";

    public static KubernetesPrebuiltImagesOptions FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new KubernetesPrebuiltImagesOptions();
        configuration.GetSection(ConfigurationPath).Bind(options);

        var versionTag = configuration[VersionTagConfigurationKey];
        options.Tag = !string.IsNullOrWhiteSpace(versionTag)
            ? versionTag
            : string.IsNullOrWhiteSpace(options.Tag)
                ? DefaultTag
                : options.Tag;

        return options;
    }
}
