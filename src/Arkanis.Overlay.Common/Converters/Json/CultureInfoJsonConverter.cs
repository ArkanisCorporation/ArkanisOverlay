namespace Arkanis.Common.Converters.Json;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CultureInfoJsonConverter : JsonConverter<CultureInfo>
{
    public override CultureInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var cultureId = JsonSerializer.Deserialize<string>(ref reader, options);
        return cultureId is not null
            ? new CultureInfo(cultureId)
            : null;
    }

    public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Name);
}
