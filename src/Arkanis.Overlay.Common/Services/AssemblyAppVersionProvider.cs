namespace Arkanis.Overlay.Common.Services;

using System.Reflection;
using Abstractions;
using Abstractions.Services;
using NuGet.Versioning;

public sealed class AssemblyAppVersionProvider : IAppVersionProvider
{
    public SemanticVersion CurrentVersion { get; } = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>() is { } versionAttribute
        ? SemanticVersion.Parse(versionAttribute.InformationalVersion)
        : new SemanticVersion(0, 0, 1, "dev");

    public string CurrentVelopackChannelId
        => "__unspecified";

    public DateTimeOffset? AutoUpdateCheckAt
        => null;
}
