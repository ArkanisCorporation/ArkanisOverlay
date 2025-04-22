namespace Arkanis.Common.Converters.Json;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class RegionInfoJsonConverter : JsonConverter<RegionInfo>
{
    public override RegionInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var regionId = JsonSerializer.Deserialize<string>(ref reader, options);
        return regionId is not null
            ? new RegionInfo(regionId)
            : null;
    }

    public override void Write(Utf8JsonWriter writer, RegionInfo value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Name);
}
