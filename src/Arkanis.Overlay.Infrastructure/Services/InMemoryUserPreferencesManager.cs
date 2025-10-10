namespace Arkanis.Overlay.Infrastructure.Services;

using Common.Options;
using Domain.Abstractions.Services;

public class InMemoryUserPreferencesManager : IUserPreferencesManager
{
    public UserPreferences CurrentPreferences { get; private set; } = new();

    public event EventHandler<UserPreferences>? ApplyPreferences;
    public event EventHandler<UserPreferences>? UpdatePreferences;

    public Task LoadUserPreferencesAsync()
        => Task.CompletedTask;

    public Task SaveAndApplyUserPreferencesAsync(UserPreferences userPreferences)
    {
        userPreferences.DisableAnalytics = false;

        UpdatePreferences?.Invoke(this, userPreferences);
        CurrentPreferences = userPreferences;
        ApplyPreferences?.Invoke(this, CurrentPreferences);
        return Task.CompletedTask;
    }
}
