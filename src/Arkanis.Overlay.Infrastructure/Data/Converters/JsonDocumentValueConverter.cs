namespace Arkanis.Overlay.Infrastructure.Data.Converters;

using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public sealed class JsonDocumentValueConverter() : ValueConverter<JsonDocument, string>(
    document => ToJson(document),
    json => JsonDocument.Parse(json, DocumentOptions)
)
{
    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Indented = true,
    };

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
        MaxDepth = 64,
    };

    private static string ToJson(JsonDocument document)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
        {
            document.WriteTo(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
