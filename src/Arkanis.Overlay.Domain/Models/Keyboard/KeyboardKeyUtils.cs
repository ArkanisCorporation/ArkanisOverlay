namespace Arkanis.Overlay.Domain.Models.Keyboard;

public static class KeyboardKeyUtils
{
    private static readonly HashSet<KeyboardKey> ModifierKeys =
    [
        KeyboardKey.ShiftLeft,
        KeyboardKey.ShiftRight,
        KeyboardKey.ControlLeft,
        KeyboardKey.ControlRight,
        KeyboardKey.AltLeft,
        KeyboardKey.AltRight,
        KeyboardKey.MetaLeft,
        KeyboardKey.MetaRight,
    ];

    private static readonly HashSet<KeyboardKey> LockKeys = [KeyboardKey.CapsLock, KeyboardKey.NumLock, KeyboardKey.ScrollLock];

    private static readonly HashSet<KeyboardKey> NavigationKeys =
    [
        KeyboardKey.ArrowLeft,
        KeyboardKey.ArrowUp,
        KeyboardKey.ArrowRight,
        KeyboardKey.ArrowDown,
        KeyboardKey.Home,
        KeyboardKey.End,
        KeyboardKey.PageUp,
        KeyboardKey.PageDown,
    ];

    private static readonly HashSet<KeyboardKey> EditingKeys =
    [
        KeyboardKey.Backspace, KeyboardKey.Delete, KeyboardKey.Insert, KeyboardKey.Enter, KeyboardKey.Tab, KeyboardKey.Escape,
    ];

    private static readonly HashSet<KeyboardKey> FunctionKeys =
    [
        KeyboardKey.F1,
        KeyboardKey.F2,
        KeyboardKey.F3,
        KeyboardKey.F4,
        KeyboardKey.F5,
        KeyboardKey.F6,
        KeyboardKey.F7,
        KeyboardKey.F8,
        KeyboardKey.F9,
        KeyboardKey.F10,
        KeyboardKey.F11,
        KeyboardKey.F12,
        KeyboardKey.F13,
        KeyboardKey.F14,
        KeyboardKey.F15,
        KeyboardKey.F16,
        KeyboardKey.F17,
        KeyboardKey.F18,
        KeyboardKey.F19,
        KeyboardKey.F20,
        KeyboardKey.F21,
        KeyboardKey.F22,
        KeyboardKey.F23,
        KeyboardKey.F24,
    ];

    private static readonly HashSet<KeyboardKey> AlphanumericKeys =
    [
        KeyboardKey.Digit0,
        KeyboardKey.Digit1,
        KeyboardKey.Digit2,
        KeyboardKey.Digit3,
        KeyboardKey.Digit4,
        KeyboardKey.Digit5,
        KeyboardKey.Digit6,
        KeyboardKey.Digit7,
        KeyboardKey.Digit8,
        KeyboardKey.Digit9,
        KeyboardKey.KeyA,
        KeyboardKey.KeyB,
        KeyboardKey.KeyC,
        KeyboardKey.KeyD,
        KeyboardKey.KeyE,
        KeyboardKey.KeyF,
        KeyboardKey.KeyG,
        KeyboardKey.KeyH,
        KeyboardKey.KeyI,
        KeyboardKey.KeyJ,
        KeyboardKey.KeyK,
        KeyboardKey.KeyL,
        KeyboardKey.KeyM,
        KeyboardKey.KeyN,
        KeyboardKey.KeyO,
        KeyboardKey.KeyP,
        KeyboardKey.KeyQ,
        KeyboardKey.KeyR,
        KeyboardKey.KeyS,
        KeyboardKey.KeyT,
        KeyboardKey.KeyU,
        KeyboardKey.KeyV,
        KeyboardKey.KeyW,
        KeyboardKey.KeyX,
        KeyboardKey.KeyY,
        KeyboardKey.KeyZ,
    ];

    private static readonly HashSet<KeyboardKey> NumpadKeys =
    [
        KeyboardKey.Numpad0,
        KeyboardKey.Numpad1,
        KeyboardKey.Numpad2,
        KeyboardKey.Numpad3,
        KeyboardKey.Numpad4,
        KeyboardKey.Numpad5,
        KeyboardKey.Numpad6,
        KeyboardKey.Numpad7,
        KeyboardKey.Numpad8,
        KeyboardKey.Numpad9,
        KeyboardKey.NumpadMultiply,
        KeyboardKey.NumpadAdd,
        KeyboardKey.NumpadSubtract,
        KeyboardKey.NumpadDecimal,
        KeyboardKey.NumpadDivide,
    ];

    private static readonly HashSet<KeyboardKey> SymbolKeys =
    [
        KeyboardKey.Semicolon,
        KeyboardKey.Equal,
        KeyboardKey.Comma,
        KeyboardKey.Minus,
        KeyboardKey.Period,
        KeyboardKey.Slash,
        KeyboardKey.Backquote,
        KeyboardKey.BracketLeft,
        KeyboardKey.Backslash,
        KeyboardKey.BracketRight,
        KeyboardKey.Quote,
        KeyboardKey.IntBackslash,
    ];

    private static readonly HashSet<KeyboardKey> MediaKeys =
    [
        KeyboardKey.PrintScreen,
        KeyboardKey.Pause,
        KeyboardKey.ContextMenu,
        KeyboardKey.AudioVolumeMute,
        KeyboardKey.AudioVolumeDown,
        KeyboardKey.AudioVolumeUp,
        KeyboardKey.LaunchMediaPlayer,
        KeyboardKey.LaunchApplication1,
        KeyboardKey.LaunchApplication2,
    ];

    private static readonly HashSet<KeyboardKey> WhitespaceKeys = [KeyboardKey.Space];

    private static readonly HashSet<KeyboardKey> OtherKeys = [KeyboardKey.Unknown];

    private static readonly Dictionary<KeyboardKey, KeyboardKeyCategory> KeyToCategory = new()
    {
        // Modifier Keys
        [KeyboardKey.ShiftLeft] = KeyboardKeyCategory.Modifier,
        [KeyboardKey.ShiftRight] = KeyboardKeyCategory.Modifier,
        [KeyboardKey.ControlLeft] = KeyboardKeyCategory.Modifier,
        [KeyboardKey.ControlRight] = KeyboardKeyCategory.Modifier,
        [KeyboardKey.AltLeft] = KeyboardKeyCategory.Modifier,
        [KeyboardKey.AltRight] = KeyboardKeyCategory.Modifier,
        [KeyboardKey.MetaLeft] = KeyboardKeyCategory.Modifier,
        [KeyboardKey.MetaRight] = KeyboardKeyCategory.Modifier,

        // Lock Keys
        [KeyboardKey.CapsLock] = KeyboardKeyCategory.Lock,
        [KeyboardKey.NumLock] = KeyboardKeyCategory.Lock,
        [KeyboardKey.ScrollLock] = KeyboardKeyCategory.Lock,

        // Navigation Keys
        [KeyboardKey.ArrowLeft] = KeyboardKeyCategory.Navigation,
        [KeyboardKey.ArrowUp] = KeyboardKeyCategory.Navigation,
        [KeyboardKey.ArrowRight] = KeyboardKeyCategory.Navigation,
        [KeyboardKey.ArrowDown] = KeyboardKeyCategory.Navigation,
        [KeyboardKey.Home] = KeyboardKeyCategory.Navigation,
        [KeyboardKey.End] = KeyboardKeyCategory.Navigation,
        [KeyboardKey.PageUp] = KeyboardKeyCategory.Navigation,
        [KeyboardKey.PageDown] = KeyboardKeyCategory.Navigation,

        // Editing Keys
        [KeyboardKey.Backspace] = KeyboardKeyCategory.Editing,
        [KeyboardKey.Delete] = KeyboardKeyCategory.Editing,
        [KeyboardKey.Insert] = KeyboardKeyCategory.Editing,
        [KeyboardKey.Enter] = KeyboardKeyCategory.Editing,
        [KeyboardKey.Tab] = KeyboardKeyCategory.Editing,
        [KeyboardKey.Escape] = KeyboardKeyCategory.Editing,

        // Function Keys
        [KeyboardKey.F1] = KeyboardKeyCategory.Function,
        [KeyboardKey.F2] = KeyboardKeyCategory.Function,
        [KeyboardKey.F3] = KeyboardKeyCategory.Function,
        [KeyboardKey.F4] = KeyboardKeyCategory.Function,
        [KeyboardKey.F5] = KeyboardKeyCategory.Function,
        [KeyboardKey.F6] = KeyboardKeyCategory.Function,
        [KeyboardKey.F7] = KeyboardKeyCategory.Function,
        [KeyboardKey.F8] = KeyboardKeyCategory.Function,
        [KeyboardKey.F9] = KeyboardKeyCategory.Function,
        [KeyboardKey.F10] = KeyboardKeyCategory.Function,
        [KeyboardKey.F11] = KeyboardKeyCategory.Function,
        [KeyboardKey.F12] = KeyboardKeyCategory.Function,

        // Alphanumeric Keys
        [KeyboardKey.Digit0] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.Digit1] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.Digit2] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.Digit3] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.Digit4] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.Digit5] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.Digit6] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.Digit7] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.Digit8] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.Digit9] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyA] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyB] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyC] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyD] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyE] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyF] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyG] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyH] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyI] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyJ] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyK] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyL] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyM] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyN] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyO] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyP] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyQ] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyR] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyS] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyT] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyU] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyV] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyW] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyX] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyY] = KeyboardKeyCategory.Alphanumeric,
        [KeyboardKey.KeyZ] = KeyboardKeyCategory.Alphanumeric,

        // Numpad Keys
        [KeyboardKey.Numpad0] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.Numpad1] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.Numpad2] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.Numpad3] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.Numpad4] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.Numpad5] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.Numpad6] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.Numpad7] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.Numpad8] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.Numpad9] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.NumpadMultiply] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.NumpadAdd] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.NumpadSubtract] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.NumpadDecimal] = KeyboardKeyCategory.Numpad,
        [KeyboardKey.NumpadDivide] = KeyboardKeyCategory.Numpad,

        // Symbol & Punctuation Keys
        [KeyboardKey.Semicolon] = KeyboardKeyCategory.Symbol,
        [KeyboardKey.Equal] = KeyboardKeyCategory.Symbol,
        [KeyboardKey.Comma] = KeyboardKeyCategory.Symbol,
        [KeyboardKey.Minus] = KeyboardKeyCategory.Symbol,
        [KeyboardKey.Period] = KeyboardKeyCategory.Symbol,
        [KeyboardKey.Slash] = KeyboardKeyCategory.Symbol,
        [KeyboardKey.Backquote] = KeyboardKeyCategory.Symbol,
        [KeyboardKey.BracketLeft] = KeyboardKeyCategory.Symbol,
        [KeyboardKey.Backslash] = KeyboardKeyCategory.Symbol,
        [KeyboardKey.BracketRight] = KeyboardKeyCategory.Symbol,
        [KeyboardKey.Quote] = KeyboardKeyCategory.Symbol,
        [KeyboardKey.IntBackslash] = KeyboardKeyCategory.Symbol,

        // Media/System Keys
        [KeyboardKey.PrintScreen] = KeyboardKeyCategory.Media,
        [KeyboardKey.Pause] = KeyboardKeyCategory.Media,
        [KeyboardKey.ContextMenu] = KeyboardKeyCategory.Media,
        [KeyboardKey.AudioVolumeMute] = KeyboardKeyCategory.Media,
        [KeyboardKey.AudioVolumeDown] = KeyboardKeyCategory.Media,
        [KeyboardKey.AudioVolumeUp] = KeyboardKeyCategory.Media,
        [KeyboardKey.LaunchMediaPlayer] = KeyboardKeyCategory.Media,
        [KeyboardKey.LaunchApplication1] = KeyboardKeyCategory.Media,
        [KeyboardKey.LaunchApplication2] = KeyboardKeyCategory.Media,

        // Whitespace & Misc
        [KeyboardKey.Space] = KeyboardKeyCategory.Whitespace,

        // Unknown
        [KeyboardKey.Unknown] = KeyboardKeyCategory.Unknown,
    };

    private static HashSet<KeyboardKey> GetKeys(KeyboardKeyCategory category)
        => category switch
        {
            KeyboardKeyCategory.Unknown => OtherKeys,
            KeyboardKeyCategory.Modifier => ModifierKeys,
            KeyboardKeyCategory.Lock => LockKeys,
            KeyboardKeyCategory.Navigation => NavigationKeys,
            KeyboardKeyCategory.Editing => EditingKeys,
            KeyboardKeyCategory.Function => FunctionKeys, // standalone
            KeyboardKeyCategory.Alphanumeric => AlphanumericKeys,
            KeyboardKeyCategory.Numpad => NumpadKeys,
            KeyboardKeyCategory.Symbol => SymbolKeys,
            KeyboardKeyCategory.Media => MediaKeys, // standalone
            KeyboardKeyCategory.Whitespace => WhitespaceKeys,
            _ => [],
        };

    public static KeyboardKeyCategory GetCategory(KeyboardKey key)
        => KeyToCategory.GetValueOrDefault(key, KeyboardKeyCategory.Unknown);

    public static bool IsKeyInCategory(KeyboardKey key, KeyboardKeyCategory category)
        => GetKeys(category).Contains(key);

    public static bool IsModifierKey(KeyboardKey key)
        => IsKeyInCategory(key, KeyboardKeyCategory.Modifier);

    public static bool IsPrintableKey(KeyboardKey key)
        => IsKeyInCategory(key, KeyboardKeyCategory.Alphanumeric)
           || IsKeyInCategory(key, KeyboardKeyCategory.Numpad)
           || IsKeyInCategory(key, KeyboardKeyCategory.Symbol);
}
