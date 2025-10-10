namespace Arkanis.Overlay.Host.Server.Services;

using System.Globalization;
using Common.Options;
using Domain.Abstractions.Services;
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

    public async Task ForceShowAsync()
        => await ShowAsync();

    public ValueTask ShowAsync()
    {
        _snackbar.Add(
            "This would open the overlay",
            configure: options =>
            {
                options.ShowCloseIcon = false;
            }
        );
        OverlayShown?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public ValueTask HideAsync()
    {
        _snackbar.Add(
            "This would close the overlay",
            configure: options =>
            {
                options.ShowCloseIcon = false;
            }
        );
        OverlayHidden?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public void Shutdown()
        => _snackbar.Add(
            "This would exit the overlay",
            configure: options =>
            {
                options.ShowCloseIcon = false;
            }
        );

    public void SetBlurEnabled(bool isEnabled)
    {
    }

    public void OnFocusGained()
        => OverlayFocused?.Invoke(this, EventArgs.Empty);

    public void OnFocusLost()
        => OverlayBlurred?.Invoke(this, EventArgs.Empty);

    public void OnGameWindowFound()
        => GameWindowFound?.Invoke(this, EventArgs.Empty);

    public void OnGameWindowLost()
        => GameWindowLost?.Invoke(this, EventArgs.Empty);

    public void OnGameWindowFocused()
        => GameWindowFocused?.Invoke(this, EventArgs.Empty);

    public void OnGameWindowBlurred()
        => GameWindowBlurred?.Invoke(this, EventArgs.Empty);

    public void OnOverlayWindowShown()
        => OverlayShown?.Invoke(this, EventArgs.Empty);

    public void OnOverlayWindowHidden()
        => OverlayHidden?.Invoke(this, EventArgs.Empty);

    public event EventHandler? OverlayShown;
    public event EventHandler? OverlayHidden;

    public event EventHandler? OverlayFocused;
    public event EventHandler? OverlayBlurred;
    public event EventHandler? GameWindowFocused;
    public event EventHandler? GameWindowBlurred;
    public event EventHandler? GameWindowFound;
    public event EventHandler? GameWindowLost;

    private void ApplyUserPreferencesAsync(object? sender, UserPreferences userPreferences)
    {
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = userPreferences.ActiveCultureInfo;
        SetBlurEnabled(userPreferences.BlurBackground);
    }
}
