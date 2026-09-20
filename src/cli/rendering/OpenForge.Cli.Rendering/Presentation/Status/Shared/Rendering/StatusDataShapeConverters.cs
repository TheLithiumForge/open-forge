using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Status.Models;

namespace OpenForge.Cli.Core.Presentation.Status.Shared.Rendering;

internal sealed class StatusDataExtensionJsonConverter : JsonConverter<StatusDataExtension>
{
    public override StatusDataExtension Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => throw new NotSupportedException("Status data is write-only.");

    public override void Write(
        Utf8JsonWriter writer,
        StatusDataExtension value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("id", value.Id);
        writer.WriteString("version", value.Version);
        if (value.Files is { } counts)
        {
            writer.WriteStartObject("files");
            writer.WriteNumberOrNull("current", counts.Current);
            writer.WriteNumberOrNull("changed", counts.Changed);
            writer.WriteNumberOrNull("missing", counts.Missing);
            writer.WriteEndObject();
        }
        else if (value.FileRows is { } rows)
        {
            writer.WriteStartArray("files");
            foreach (var row in rows)
            {
                writer.WriteStartObject();
                writer.WriteString("path", row.Path);
                writer.WriteString("state", row.State);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}

internal sealed class StatusDataLibraryJsonConverter : JsonConverter<StatusDataLibrary>
{
    public override StatusDataLibrary Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => throw new NotSupportedException("Status data is write-only.");

    public override void Write(
        Utf8JsonWriter writer,
        StatusDataLibrary value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("id", value.Id);
        writer.WriteString("sourceRoot", value.SourceRoot);
        writer.WriteString("destinationRoot", value.DestinationRoot);
        if (value.Links is { } counts)
        {
            writer.WriteStartObject("links");
            writer.WriteNumberOrNull("current", counts.Current);
            writer.WriteNumberOrNull("missing", counts.Missing);
            writer.WriteNumberOrNull("changed", counts.Changed);
            writer.WriteEndObject();
        }
        else if (value.LinkRows is { } rows)
        {
            writer.WriteStartArray("links");
            foreach (var row in rows)
            {
                writer.WriteStartObject();
                writer.WriteString("path", row.Path);
                writer.WriteString("state", row.State);
                writer.WriteString("expectedTarget", row.ExpectedTarget);
                writer.WriteString("observedTarget", row.ObservedTarget);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}

internal static class StatusDataJsonWriterExtensions
{
    internal static void WriteNumberOrNull(this Utf8JsonWriter writer, string propertyName, long? value)
    {
        if (value is { } number)
            writer.WriteNumber(propertyName, number);
        else
            writer.WriteNull(propertyName);
    }
}
