namespace Arkanis.Overlay.Infrastructure.Services;

using Arkanis.Overlay.Common.Options;
using Arkanis.Overlay.Domain.Abstractions.Services;
using Common.Services;

public class UserPreferencesBasedOptionsChangeTokenSource<TOptions> : OptionsChangeTokenSourceProvider<TOptions>
{
    private readonly IUserPreferencesProvider _userPreferencesProvider;

    public UserPreferencesBasedOptionsChangeTokenSource(IUserPreferencesProvider userPreferencesProvider)
    {
        _userPreferencesProvider = userPreferencesProvider;
        _userPreferencesProvider.ApplyPreferences += OnUserPreferencesProviderOnApplyPreferences;
    }

    public override void Dispose()
    {
        _userPreferencesProvider.ApplyPreferences -= OnUserPreferencesProviderOnApplyPreferences;
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnUserPreferencesProviderOnApplyPreferences(object? _, UserPreferences userPreferences)
        => SignalChange();
}
