namespace Arkanis.Overlay.Host.Aspire.IntegrationTests;

using global::System.Diagnostics;
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

    [Theory]
    [InlineData("Kubernetes-Staging")]
    [InlineData("Kubernetes-Production")]
    public async Task Kubernetes_fixture_publish_emits_a_hardened_overlay_workload(string deploymentEnvironment)
    {
        var deployment = (await PublishFixtureAsync(deploymentEnvironment)).ReplaceLineEndings("\n");

        deployment.ShouldContain("replicas: 1");
        deployment.ShouldContain("type: \"Recreate\"");
        deployment.ShouldContain("runAsNonRoot: true");
        deployment.ShouldContain("runAsUser: 1654");
        deployment.ShouldContain("runAsGroup: 1654");
        deployment.ShouldContain("fsGroup: 1654");
        deployment.ShouldContain("allowPrivilegeEscalation: false");
        deployment.ShouldContain("drop:");
        deployment.ShouldContain("- \"ALL\"");
        deployment.ShouldContain("imagePullSecrets:");
        deployment.ShouldContain("name: \"ghcr-pull\"");
        deployment.ShouldContain("mountPath: \"/var/lib/overlay\"");
        deployment.ShouldContain("readOnly: false");
        deployment.ShouldContain("scheme: \"HTTP\"");
    }

    private static async Task<string> PublishFixtureAsync(string deploymentEnvironment)
    {
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            "arkanis-overlay-aspire-publish-tests",
            Guid.NewGuid().ToString("N")
        );
        var generatedConfigPath = Path.Combine(RepositoryRoot, "aspire.config.json");
        var generatedConfigExisted = File.Exists(generatedConfigPath);
        Directory.CreateDirectory(outputPath);

        try
        {
            var dotnetExecutable = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(dotnetExecutable)
                {
                    WorkingDirectory = RepositoryRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };
            if (Path.GetDirectoryName(dotnetExecutable) is { Length: > 0 } dotnetDirectory)
            {
                process.StartInfo.Environment["PATH"] = string.Join(
                    Path.PathSeparator,
                    dotnetDirectory,
                    process.StartInfo.Environment["PATH"]
                );
            }
            process.StartInfo.ArgumentList.Add("tool");
            process.StartInfo.ArgumentList.Add("run");
            process.StartInfo.ArgumentList.Add("aspire");
            process.StartInfo.ArgumentList.Add("--");
            process.StartInfo.ArgumentList.Add("publish");
            process.StartInfo.ArgumentList.Add("--no-build");
            process.StartInfo.ArgumentList.Add("--apphost");
            process.StartInfo.ArgumentList.Add(Path.Combine(RepositoryRoot, "src", "Arkanis.Overlay.Host.Aspire"));
            process.StartInfo.ArgumentList.Add("--output-path");
            process.StartInfo.ArgumentList.Add(outputPath);
            process.StartInfo.ArgumentList.Add("--environment");
            process.StartInfo.ArgumentList.Add(deploymentEnvironment);
            process.StartInfo.ArgumentList.Add("--non-interactive");
            process.StartInfo.ArgumentList.Add("--");
            process.StartInfo.ArgumentList.Add("--contentRoot");
            process.StartInfo.ArgumentList.Add(FixtureConfigurationRoot);

            process.Start().ShouldBeTrue();
            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            var output = (await standardOutput) + Environment.NewLine + (await standardError);
            process.ExitCode.ShouldBe(0, output);

            return await File.ReadAllTextAsync(
                Path.Combine(outputPath, "templates", "overlay", "deployment.yaml"),
                TestContext.Current.CancellationToken
            );
        }
        finally
        {
            Directory.Delete(outputPath, recursive: true);

            if (!generatedConfigExisted && File.Exists(generatedConfigPath))
            {
                File.Delete(generatedConfigPath);
            }
        }
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

    private static string RepositoryRoot
    {
        get
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ArkanisOverlay.sln")))
                {
                    return directory.FullName;
                }
            }

            throw new DirectoryNotFoundException("Could not locate the repository root.");
        }
    }
}
