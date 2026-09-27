namespace Arkanis.Overlay.Host.Aspire.Options;

using Arkanis.Aspire.Hosting.Extensions.Kubernetes;
using Microsoft.Extensions.Configuration;

public sealed class KubernetesDeploymentOptions
{
    public required string Namespace { get; init; }

    public static KubernetesDeploymentOptions FromConfiguration(
        IConfiguration configuration,
        string environmentName,
        ResourceName resourceName
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);
        var resource = resourceName.Content;

        var configuredTarget = configuration["Kubernetes:Target"];
        if (!string.Equals(configuredTarget, environmentName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Kubernetes:Target must match the selected Aspire environment '{environmentName}', but was '{configuredTarget ?? "<missing>"}'."
            );
        }

        var namespaceName = Require(configuration, "Kubernetes:Namespace");
        var ingressName = $"{resource}-ingress-http";
        var resourceIngressPath = $"Kubernetes:Ingress:Resources:{resource}";
        var ingressPath = $"{resourceIngressPath}:{ingressName}";
        Require(configuration, $"{ingressPath}:Host");
        Require(configuration, $"{ingressPath}:TlsSecretName");
        Require(configuration, "Kubernetes:Ingress:CertClusterIssuerName");
        if (configuration.GetValue<bool?>($"{ingressPath}:Enabled") is false)
        {
            throw new InvalidOperationException($"'{ingressPath}' must be enabled for Kubernetes publishing.");
        }

        var unsupportedIngresses = configuration.GetSection(resourceIngressPath)
            .GetChildren()
            .Select(child => child.Key)
            .Where(name => !string.Equals(name, ingressName, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (unsupportedIngresses.Length > 0)
        {
            throw new InvalidOperationException(
                $"Ingress configuration for resource '{resource}' contains unsupported ingress item(s): {string.Join(", ", unsupportedIngresses)}."
            );
        }

        return new KubernetesDeploymentOptions { Namespace = namespaceName };
    }

    private static string Require(IConfiguration configuration, string key)
        => !string.IsNullOrWhiteSpace(configuration[key])
            ? configuration[key]!.Trim()
            : throw new InvalidOperationException($"'{key}' is required for Kubernetes publishing.");
}
