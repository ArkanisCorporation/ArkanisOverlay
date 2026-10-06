namespace Arkanis.Overlay.Common.Converters.Json;

using System.Text.Json;
using System.Text.Json.Serialization;
using Models;

public class UpdateChannelConverter : JsonConverter<UpdateChannel>
{
    public override UpdateChannel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var updateChannelId = JsonSerializer.Deserialize<string>(ref reader, options);
        return UpdateChannel.ById(updateChannelId);
    }

    public override void Write(Utf8JsonWriter writer, UpdateChannel value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.InternalId);
}
