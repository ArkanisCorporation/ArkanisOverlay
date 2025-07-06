namespace Arkanis.Overlay.Host.Desktop.Services;

using Common.Abstractions;
using Common.Abstractions.Services;
using Components.Services;
using Domain.Abstractions.Services;
using Microsoft.Extensions.Hosting;

public sealed class DesktopAnalyticsPropertyProvider(
    IHostEnvironment hostEnvironment,
    IAppVersionProvider versionProvider,
    IUserPreferencesProvider userPreferencesProvider
) : SharedAnalyticsPropertyProvider(hostEnvironment, versionProvider, userPreferencesProvider)
{
    protected override string ApplicationType
        => DesktopHostModule.Namespace;
}
