namespace Arkanis.Overlay.Host.Aspire.Options;

using Arkanis.Aspire.Hosting.Extensions.Kubernetes.Options;
using Microsoft.Extensions.Configuration;

public sealed class KubernetesDeploymentOptions
{
    private const string ConfigurationPath = "Kubernetes";

    public string? Namespace { get; set; }

    public KubernetesEnvironmentVariablesOptions EnvironmentVariables { get; set; } = new();

    public static KubernetesDeploymentOptions FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new KubernetesDeploymentOptions();
        configuration.GetSection(ConfigurationPath).Bind(options);
        options.EnvironmentVariables = KubernetesEnvironmentVariablesOptions.FromConfiguration(configuration);
        return options;
    }
}
