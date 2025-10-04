namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Common.Options;

public interface IUserPreferencesManager : IUserPreferencesProvider
{
    public Task LoadUserPreferencesAsync();

    public Task SaveAndApplyUserPreferencesAsync(UserPreferences userPreferences);

    public event EventHandler<UserPreferences> UpdatePreferences;
}
