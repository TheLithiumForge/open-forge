using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Context.Models;

namespace OpenForge.Cli.Core.Presentation.Context.Shared.Rendering;

internal sealed class ContextDataJsonConverter : JsonConverter<ContextData>
{
    public override ContextData Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => throw new NotSupportedException("Context presentation data is write-only.");

    public override void Write(
        Utf8JsonWriter writer,
        ContextData value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteStartObject();
        writer.WriteStartArray("sources");
        foreach (var source in value.Sources)
        {
            writer.WriteStartObject();
            writer.WriteString("path", source.Path);
            writer.WriteString("id", source.Id);
            writer.WriteString("layer", source.Layer);
            if (source.Standard is { } standard)
            {
                writer.WritePropertyName("includedBecause");
                writer.WriteStartArray();
                foreach (var reason in standard.IncludedBecause)
                {
                    writer.WriteStringValue(reason);
                }

                writer.WriteEndArray();
                writer.WriteString("route", standard.Route);
                writer.WriteString("scope", standard.Scope);
            }

            if (source.Full is { } full)
            {
                if (full.Order is { } order)
                {
                    writer.WriteNumber("order", order);
                }
                else
                {
                    writer.WriteNull("order");
                }
            }

            writer.WriteStartArray("parts");
            foreach (var part in source.Parts)
            {
                WritePart(writer, part);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        if (value.Links is { } links)
        {
            writer.WriteStartArray("links");
            foreach (var link in links)
            {
                writer.WriteStartObject();
                writer.WriteString("from", link.From);
                writer.WritePropertyName("location");
                WriteLocation(writer, link.Location);
                writer.WriteString("destination", link.Destination);
                writer.WriteString("resolvedPath", link.ResolvedPath);
                writer.WriteString("resolution", link.Resolution);
                writer.WriteBoolean("followed", link.Followed);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    private static void WritePart(Utf8JsonWriter writer, ContextDataPart part)
    {
        writer.WriteStartObject();
        writer.WriteString("part", part.Part);
        switch (part.Kind)
        {
            case ContextDataPartKind.Text:
            case ContextDataPartKind.Metadata:
                if (part.Text is not null)
                {
                    writer.WriteString("text", part.Text);
                }
                else if (part.State is not null)
                {
                    writer.WriteString("state", part.State);
                }

                break;
            case ContextDataPartKind.Headings:
                if (part.Headings is not null)
                {
                    writer.WriteStartArray("headings");
                    foreach (var heading in part.Headings)
                    {
                        writer.WriteStartObject();
                        writer.WriteString("text", heading.Text);
                        writer.WriteNumber("level", heading.Level);
                        if (heading.Line is { } line)
                        {
                            writer.WriteNumber("line", line);
                        }

                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();
                }
                else if (part.State is not null)
                {
                    writer.WriteString("state", part.State);
                }

                break;
            case ContextDataPartKind.Paths:
                if (part.Paths is not null)
                {
                    writer.WriteStartArray("paths");
                    foreach (var path in part.Paths)
                    {
                        writer.WriteStringValue(path);
                    }

                    writer.WriteEndArray();
                }
                else if (part.State is not null)
                {
                    writer.WriteString("state", part.State);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(part), part.Kind, "The Context data part kind is not defined.");
        }

        writer.WriteEndObject();
    }

    private static void WriteLocation(Utf8JsonWriter writer, ContextDataLocation location)
    {
        writer.WriteStartObject();
        writer.WriteNumber("line", location.Line);
        writer.WriteNumber("column", location.Column);
        writer.WriteEndObject();
    }
}
