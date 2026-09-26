namespace Arkanis.Overlay.Host.Aspire;

using Microsoft.Extensions.Hosting;

public static class DeploymentTargetEnvironments
{
    public const string Kubernetes = nameof(Kubernetes);

    public static DeploymentEnvironment? GetDeploymentEnvironment(this IHostEnvironment hostEnvironment)
        => hostEnvironment.EnvironmentName.Split(separator: '-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) switch
        {
            [Kubernetes, var name] => new DeploymentEnvironment(Kubernetes, name),
            _ => null,
        };

    public static bool IsKubernetesDeployment(this IHostEnvironment hostEnvironment)
        => hostEnvironment.EnvironmentName.StartsWith(Kubernetes, StringComparison.OrdinalIgnoreCase);
}

public sealed record DeploymentEnvironment(string Class, string EnvironmentType);
