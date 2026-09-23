using System.Diagnostics;
using System.Reflection;
using Arkanis.Aspire.Hosting.Extensions._1Password;
using Arkanis.Aspire.Hosting.Extensions.Kubernetes;
using Arkanis.Aspire.Hosting.Extensions.Kubernetes.ExternalSecrets;
using Arkanis.Aspire.Hosting.Extensions.Kubernetes.KubernetesIngresses;
using Arkanis.Aspire.Hosting.Extensions.Kubernetes.Options;
using Arkanis.Aspire.Hosting.Extensions.Kubernetes.PersistentVolumeClaims;
using Arkanis.Aspire.Hosting.Extensions.Kubernetes.Targeting;
using Arkanis.Overlay.Host.Aspire;
using Arkanis.Overlay.Host.Aspire.Options;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Kubernetes;
using Aspire.Hosting.Kubernetes.Resources;
using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

if (builder.Environment.GetDeploymentEnvironment() is { } deploymentEnvironment)
{
    builder.Configuration.AddJsonFile($"appsettings.{deploymentEnvironment.Class}.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddJsonFile(
        $"appsettings.{deploymentEnvironment.Class}.{deploymentEnvironment.EnvironmentType}.json",
        optional: true,
        reloadOnChange: false
    );
}

builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

if (!builder.Environment.IsKubernetesDeployment())
{
    await builder.Use1PasswordAsync("arkaniscorp.1password.com");
}

var kubernetesDeployment = KubernetesDeploymentOptions.FromConfiguration(builder.Configuration);
var prebuiltImages = KubernetesPrebuiltImagesOptions.FromConfiguration(builder.Configuration);
var targetedResources = new TargetedResourceFactory(builder.Environment.IsKubernetesDeployment());
var overlayResourceName = new ResourceName("overlay");
var probeOptions = new ResourceHealthCheckProbeOptions
{
    PeriodSeconds = 10,
    TimeoutSeconds = 5,
};

//? X is for evaluation purposes only and is subject to change or removal in future updates.
#pragma warning disable ASPIREPROBES001

var overlay = targetedResources.AddTargetedApplicationResource(
        () => builder.AddProject<Arkanis_Overlay_Host_Server>(overlayResourceName.Content),
        () => AddKubernetesOverlayContainer(overlayResourceName)
    )
    .ConfigureAny(resource => resource
        .WithExternalHttpEndpoints()
        .AddAllHealthCheckProbes(probeOptions)
    )
    .ConfigureAny(resource => resource.WithKubernetesEnvironmentVariables(environment => environment.WithConfigurationFrom(builder.Configuration)));

#pragma warning restore ASPIREPROBES001

if (builder.Environment.IsKubernetesDeployment())
{
    overlay.ConfigureAny(resource =>
        {
            ConfigureKubernetesIngress(resource, overlayResourceName, "overlay-ingress-http");
            resource.WithKubernetesPersistentVolumeClaim(
                "overlay-data",
                volume => volume.WithConfigurationFrom(builder.Configuration)
            );
        }
    );

    var kubernetes = builder.AddKubernetesEnvironment("arkanis-overlay");
    kubernetes.WithHelm(helm =>
        {
            helm.WithChartName("arkanis-overlay")
                .WithChartVersion(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion ?? "0.0.0")
                .WithChartDescription("An Arkanis Overlay deployment.");

            if (!string.IsNullOrWhiteSpace(kubernetesDeployment.Namespace))
            {
                helm.WithNamespace(kubernetesDeployment.Namespace);
            }
        }
    );
    kubernetes.WithExternalSecrets(ExternalSecretsOptions.FromConfiguration(builder.Configuration));
    kubernetes.WithPersistentVolumeClaims(KubernetesPersistentVolumeClaimsOptions.FromConfiguration(builder.Configuration));
}

var app = builder.Build();
await app.RunAsync();
return 0;

IResourceBuilder<ContainerResource> AddKubernetesOverlayContainer(ResourceName name)
    => builder
        .AddContainer(name.Content, prebuiltImages.Overlay, prebuiltImages.Tag)
        .WithHttpEndpoint(targetPort: 8080, env: "HTTP_PORTS")
        .PublishAsKubernetesService(resource =>
            {
                if (resource.Workload is not Deployment deployment)
                {
                    return;
                }

                ConfigureKubernetesOverlayDeployment(deployment);
            }
        );

void ConfigureKubernetesOverlayDeployment(Deployment deployment)
{
    deployment.Spec.Replicas = 1;
    deployment.Spec.Strategy.Type = "Recreate";

    var podSpec = deployment.Spec.Template.Spec;
    podSpec.SecurityContext = new PodSecurityContextV1
    {
        RunAsNonRoot = true,
        RunAsUser = 1654,
        RunAsGroup = 1654,
        FsGroup = 1654,
    };
    podSpec.ImagePullSecrets.Add(new LocalObjectReferenceV1 { Name = "ghcr-pull" });

    foreach (var container in podSpec.Containers)
    {
        container.ImagePullPolicy = prebuiltImages.ImagePullPolicy;
        var securityContext = new SecurityContextV1
        {
            AllowPrivilegeEscalation = false,
            Capabilities = new CapabilitiesV1(),
        };
        securityContext.Capabilities.Drop.Add("ALL");
        container.SecurityContext = securityContext;

        NormalizeKubernetesHttpProbeScheme(container.StartupProbe);
        NormalizeKubernetesHttpProbeScheme(container.LivenessProbe);
        NormalizeKubernetesHttpProbeScheme(container.ReadinessProbe);
    }
}

void NormalizeKubernetesHttpProbeScheme(ProbeV1? probe)
{
    var httpGet = probe?.HttpGet;
    if (httpGet is null || string.IsNullOrWhiteSpace(httpGet.Scheme))
    {
        return;
    }

    httpGet.Scheme = httpGet.Scheme switch
    {
        var scheme when string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) => "HTTP",
        var scheme when string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) => "HTTPS",
        var scheme => scheme,
    };
}

void ConfigureKubernetesIngress<T>(IResourceBuilder<T> resourceBuilder, ResourceName resourceName, string ingressName)
    where T : IResourceWithEndpoints
{
    var ingresses = KubernetesIngressOptions.FromConfiguration(builder.Configuration).GetResourceIngresses(resourceName);
    var unsupportedIngressNames = ingresses.Keys
        .Where(name => !string.Equals(name, ingressName, StringComparison.Ordinal))
        .Order(StringComparer.Ordinal)
        .ToArray();

    if (unsupportedIngressNames.Length > 0)
    {
        throw new InvalidOperationException(
            $"Ingress configuration for resource '{resourceName.Content}' contains unsupported ingress item(s): "
            + string.Join(", ", unsupportedIngressNames.Select(static name => $"'{name}'"))
            + "."
        );
    }

    if (!ingresses.TryGetValue(ingressName, out var ingress) || !ingress.ShouldPublish)
    {
        return;
    }

    resourceBuilder.WithKubernetesIngress(resourceIngress => resourceIngress.WithConfigurationFrom(builder.Configuration));
}
