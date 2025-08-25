namespace Arkanis.Overlay.Components.Services;

using Abstractions;
using Domain.Models.Keyboard;
using Extensions;
using Helpers;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class KeyboardProxy : IDisposable, IKeyboardProxy
{
    private readonly ILogger _logger;

    protected KeyboardProxy(ILogger logger)
    {
        _logger = logger;
        ShortcutBuilder = new KeyboardShortcutBuilder(
            _ => Task.CompletedTask,
            _ =>
            {
                ShortcutBuilder?.Clear();
                return Task.CompletedTask;
            }
        )
        {
            FinalizationTimeout = TimeSpan.Zero,
        };
    }

    public KeyboardProxy(ILogger<KeyboardProxy> logger) : this(logger as ILogger)
    {
    }

    public KeyboardProxy() : this(NullLogger<KeyboardProxy>.Instance)
    {
    }

    protected KeyboardShortcutBuilder ShortcutBuilder { get; }

    public virtual void Dispose()
    {
        ShortcutBuilder.Dispose();
        GC.SuppressFinalize(this);
    }

    public event EventHandler<KeyboardKey>? OnKeyUp;
    public event EventHandler<KeyboardKey>? OnKeyDown;
    public event EventHandler<KeyboardShortcut>? OnKeyboardShortcut;

    public void Clear()
        => ShortcutBuilder.Clear();

    public void RegisterKeyUp(KeyboardEventArgs keyboardEvent)
    {
        var keyboardKey = keyboardEvent.GetKey();
        OnKeyUp?.Invoke(this, keyboardKey);
        StripShortcut(keyboardEvent);
    }

    public void RegisterKeyDown(KeyboardEventArgs keyboardEvent)
    {
        var keyboardKey = keyboardEvent.GetKey();
        OnKeyDown?.Invoke(this, keyboardKey);
        ExtendShortcut(keyboardEvent);
    }

    private void RegisterKeyboardShortcut(KeyboardShortcut keyboardShortcut)
    {
        if (keyboardShortcut.PressedKeys.Count > 1)
        {
            _logger.LogDebug("KeyboardShortcut: {KeyboardShortcut}", keyboardShortcut.Description);
        }

        OnKeyboardShortcut?.Invoke(this, keyboardShortcut);
    }

    private void ExtendShortcut(KeyboardEventArgs keyboardEvent)
    {
        ShortcutBuilder.KeyPressed(keyboardEvent);
        RegisterKeyboardShortcut(ShortcutBuilder.Value.Copy());
    }

    private void StripShortcut(KeyboardEventArgs keyboardEvent)
        => ShortcutBuilder.KeyReleased(keyboardEvent);
}
