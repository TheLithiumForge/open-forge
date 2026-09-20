using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Library.List.Models;

namespace OpenForge.Cli.Core.Presentation.Library.List.Shared.Rendering;

internal sealed class LibraryListDataLinksConverter : JsonConverter<LibraryListDataLinks>
{
    public override LibraryListDataLinks Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => throw new NotSupportedException("Library List link data is write-only.");

    public override void Write(
        Utf8JsonWriter writer,
        LibraryListDataLinks value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        switch (value.Shape)
        {
            case LibraryListDataLinksShape.Counts:
                writer.WriteStartObject();
                writer.WriteNumber("current", value.Counts.Current);
                writer.WriteNumber("missing", value.Counts.Missing);
                writer.WriteNumber("changed", value.Counts.Changed);
                writer.WriteNumber("unavailable", value.Counts.Unavailable);
                writer.WriteEndObject();
                return;
            case LibraryListDataLinksShape.Paths:
                WriteRows(writer, value.Rows, includeTargets: false);
                return;
            case LibraryListDataLinksShape.Targets:
                WriteRows(writer, value.Rows, includeTargets: true);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value.Shape, "The Library List link shape is not defined.");
        }
    }

    private static void WriteRows(
        Utf8JsonWriter writer,
        IReadOnlyList<LibraryListDataLink> rows,
        bool includeTargets)
    {
        writer.WriteStartArray();
        foreach (var row in rows)
        {
            writer.WriteStartObject();
            writer.WriteString("path", row.Path);
            writer.WriteString("state", row.State);
            if (includeTargets)
            {
                writer.WriteString("expectedTarget", row.ExpectedTarget);
                writer.WriteString("observedTarget", row.ObservedTarget);
                writer.WriteString("sourceId", row.SourceId);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
