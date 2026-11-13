using System.Globalization;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

internal static class ToolDocumentDecoder
{
    private static readonly UTF8Encoding Utf8 = new(false, true);

    internal static byte[] JsonBytes(ReadOnlySpan<byte> bytes, string path)
    {
        var text = Utf8.GetString(bytes);
        if (Path.GetExtension(path).ToLowerInvariant() is not (".yaml" or ".yml"))
        {
            JsonDuplicatePropertyValidator.ValidateNoDuplicateProperties(bytes);
            return bytes.ToArray();
        }

        var events = new Parser(new StringReader(text));
        while (events.MoveNext())
        {
            if (events.Current is AnchorAlias || events.Current is NodeEvent node && !node.Tag.IsEmpty)
                throw new JsonException("Tool YAML cannot contain aliases or explicit tags.");
        }

        var document = new YamlDocumentParser().Parse(text);
        if (document.State != YamlDocumentState.Complete || document.Root is null || document.HasAliases || document.HasUnsupportedMappings)
            throw new JsonException("Tool YAML requires one mapping with distinct scalar keys.");
        var decoder = new StaticDeserializerBuilder(new FrameworkMetadataYamlContext())
            .WithAttemptingUnquotedStringTypeDeserialization().Build();
        using var output = new MemoryStream();
        using (var writer = new Utf8JsonWriter(output)) Write(writer, document.Root, text, decoder);
        return output.ToArray();
    }

    private static void Write(Utf8JsonWriter writer, YamlNode node, string source, IDeserializer decoder)
    {
        if (node.Mapping is { } mapping)
        {
            writer.WriteStartObject();
            foreach (var entry in mapping)
            {
                writer.WritePropertyName(entry.Key.Scalar?.Value ?? throw new JsonException("A tool YAML key must be a scalar."));
                Write(writer, entry.Value, source, decoder);
            }
            writer.WriteEndObject();
            return;
        }
        if (node.Sequence is { } sequence)
        {
            writer.WriteStartArray();
            foreach (var entry in sequence) Write(writer, entry, source, decoder);
            writer.WriteEndArray();
            return;
        }
        var scalar = node.Scalar ?? throw new JsonException("A tool YAML value must be a JSON-compatible value.");
        if (scalar.Style is not ScalarStyle.Plain)
        {
            writer.WriteStringValue(scalar.Value);
            return;
        }
        // YamlDotNet resolves plain scalar types. The projection only writes those typed results.
        var value = decoder.Deserialize<object>(source.Substring(scalar.Span.Start, scalar.Span.Length));
        switch (value)
        {
            case null: writer.WriteNullValue(); break;
            case string text: writer.WriteStringValue(text); break;
            case bool boolean: writer.WriteBooleanValue(boolean); break;
            case decimal number: writer.WriteNumberValue(number); break;
            case double number when double.IsFinite(number): writer.WriteNumberValue(number); break;
            case float number when float.IsFinite(number): writer.WriteNumberValue(number); break;
            case sbyte or byte or short or ushort or int or uint or long:
                writer.WriteNumberValue(Convert.ToInt64(value, CultureInfo.InvariantCulture)); break;
            case ulong number: writer.WriteNumberValue(number); break;
            default: throw new JsonException("Tool YAML scalar values must be finite JSON scalars.");
        }
    }
}
