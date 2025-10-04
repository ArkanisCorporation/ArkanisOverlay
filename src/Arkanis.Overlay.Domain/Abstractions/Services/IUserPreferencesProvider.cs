namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Common.Options;

public interface IUserPreferencesProvider
{
    public UserPreferences CurrentPreferences { get; }

    public event EventHandler<UserPreferences> ApplyPreferences;
}
