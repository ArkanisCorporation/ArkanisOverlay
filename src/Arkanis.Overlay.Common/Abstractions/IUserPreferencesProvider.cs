namespace Arkanis.Overlay.Common.Abstractions;

using Options;

public interface IUserPreferencesProvider
{
    public UserPreferences CurrentPreferences { get; }

    public event EventHandler<UserPreferences> ApplyPreferences;
}
