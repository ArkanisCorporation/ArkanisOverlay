namespace Arkanis.Overlay.Components.Helpers;

using Domain.Models.Keyboard;
using Extensions;
using Microsoft.AspNetCore.Components.Web;

public sealed class KeyboardShortcutBuilder : IDisposable
{
    private readonly Func<KeyboardShortcut, Task> _completionCallback;
    private readonly Timer? _finalizeTimer;

    private readonly List<KeyboardKey> _releasedKeys = [];
    private readonly Func<KeyboardShortcut, Task> _timeoutCallback;

    public KeyboardShortcutBuilder(Func<KeyboardShortcut, Task> timeoutCallback, Func<KeyboardShortcut, Task> completionCallback)
    {
        _timeoutCallback = timeoutCallback;
        _completionCallback = completionCallback;
        _finalizeTimer = new Timer(FinalizeAsync, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public bool Finalized { get; private set; } = true;

    public KeyboardShortcut Value { get; private set; } = KeyboardShortcut.None;

    public TimeSpan FinalizationTimeout { get; set; } = TimeSpan.Zero;

    public void Dispose()
        => _finalizeTimer?.Dispose();

    private bool AreAnyKeysPressed()
    {
        lock (_releasedKeys)
        {
            // keys that have been pressed have all been released
            return Value.PressedKeys.SetEquals(_releasedKeys) == false;
        }
    }

    private void ClearReleasedKeysFromKeyPress()
    {
        lock (_releasedKeys)
        {
            _releasedKeys.ForEach(key => Value -= key);
            _releasedKeys.Clear();
        }
    }

    public void Clear()
    {
        lock (_releasedKeys)
        {
            _releasedKeys.Clear();
        }

        Value = KeyboardShortcut.None;
    }

    public void KeyPressed(KeyboardEventArgs eventArgs)
    {
        var keyboardKey = eventArgs.GetKey();
        AddKey(keyboardKey);

        if ((eventArgs.ShiftKey || eventArgs.MetaKey) && !KeyboardKeyUtils.IsKeyInCategory(keyboardKey, KeyboardKeyCategory.Modifier))
        {
            // WORKAROUND:
            // key release event is not reported for keys pressed together with a shift/meta modifier key
            // by marking the key as released, we can ensure that the shortcut is properly finalized
            // RemoveKey(keyboardKey);
        }
    }

    public void AddKey(KeyboardKey keyboardKey)
    {
        // a key has been pressed
        if (Finalized)
        {
            // the shortcut has been previously finalized, start from scratch
            Value = KeyboardShortcut.None;
            Finalized = false;
        }

        lock (_releasedKeys)
        {
            // remove any released keys from the shortcut
            ClearReleasedKeysFromKeyPress();

            Value += keyboardKey;
        }
    }

    public void KeyReleased(KeyboardEventArgs eventArgs)
    {
        var keyboardKey = eventArgs.GetKey();
        RemoveKey(keyboardKey);
    }

    public void RemoveKey(KeyboardKey keyboardKey)
    {
        if (KeyboardKeyUtils.IsKeyInCategory(keyboardKey, KeyboardKeyCategory.Modifier))
        {
            // a key has been released
            lock (_releasedKeys)
            {
                _releasedKeys.Add(keyboardKey);
            }

            ClearReleasedKeysFromKeyPress();
            return;
        }

        if (FinalizationTimeout == TimeSpan.Zero)
        {
            FinalizeAsync(null);
            return;
        }

        // start a timer to remove the key
        //   or to finalize the shortcut
        _finalizeTimer?.Change(FinalizationTimeout, Timeout.InfiniteTimeSpan);
    }

    private async void FinalizeAsync(object? state)
    {
        // input timeout has expired
        if (AreAnyKeysPressed())
        {
            ClearReleasedKeysFromKeyPress();
            await _timeoutCallback.Invoke(Value);
        }

        lock (_releasedKeys)
        {
            _releasedKeys.Clear();
            Finalized = true;
        }

        await _completionCallback.Invoke(Value);
    }
}
