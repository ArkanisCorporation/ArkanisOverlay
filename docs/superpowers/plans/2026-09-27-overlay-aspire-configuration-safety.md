# Overlay Aspire Configuration Safety Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.
> Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Adopt the current Kubernetes hosting extensions while making a wrong deployment target or test configuration fail during AppHost construction.

**Architecture:** The library loads conventional configuration and identifies Kubernetes targets.
The AppHost validates one target-specific configuration identity and builds Ingress and PVC resources with resource-local builders.
Tests use distinct test environment names and a synthetic content root.

**Tech Stack:** .NET 10, Aspire 13.5, Arkanis Kubernetes hosting extensions `1.0.0-dev.22`, xUnit v3.

**Spec:** `docs/superpowers/specs/2026-09-27-overlay-aspire-configuration-safety-design.md`

## Global Constraints

- Work in this branch and worktree without modifying unrelated staged or untracked files.
- Use test target names `Kubernetes-Test-Staging` and `Kubernetes-Test-Production`.
- Production CI continues to use `Kubernetes-Staging` and `Kubernetes-Production`.
- Never run `aspire deploy`, Helm, or `kubectl` for verification.
- Do not add persistent tests that inspect AppHost source text.
- Do not change the checked-in SDK pin.

## Review Focus

- A test target with no matching fixture identity must fail before creating a plausible artifact.
- A real target using the fixture content root must fail before creating a plausible artifact.
- An unqualified or malformed Kubernetes environment must not fall back to local project mode.
- A missing image tag must not become a mutable `latest` alias.
- An explicitly disabled public Ingress must fail validation.
- Fixture artifacts must not contain a real namespace or hostname.

### Task 1: Target selection and required configuration

**Files:**

- Modify: `src/Arkanis.Overlay.Host.Aspire/AppHost.cs`
- Modify: `src/Arkanis.Overlay.Host.Aspire/Options/KubernetesDeploymentOptions.cs`
- Modify: `src/Arkanis.Overlay.Host.Aspire/Options/KubernetesPrebuiltImagesOptions.cs`
- Delete: `src/Arkanis.Overlay.Host.Aspire/DeploymentTargetEnvironments.cs`
- Modify: `src/Arkanis.Overlay.Host.Aspire/appsettings.Kubernetes.Staging.json`
- Modify: `src/Arkanis.Overlay.Host.Aspire/appsettings.Kubernetes.Production.json`
- Test: `tests/Arkanis.Overlay.Host.Aspire.IntegrationTests/KubernetesPublishTests.cs`

**Interfaces:** `KubernetesDeploymentOptions.FromConfiguration(IConfiguration, string, ResourceName)` returns the validated namespace.
`KubernetesPrebuiltImagesOptions.FromConfiguration(IConfiguration)` requires an explicit image repository and tag.

- [x] Add a test that builds `Kubernetes-Test-Other` against the fixture root and expects an `InvalidOperationException`.
- [x] Run that test and confirm the failure is due to absent target validation.
- [x] Replace manual JSON layering with `AddDeploymentEnvironmentConfiguration(includeLocalSettings: !builder.Environment.IsKubernetesDeployment())`.
- [x] Validate exact `Kubernetes:Target` identity, namespace, image, Ingress host, TLS secret, cluster issuer, and enabled state for Kubernetes targets.
- [x] Remove the local target extension and mutable image-tag fallbacks.
- [x] Run the focused target-safety test and confirm it passes.

### Task 2: Resource-local Ingress and claim builders

**Files:**

- Modify: `Directory.Packages.props`
- Modify: `src/Arkanis.Overlay.Host.Aspire/Arkanis.Overlay.Host.Aspire.csproj`
- Modify: `src/Arkanis.Overlay.Host.Aspire/AppHost.cs`
- Modify: `src/Arkanis.Overlay.Host.Aspire/appsettings.json`
- Delete: `src/Arkanis.Overlay.Host.Aspire/appsettings.Kubernetes.json`
- Modify: `src/Arkanis.Overlay.Host.Aspire/appsettings.Kubernetes.Staging.json`
- Modify: `src/Arkanis.Overlay.Host.Aspire/appsettings.Kubernetes.Production.json`
- Test: `tests/Arkanis.Overlay.Host.Aspire.IntegrationTests/KubernetesPublishTests.cs`

**Interfaces:** One resource-local `WithKubernetesIngress` uses `overlay-ingress-http` configuration.
One `WithNewKubernetesPersistentVolumeClaim` creates and mounts `overlay-data` at `/var/lib/overlay`.

- [x] Extend the fixture artifact test to inspect generated Ingress, PVC, and Helm values alongside deployment YAML.
- [x] Run the focused artifact test and confirm it exposes the old resource wiring or missing assertions.
- [x] Upgrade the core package to `1.0.0-dev.22` and remove the unused External Secrets package reference and registration.
- [x] Replace the legacy Ingress options and environment-wide PVC registration with resource-local fluent builders.
- [x] Define `XDG_DATA_HOME` from the same mount-path constant and keep the existing workload security and probes.
- [x] Run the focused publish tests and confirm the generated manifests preserve the deployment contract.

### Task 3: Test-only environments and final verification

**Files:**

- Modify: `tests/Arkanis.Overlay.Host.Aspire.IntegrationTests/KubernetesPublishTests.cs`
- Modify: `tests/Arkanis.Overlay.Host.Aspire.IntegrationTests/Fixtures/AspirePublish/appsettings.json`
- Delete: `tests/Arkanis.Overlay.Host.Aspire.IntegrationTests/Fixtures/AspirePublish/appsettings.Kubernetes.json`
- Rename: fixture target JSON files to `appsettings.Kubernetes.Test-Staging.json` and `appsettings.Kubernetes.Test-Production.json`
- Modify: `src/Arkanis.Overlay.Host.Aspire/appsettings.Kubernetes.Staging.json`
- Modify: `src/Arkanis.Overlay.Host.Aspire/appsettings.Kubernetes.Production.json`
- Review: `.github/workflows/build.yaml` and `.github/workflows/_deploy-aspire-kubernetes.yaml`

**Interfaces:** Tests set the fixture content root for both `DistributedApplicationTestingBuilder` and `aspire publish`.
Fixture application environments are `Testing`.

- [x] Rename test target inputs and fixture files before changing fixture content.
- [x] Add fixture target identities and assert synthetic namespace, host, TLS, image, claim, and application environment values.
- [x] Reject inherited `Kubernetes__*` overrides before any test creates or publishes the AppHost model.
- [x] Run tests and confirm wrong target/content-root combinations fail.
- [x] Run the complete Aspire integration-test project and build the AppHost.
- [x] Review the final diff and CI target inputs, ensuring only test names appear in tests and real names remain in deployment callers.
- [x] Report any validation blocked by the missing pinned `10.0.301` SDK separately.
