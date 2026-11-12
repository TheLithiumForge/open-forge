using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Parsing;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Configuration;

// Host parsing is kept behind this seam while the provisional TOML dependency is reviewed.
internal static class ToolHostConfigReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static (JsonElement? Value, string? Cause) ReadMember(ToolMemberReceipt member, byte[] bytes)
    {
        try
        {
            if (member.Format == ToolVocabulary.JsonMembers)
            {
                if (!member.Selector.StartsWith('/')) return (null, "The JSON member selector is invalid.");
                using var document = JsonDocument.Parse(bytes);
                JsonDuplicatePropertyValidator.ValidateNoDuplicateProperties(bytes);
                var value = document.RootElement;
                foreach (var segment in member.Selector.Split('/').Skip(1))
                {
                    var key = segment.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);
                    if (value.ValueKind != JsonValueKind.Object || !value.TryGetProperty(key, out value)) return (null, null);
                }
                return (value.Clone(), null);
            }
            if (member.Format != ToolVocabulary.TomlTable) return (null, "The host format is unknown.");
            var text = StrictUtf8.GetString(bytes);
            _ = SyntaxParser.ParseStrict(text, validate: true);
            object? selected = TomlSerializer.Deserialize(text, ToolHostTomlContext.Default.TomlTable);
            foreach (var key in member.Selector.Split('.'))
                if (selected is not TomlTable table || !table.TryGetValue(key, out selected)) return (null, null);
            using var output = new MemoryStream();
            using (var writer = new Utf8JsonWriter(output)) WriteValue(writer, selected);
            using var parsed = JsonDocument.Parse(output.ToArray());
            return (parsed.RootElement.Clone(), null);
        }
        catch (Exception exception) when (exception is JsonException or TomlException or DecoderFallbackException or InvalidDataException)
        {
            return (null, "The host document or generated member is invalid.");
        }
    }

    private static void WriteValue(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case TomlTable table:
                writer.WriteStartObject();
                foreach (var (key, child) in table)
                {
                    writer.WritePropertyName(key);
                    WriteValue(writer, child);
                }
                writer.WriteEndObject();
                break;
            case TomlArray array:
                writer.WriteStartArray();
                foreach (var child in array) WriteValue(writer, child);
                writer.WriteEndArray();
                break;
            case string text: writer.WriteStringValue(text); break;
            case bool boolean: writer.WriteBooleanValue(boolean); break;
            case long integer: writer.WriteNumberValue(integer); break;
            case double number when double.IsFinite(number): writer.WriteNumberValue(number); break;
            default: throw new InvalidDataException("The generated MCP member contains an unsupported value.");
        }
    }
}
