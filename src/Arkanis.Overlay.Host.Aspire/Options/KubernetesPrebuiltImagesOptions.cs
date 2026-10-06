namespace Arkanis.Overlay.Host.Aspire.Options;

using Microsoft.Extensions.Configuration;

public sealed class KubernetesPrebuiltImagesOptions
{
    private const string ConfigurationPath = "Kubernetes:Images";
    public required string Overlay { get; init; }

    public required string Tag { get; init; }

    public string ImagePullPolicy
        => Tag.EndsWith("latest", StringComparison.OrdinalIgnoreCase) ? "Always" : "IfNotPresent";

    public static KubernetesPrebuiltImagesOptions FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return new KubernetesPrebuiltImagesOptions
        {
            Overlay = Require(configuration, $"{ConfigurationPath}:Overlay"),
            Tag = Require(configuration, $"{ConfigurationPath}:Tag"),
        };
    }

    private static string Require(IConfiguration configuration, string key)
        => !string.IsNullOrWhiteSpace(configuration[key])
            ? configuration[key]!.Trim()
            : throw new InvalidOperationException($"'{key}' is required for Kubernetes publishing.");
}
