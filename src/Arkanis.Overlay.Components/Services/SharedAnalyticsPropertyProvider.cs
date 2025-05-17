namespace Arkanis.Overlay.Components.Services;

using Common.Abstractions;
using Domain.Abstractions.Services;
using Microsoft.Extensions.Hosting;

public abstract class SharedAnalyticsPropertyProvider
{
    private const string EnvironmentTypeKey = "environment_type";
    private const string InstallationIdKey = "installation_id";
    private const string ApplicationVersionKey = "app_version";
    private const string ApplicationTypeKey = "app_type";
    private const string TrafficTypeKey = "traffic_type";

    protected SharedAnalyticsPropertyProvider(
        IHostEnvironment hostEnvironment,
        IAppVersionProvider versionProvider,
        IUserPreferencesProvider userPreferencesProvider
    )
    {
        Properties[EnvironmentTypeKey] = hostEnvironment.EnvironmentName;
        Properties[ApplicationVersionKey] = versionProvider.CurrentVersion.ToNormalizedString();
        Properties[InstallationIdKey] = userPreferencesProvider.CurrentPreferences.InstallationId.ToString();
        Properties[TrafficTypeKey] = hostEnvironment.IsProduction() ? "public" : "internal";
    }

    public Guid GameSessionId { get; private set; }

    protected Dictionary<string, object> Properties { get; } = [];

    protected abstract string ApplicationType { get; }

    public virtual IEnumerable<KeyValuePair<string, object>> PropertyItems
        => Properties.Append(KeyValuePair.Create<string, object>(ApplicationTypeKey, ApplicationType));

    public void UseNewGameSessionId()
        => GameSessionId = Guid.NewGuid();
}
