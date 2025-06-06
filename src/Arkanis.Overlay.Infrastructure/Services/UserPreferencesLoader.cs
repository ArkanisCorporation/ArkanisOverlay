namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions.Services;
using Microsoft.Extensions.Hosting;

internal class UserPreferencesLoader(IUserPreferencesManager preferencesManager) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
        => preferencesManager.LoadUserPreferencesAsync();

    public Task StopAsync(CancellationToken cancellationToken)
        => preferencesManager.SaveAndApplyUserPreferencesAsync(preferencesManager.CurrentPreferences);
}
