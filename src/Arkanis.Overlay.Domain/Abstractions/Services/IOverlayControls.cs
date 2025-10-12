namespace Arkanis.Overlay.Domain.Abstractions.Services;

/// <summary>
///     Controls the overlay, allowing it to be displayed, hidden, forced to be shown, and its blur effect to be enabled or disabled.
///     Also provides a way to shut down the overlay.
/// </summary>
public interface IOverlayControls
{
    /// <summary>
    ///     Used to open the overlay even when the game is not running or is out of focus.
    /// </summary>
    /// <returns>A task that completes when the overlay has been successfully forced to be shown.</returns>
    public Task ForceShowAsync();

    /// <summary>
    ///     Displays the overlay when the game is running and in focus.
    /// </summary>
    /// <returns>A value task that completes when the overlay has been successfully displayed.</returns>
    public ValueTask ShowAsync();

    /// <summary>
    ///     Hides the overlay if it is currently displayed.
    /// </summary>
    /// <returns>A value task that completes when the overlay has been successfully hidden.</returns>
    public ValueTask HideAsync();

    /// <summary>
    ///     Enables or disables the blur effect of the overlay.
    /// </summary>
    /// <param name="isEnabled">Provide <c>true</c> to enable the blur effect, <c>false</c> to disable it.</param>
    public void SetBlurEnabled(bool isEnabled);

    /// <summary>
    ///     Completely shuts down the Overlay.
    /// </summary>
    public void Shutdown();
}
