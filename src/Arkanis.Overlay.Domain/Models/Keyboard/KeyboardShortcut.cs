namespace Arkanis.Overlay.Domain.Models.Keyboard;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Humanizer;

[JsonConverter(typeof(JsonConverter))]
public sealed class KeyboardShortcut(IEnumerable<KeyboardKey> pressedKeys) : IEquatable<KeyboardShortcut>
{
    public static readonly KeyboardShortcut None = new([]);

    public HashSet<KeyboardKey> PressedKeys { get; } = pressedKeys
        .Distinct()
        .ToHashSet();

    public bool IsEmpty
        => PressedKeys.Count == 0;

    public bool IsValid
    {
        get
        {
            var categoryCounts = PressedKeys.Select(KeyboardKeyUtils.GetCategory).GroupBy(key => key).ToDictionary(x => x.Key, x => x.Count());
            var modifierKeyCount = categoryCounts.GetValueOrDefault(KeyboardKeyCategory.Modifier, 0);
            return IsEmpty
                   || modifierKeyCount >= 2
                   || (modifierKeyCount == 1 && categoryCounts.Count >= 2 && categoryCounts.Values.Sum() == 2);
        }
    }

    public string Description
        => new StringBuilder().AppendJoin(' ', FormatParts).ToString();

    public IEnumerable<FormatPart> FormatParts
        => PressedKeys.Count > 0
            ? PressedKeys.SelectMany<KeyboardKey, FormatPart>((pressedKey, index) =>
                {
                    var key = new Key(pressedKey.Humanize(LetterCasing.Title));
                    return index > 0
                        ? [Fill.Plus, key]
                        : [key];
                }
            )
            : [EmptyKey.Instance];

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

    public record FormatPart;

    public sealed record EmptyKey : FormatPart
    {
        public static readonly EmptyKey Instance = new();

        public override string ToString()
            => "None";
    }

    public sealed record Key(string Name) : FormatPart
    {
        public override string ToString()
            => Name;
    }

    public sealed record Fill(string Content) : FormatPart
    {
        public static readonly Fill Plus = new("+");

        public override string ToString()
            => Content;
    }

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
