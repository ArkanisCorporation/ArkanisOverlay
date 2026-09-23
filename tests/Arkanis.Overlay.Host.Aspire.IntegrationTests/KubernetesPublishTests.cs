namespace Arkanis.Overlay.Host.Aspire.IntegrationTests;

using global::Arkanis.Aspire.Hosting.Extensions.Kubernetes.KubernetesIngresses;
using global::Arkanis.Aspire.Hosting.Extensions.Kubernetes.PersistentVolumeClaims;
using global::Aspire.Hosting.ApplicationModel;
using global::Aspire.Hosting.Testing;
using global::Microsoft.Extensions.Hosting;
using global::Shouldly;
using global::Xunit;
using AppHostMarker = global::Arkanis.Overlay.Host.Aspire.AppHostMarker;

public sealed class KubernetesPublishTests
{
    [Theory]
    [InlineData("Kubernetes-Staging", "overlay-fixture-staging", "Staging", "overlay-fixture-tls-staging")]
    [InlineData("Kubernetes-Production", "overlay-fixture-production", "Production", "overlay-fixture-tls-production")]
    public async Task Kubernetes_fixture_models_a_non_production_overlay_deployment(
        string deploymentEnvironment,
        string expectedNamespace,
        string expectedApplicationEnvironment,
        string expectedTlsSecretName
    )
    {
        await using var builder = await DistributedApplicationTestingBuilder.CreateAsync<AppHostMarker>(
            args: [],
            configureBuilder: (_, hostSettings) =>
            {
                hostSettings.EnvironmentName = deploymentEnvironment;
                hostSettings.ContentRootPath = FixtureConfigurationRoot;
            },
            TestContext.Current.CancellationToken
        );

        builder.Configuration["Kubernetes:Namespace"].ShouldBe(expectedNamespace);
        builder.Configuration["Kubernetes:Images:Overlay"].ShouldBe("ghcr.io/example/overlay-fixture");
        builder.Configuration["Kubernetes:Images:Tag"].ShouldBe("v0.0.0-fixture");
        var ingressHost = builder.Configuration["Kubernetes:Ingress:Resources:overlay:overlay-ingress-http:Host"];
        var configuredNamespace = builder.Configuration["Kubernetes:Namespace"];
        ingressHost.ShouldBe("overlay.fixture.invalid");
        ingressHost!.ShouldNotContain("overlay.arkanis");
        configuredNamespace!.ShouldNotContain("arkanis-overlay");

        var overlay = builder.Resources.OfType<ContainerResource>().Single(resource => resource.Name == "overlay");
        overlay.TryGetContainerImageName(useBuiltImage: false, out var image).ShouldBeTrue();
        image.ShouldBe("ghcr.io/example/overlay-fixture:v0.0.0-fixture");

#pragma warning disable ASPIREPROBES001
        overlay.Annotations.OfType<EndpointProbeAnnotation>()
            .Select(probe => (probe.Type, probe.Path))
            .ShouldBe(
                [
                    (ProbeType.Liveness, "/healthz/alive"),
                    (ProbeType.Readiness, "/healthz/ready"),
                    (ProbeType.Startup, "/healthz/startup"),
                ],
                ignoreOrder: true
            );
#pragma warning restore ASPIREPROBES001

        var ingress = overlay.Annotations.OfType<KubernetesIngressAnnotation>().Single();
        ingress.Name.ShouldBe("overlay-ingress-http");
        ingress.Host.ShouldBe("overlay.fixture.invalid");
        ingress.TlsSecretName.ShouldBe(expectedTlsSecretName);

        var volume = overlay.Annotations.OfType<KubernetesPersistentVolumeClaimMountAnnotation>().Single();
        volume.PersistentVolumeClaimName.ToString().ShouldBe("overlay-data");
        volume.MountPath.ToString().ShouldBe("/var/lib/overlay");
        builder.Configuration["Kubernetes:EnvironmentVariables:Shared:DOTNET_ENVIRONMENT"].ShouldBe(expectedApplicationEnvironment);
    }

    private static string FixtureConfigurationRoot
    {
        get
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            {
                var fixturePath = Path.Combine(
                    directory.FullName,
                    "tests",
                    "Arkanis.Overlay.Host.Aspire.IntegrationTests",
                    "Fixtures",
                    "AspirePublish"
                );

                if (File.Exists(Path.Combine(fixturePath, "appsettings.json")))
                {
                    return fixturePath;
                }
            }

            throw new DirectoryNotFoundException("Could not locate the Aspire publish-test fixture configuration.");
        }
    }
}
