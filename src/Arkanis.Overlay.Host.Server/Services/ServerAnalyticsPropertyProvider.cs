namespace Arkanis.Overlay.Host.Server.Services;

using Common.Abstractions.Services;
using Domain.Abstractions.Services;
using Overlay.Components.Services;

public class ServerAnalyticsPropertyProvider(
    IHostEnvironment hostEnvironment,
    IAppVersionProvider versionProvider,
    IUserPreferencesProvider userPreferencesProvider
) : SharedAnalyticsPropertyProvider(hostEnvironment, versionProvider, userPreferencesProvider)
{
    protected override string ApplicationType
        => ServerHostModule.Namespace;
}
