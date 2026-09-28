using System.Diagnostics;
using System.Reflection;
using Arkanis.Aspire.Hosting.Extensions._1Password;
using Arkanis.Aspire.Hosting.Extensions.Kubernetes;
using Arkanis.Aspire.Hosting.Extensions.Kubernetes.KubernetesIngresses;
using Arkanis.Aspire.Hosting.Extensions.Kubernetes.PersistentVolumeClaims;
using Arkanis.Aspire.Hosting.Extensions.Kubernetes.Targeting;
using Arkanis.Overlay.Host.Aspire;
using Arkanis.Overlay.Host.Aspire.Options;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Kubernetes;
using Aspire.Hosting.Kubernetes.Resources;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);
var isKubernetesDeployment = builder.Environment.IsKubernetesDeployment();
if (builder.Environment.EnvironmentName.StartsWith("Kubernetes", StringComparison.OrdinalIgnoreCase)
    && builder.Environment.GetDeploymentEnvironment() is null)
{
    throw new InvalidOperationException(
        $"'{builder.Environment.EnvironmentName}' is not a qualified Kubernetes deployment environment."
    );
}

builder.AddDeploymentEnvironmentConfiguration(includeLocalSettings: !isKubernetesDeployment);
var overlayResourceName = new ResourceName("overlay");

if (!isKubernetesDeployment)
{
    await builder.Use1PasswordAsync("arkaniscorp.1password.com");
}

var kubernetesDeployment = isKubernetesDeployment
    ? KubernetesDeploymentOptions.FromConfiguration(builder.Configuration, builder.Environment.EnvironmentName, overlayResourceName)
    : null;
var prebuiltImages = isKubernetesDeployment ? KubernetesPrebuiltImagesOptions.FromConfiguration(builder.Configuration) : null;
var targetedResources = new TargetedResourceFactory(isKubernetesDeployment);
const string overlayDataName = "overlay-data";
const string overlayDataMountPath = "/var/lib/overlay";
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
    );

#pragma warning restore ASPIREPROBES001

if (isKubernetesDeployment)
{
    overlay.ConfigureAny(resource =>
        {
            resource.WithKubernetesEnvironmentVariables(environment => environment
                .WithConfigurationFrom(builder.Configuration)
                .WithVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED", "true")
                .WithVariable("XDG_DATA_HOME", overlayDataMountPath));
            resource.WithKubernetesIngress(ingress => ingress.WithConfigurationFrom(builder.Configuration));
            resource.WithNewKubernetesPersistentVolumeClaim(
                overlayDataName,
                overlayDataName,
                overlayDataMountPath,
                claim => claim
                    .WithStorageRequest("1Gi")
                    .WithReadWriteOnce()
                    .WithStorageClass("longhorn-ext4-r2")
            );
        }
    );

    var kubernetes = builder.AddKubernetesEnvironment("arkanis-overlay");
    kubernetes.WithHelm(helm =>
        {
            helm.WithChartName("arkanis-overlay")
                .WithChartVersion(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion ?? "0.0.0")
                .WithChartDescription("An Arkanis Overlay deployment.");

            helm.WithNamespace(kubernetesDeployment!.Namespace);
        }
    );
}

var app = builder.Build();
await app.RunAsync();
return 0;

IResourceBuilder<ContainerResource> AddKubernetesOverlayContainer(ResourceName name)
    => builder
        .AddContainer(name.Content, prebuiltImages!.Overlay, prebuiltImages.Tag)
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
    deployment.Spec.Strategy.RollingUpdate = null!;

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
        container.ImagePullPolicy = prebuiltImages!.ImagePullPolicy;
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
