namespace Arkanis.Overlay.UI.Models;

using System.Diagnostics;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Media;

[DebuggerDisplay("ActionEntryBase")]
public abstract class AActionEntry;

[DebuggerDisplay("Separator")]
public sealed class SeparatorActionEntry : AActionEntry;

[DebuggerDisplay("ActionEntry {Name}")]
public abstract class ActionEntry : AActionEntry
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public Color Color { get; init; }

    public virtual bool IsEnabled
        => !IsInDevelopment;

    public bool IsInDevelopment { get; init; }

    public Icon? Icon { get; init; }

    public KeyGesture? Shortcut { get; set; }
}

[DebuggerDisplay("CommandActionEntry {Name}")]
public sealed class CommandActionEntry : ActionEntry
{
    public ICommand? Command { get; init; }
}
