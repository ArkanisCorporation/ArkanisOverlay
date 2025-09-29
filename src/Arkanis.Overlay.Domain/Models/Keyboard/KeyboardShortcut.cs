namespace Arkanis.Overlay.Domain.Models.Keyboard;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Humanizer;
using MoreLinq;

[JsonConverter(typeof(JsonConverter))]
public sealed class KeyboardShortcut(IEnumerable<KeyboardKey> pressedKeys) : IEquatable<KeyboardShortcut>
{
    public static readonly KeyboardShortcut None = new([]);

    public HashSet<KeyboardKey> PressedKeys { get; } = pressedKeys
        .Distinct()
        .ToHashSet();

    public bool IsEmpty
        => PressedKeys.Count == 0;

    private bool HasStandaloneKey
        => PressedKeys.Intersect(KeyboardKeyUtils.GetKeys(KeyboardKeyCategory.ValidStandalone)).Any();

    private bool HasModifierKey
        => PressedKeys.Select(KeyboardKeyUtils.GetCategory).Any(category => category == KeyboardKeyCategory.Modifier);

    private bool HasNonModifierKey
        => PressedKeys.Select(KeyboardKeyUtils.GetCategory).Any(category => category != KeyboardKeyCategory.Modifier);

    public bool IsValid
        => IsEmpty
           || HasStandaloneKey
           || (HasModifierKey && HasNonModifierKey);

    public IEnumerable<string> KeyNames
        => PressedKeys.OrderBy(KeyboardKeyUtils.GetModifierSortOrder).Select(KeyboardKeyUtils.GetDisplayName).FallbackIfEmpty("None");

    public override string ToString()
        => string.Join(" + ", KeyNames);

    public string Description
        => ToString();

    public bool Equals(KeyboardShortcut? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || PressedKeys.SetEquals(other.PressedKeys);
    }

    public KeyboardShortcut Copy()
        => new(PressedKeys);

    public static implicit operator KeyboardShortcut(KeyboardKey key)
        => new([key]);

    public static KeyboardShortcut operator +(KeyboardShortcut shortcut, KeyboardKey key)
        => new(shortcut.PressedKeys.Append(key));

    public static KeyboardShortcut operator -(KeyboardShortcut shortcut, KeyboardKey key)
        => new(shortcut.PressedKeys.Where(pressedKey => pressedKey != key));

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj) || Equals(obj as KeyboardShortcut);
    }

    public override int GetHashCode()
        => PressedKeys.Aggregate(6428197, (code, key) => HashCode.Combine(code, key.GetHashCode()));

    public class JsonConverter : JsonConverter<KeyboardShortcut>
    {
        public override KeyboardShortcut Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var keys = JsonSerializer.Deserialize<KeyboardKey[]>(ref reader, options) ?? [];
            return new KeyboardShortcut(keys);
        }

        public override void Write(Utf8JsonWriter writer, KeyboardShortcut value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value.PressedKeys, options);
    }
}
