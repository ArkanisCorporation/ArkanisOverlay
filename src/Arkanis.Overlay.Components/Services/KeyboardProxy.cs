namespace Arkanis.Overlay.Components.Services;

using Domain.Models.Keyboard;
using Helpers;

public sealed class KeyboardProxy : IDisposable
{
    private readonly KeyboardShortcutBuilder _shortcutBuilder;

    public KeyboardProxy()
        => _shortcutBuilder = new KeyboardShortcutBuilder(
            _ => Task.CompletedTask,
            _ =>
            {
                _shortcutBuilder?.Clear();
                return Task.CompletedTask;
            }
        )
        {
            FinalizationTimeout = TimeSpan.Zero,
        };

    public void Dispose()
        => _shortcutBuilder.Dispose();

    public event EventHandler<KeyboardKey>? OnKeyUp;
    public event EventHandler<KeyboardKey>? OnKeyDown;
    public event EventHandler<KeyboardShortcut>? OnKeyboardShortcut;

    public void RegisterKeyUp(KeyboardKey keyboardKey)
    {
        OnKeyUp?.Invoke(this, keyboardKey);
        StripShortcut(keyboardKey);
    }

    public void RegisterKeyDown(KeyboardKey keyboardKey)
    {
        OnKeyDown?.Invoke(this, keyboardKey);
        ExtendShortcut(keyboardKey);
    }

    private void RegisterKeyboardShortcut(KeyboardShortcut keyboardShortcut)
        => OnKeyboardShortcut?.Invoke(this, keyboardShortcut);

    private void ExtendShortcut(KeyboardKey keyboardKey)
    {
        _shortcutBuilder.AddKey(keyboardKey);
        RegisterKeyboardShortcut(_shortcutBuilder.Value.Copy());
    }

    private void StripShortcut(KeyboardKey keyboardKey)
        => _shortcutBuilder.RemoveKey(keyboardKey);
}
