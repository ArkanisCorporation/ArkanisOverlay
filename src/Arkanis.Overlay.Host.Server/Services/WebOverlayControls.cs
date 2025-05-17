namespace Arkanis.Overlay.Host.Server.Services;

using System.Globalization;
using Domain.Abstractions.Services;
using Domain.Options;
using MudBlazor;

internal sealed class WebOverlayControls : IOverlayControls, IOverlayEventProvider, IOverlayEventControls, IDisposable
{
    private readonly IUserPreferencesProvider _preferencesProvider;
    private readonly ISnackbar _snackbar;

    public WebOverlayControls(ISnackbar snackbar, IUserPreferencesProvider preferencesProvider)
    {
        _snackbar = snackbar;
        _preferencesProvider = preferencesProvider;
        preferencesProvider.ApplyPreferences += ApplyUserPreferencesAsync;
    }

    public void Dispose()
        => _preferencesProvider.ApplyPreferences -= ApplyUserPreferencesAsync;

    public ValueTask ShowAsync()
    {
        _snackbar.Add("This would open the overlay", configure: options => options.ShowCloseIcon = false);
        OverlayShown?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public ValueTask HideAsync()
    {
        _snackbar.Add("This would close the overlay", configure: options => options.ShowCloseIcon = false);
        OverlayHidden?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public void SetBlurEnabled(bool isEnabled)
    {
    }

    public void OnFocusGained()
        => OverlayFocused?.Invoke(this, EventArgs.Empty);

    public void OnFocusLost()
        => OverlayBlurred?.Invoke(this, EventArgs.Empty);

    public event EventHandler? OverlayShown;
    public event EventHandler? OverlayHidden;

    public event EventHandler? OverlayFocused;
    public event EventHandler? OverlayBlurred;

    private void ApplyUserPreferencesAsync(object? sender, UserPreferences userPreferences)
    {
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = userPreferences.ActiveCultureInfo;
        SetBlurEnabled(userPreferences.BlurBackground);
    }
}
