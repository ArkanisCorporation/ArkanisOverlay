namespace Arkanis.Overlay.Host.Aspire.IntegrationTests;

using global::System.Diagnostics;
using global::Arkanis.Aspire.Hosting.Extensions.Kubernetes.KubernetesIngresses;
using global::Arkanis.Aspire.Hosting.Extensions.Kubernetes.PersistentVolumeClaims;
using global::Arkanis.Aspire.Hosting.Extensions.Kubernetes;
using global::Arkanis.Overlay.Host.Aspire.Options;
using global::Aspire.Hosting.ApplicationModel;
using global::Aspire.Hosting.Testing;
using global::Microsoft.Extensions.Configuration;
using global::Microsoft.Extensions.Hosting;
using global::Shouldly;
using global::Xunit;
using AppHostMarker = global::Arkanis.Overlay.Host.Aspire.AppHostMarker;

public sealed class KubernetesPublishTests
{
    [Theory]
    [InlineData("Kubernetes-Test-Other")]
    [InlineData("Kubernetes")]
    [InlineData("KubernetesPreview")]
    public async Task Missing_or_malformed_target_rejects_the_test_fixture(string deploymentEnvironment)
    {
        EnsureNoKubernetesEnvironmentOverrides();
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
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
        });
    }

    [Fact]
    public void Mismatched_fixture_target_is_rejected()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Kubernetes:Target"] = "Kubernetes-Test-Staging",
            })
            .Build();

        Assert.Throws<InvalidOperationException>(() =>
            KubernetesDeploymentOptions.FromConfiguration(configuration, "Kubernetes-Test-Other", new ResourceName("overlay")));
    }

    [Theory]
    [InlineData("Kubernetes:Images:Overlay")]
    [InlineData("Kubernetes:Images:Tag")]
    public void Kubernetes_image_reference_must_be_explicit(string missingKey)
    {
        var values = new Dictionary<string, string?>
        {
            ["Kubernetes:Images:Overlay"] = "ghcr.io/example/overlay-fixture",
            ["Kubernetes:Images:Tag"] = "v0.0.0-fixture",
        };
        values.Remove(missingKey);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        var exception = Assert.Throws<InvalidOperationException>(() => KubernetesPrebuiltImagesOptions.FromConfiguration(configuration));
        exception.Message.ShouldContain(missingKey);
    }

    [Theory]
    [InlineData("Kubernetes:Namespace")]
    [InlineData("Kubernetes:Ingress:Resources:overlay:overlay-ingress-http:Host")]
    [InlineData("Kubernetes:Ingress:Resources:overlay:overlay-ingress-http:TlsSecretName")]
    [InlineData("Kubernetes:Ingress:CertClusterIssuerName")]
    public void Kubernetes_target_requires_its_deployment_settings(string missingKey)
    {
        var values = new Dictionary<string, string?>
        {
            ["Kubernetes:Target"] = "Kubernetes-Test-Staging",
            ["Kubernetes:Namespace"] = "overlay-fixture-staging",
            ["Kubernetes:Ingress:Resources:overlay:overlay-ingress-http:Host"] = "overlay.fixture.invalid",
            ["Kubernetes:Ingress:Resources:overlay:overlay-ingress-http:TlsSecretName"] = "overlay-fixture-tls-staging",
            ["Kubernetes:Ingress:CertClusterIssuerName"] = "fixture-issuer",
        };
        values.Remove(missingKey);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            KubernetesDeploymentOptions.FromConfiguration(configuration, "Kubernetes-Test-Staging", new ResourceName("overlay")));
        exception.Message.ShouldContain(missingKey);
    }

    [Fact]
    public void Kubernetes_target_rejects_a_disabled_ingress()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Kubernetes:Target"] = "Kubernetes-Test-Staging",
                ["Kubernetes:Namespace"] = "overlay-fixture-staging",
                ["Kubernetes:Ingress:CertClusterIssuerName"] = "fixture-issuer",
                ["Kubernetes:Ingress:Resources:overlay:overlay-ingress-http:Host"] = "overlay.fixture.invalid",
                ["Kubernetes:Ingress:Resources:overlay:overlay-ingress-http:TlsSecretName"] = "overlay-fixture-tls-staging",
                ["Kubernetes:Ingress:Resources:overlay:overlay-ingress-http:Enabled"] = "false",
            })
            .Build();

        Assert.Throws<InvalidOperationException>(() =>
            KubernetesDeploymentOptions.FromConfiguration(configuration, "Kubernetes-Test-Staging", new ResourceName("overlay")));
    }

    [Theory]
    [InlineData("Kubernetes-Test-Staging", "overlay-fixture-staging", "Testing", "overlay-fixture-tls-staging")]
    [InlineData("Kubernetes-Test-Production", "overlay-fixture-production", "Testing", "overlay-fixture-tls-production")]
    public async Task Kubernetes_fixture_models_a_non_production_overlay_deployment(
        string deploymentEnvironment,
        string expectedNamespace,
        string expectedApplicationEnvironment,
        string expectedTlsSecretName
    )
    {
        EnsureNoKubernetesEnvironmentOverrides();
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
    [InlineData("Kubernetes-Test-Staging", "overlay-fixture-tls-staging")]
    [InlineData("Kubernetes-Test-Production", "overlay-fixture-tls-production")]
    public async Task Kubernetes_fixture_publish_emits_a_hardened_overlay_workload(
        string deploymentEnvironment,
        string expectedTlsSecretName
    )
    {
        var published = await PublishFixtureAsync(deploymentEnvironment);
        var deployment = published.Deployment.ReplaceLineEndings("\n");

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
        deployment.ShouldContain("envFrom:");
        deployment.ShouldContain("overlay-config");
        published.Values.ShouldContain("XDG_DATA_HOME: \"/var/lib/overlay\"");
        published.Values.ShouldContain("ASPNETCORE_FORWARDEDHEADERS_ENABLED: \"true\"");
        published.Values.ShouldContain("DOTNET_ENVIRONMENT: \"Testing\"");
        published.Claim.ShouldContain("overlay-data");
        published.Claim.ShouldContain("ReadWriteOnce");
        published.Claim.ShouldContain("longhorn-ext4-r2");
        published.Ingress.ShouldContain("overlay.fixture.invalid");
        published.Ingress.ShouldContain(expectedTlsSecretName);
        published.Ingress.ShouldContain("fixture-issuer");
        published.AllYaml.ShouldNotContain("arkanis-overlay-staging");
        published.AllYaml.ShouldNotContain("arkanis-overlay-production");
        published.AllYaml.ShouldNotContain("overlay.arkanis.dev");
        published.AllYaml.ShouldNotContain("overlay.arkanis.space");
        published.AllYaml.ShouldNotContain("ghcr.io/arkaniscorporation/arkanisoverlay");
        published.AllYaml.ShouldNotContain("onepassword-connect");
        published.AllYaml.ShouldNotContain("op://");
    }

    private static async Task<PublishedFixture> PublishFixtureAsync(string deploymentEnvironment)
    {
        EnsureNoKubernetesEnvironmentOverrides();
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

            var yamlPaths = Directory.GetFiles(outputPath, "*.yaml", SearchOption.AllDirectories);
            var yaml = await Task.WhenAll(yamlPaths.Select(path => File.ReadAllTextAsync(path, TestContext.Current.CancellationToken)));
            var manifests = yamlPaths.Zip(yaml).ToDictionary(pair => pair.First, pair => pair.Second);
            return new PublishedFixture(
                manifests.Single(pair => pair.Key.EndsWith(Path.Combine("overlay", "deployment.yaml"), StringComparison.Ordinal)).Value,
                manifests.Single(pair => Path.GetFileName(pair.Key) == "persistentvolumeclaim.yaml").Value,
                manifests.Single(pair => Path.GetFileName(pair.Key) == "ingress.yaml").Value,
                manifests.Single(pair => Path.GetFileName(pair.Key) == "values.yaml").Value,
                string.Join("\n", yaml)
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

    private sealed record PublishedFixture(string Deployment, string Claim, string Ingress, string Values, string AllYaml);

    private static void EnsureNoKubernetesEnvironmentOverrides()
    {
        var overrides = Environment.GetEnvironmentVariables().Keys.Cast<string>()
            .Where(key => key.StartsWith("Kubernetes__", StringComparison.OrdinalIgnoreCase)
                          || key.StartsWith("Kubernetes:", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (overrides.Length > 0)
        {
            throw new InvalidOperationException(
                $"Aspire publish tests require a clean Kubernetes configuration environment; found {string.Join(", ", overrides)}."
            );
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
