using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Rendering;

internal sealed class LibraryInspectDataFilesConverter : JsonConverter<LibraryInspectDataFiles>
{
    public override LibraryInspectDataFiles Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => throw new NotSupportedException("Library Inspect file data is write-only.");

    public override void Write(
        Utf8JsonWriter writer,
        LibraryInspectDataFiles value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        switch (value.Shape)
        {
            case LibraryInspectDataFilesShape.Differences:
            case LibraryInspectDataFilesShape.Paths:
                WriteRows(writer, value.Rows, includeTargets: false);
                return;
            case LibraryInspectDataFilesShape.Targets:
                WriteRows(writer, value.Rows, includeTargets: true);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value.Shape, "The Library Inspect file shape is not defined.");
        }
    }

    private static void WriteRows(
        Utf8JsonWriter writer,
        IReadOnlyList<LibraryInspectDataFile> rows,
        bool includeTargets)
    {
        writer.WriteStartArray();
        foreach (var row in rows)
        {
            writer.WriteStartObject();
            writer.WriteString("sourcePath", row.SourcePath);
            writer.WriteString("destinationPath", row.DestinationPath);
            writer.WriteString("relation", row.Relation);
            if (includeTargets)
            {
                writer.WriteString("expectedTarget", row.ExpectedTarget);
                writer.WriteString("observedTarget", row.ObservedTarget);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
