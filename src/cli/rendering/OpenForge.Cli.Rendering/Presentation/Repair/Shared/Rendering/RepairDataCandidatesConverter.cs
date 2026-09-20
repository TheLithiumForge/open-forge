using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Repair.Models;

namespace OpenForge.Cli.Core.Presentation.Repair.Shared.Rendering;

internal sealed class RepairDataCandidatesConverter : JsonConverter<RepairDataCandidates>
{
    public override RepairDataCandidates Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => throw new NotSupportedException("Repair candidate data is write-only.");

    public override void Write(
        Utf8JsonWriter writer,
        RepairDataCandidates value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        switch (value.Shape)
        {
            case RepairDataCandidatesShape.Count:
                writer.WriteNumberValue(value.ItemCount);
                return;
            case RepairDataCandidatesShape.Paths:
                WriteItems(writer, value.Items, includeReasons: false);
                return;
            case RepairDataCandidatesShape.Reasons:
                WriteItems(writer, value.Items, includeReasons: true);
                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value.Shape,
                    "The Repair candidate data shape is not defined.");
        }
    }

    private static void WriteItems(
        Utf8JsonWriter writer,
        IReadOnlyList<RepairDataCandidate> items,
        bool includeReasons)
    {
        writer.WriteStartArray();
        foreach (var item in items)
        {
            writer.WriteStartObject();
            writer.WriteString("path", item.Path);
            if (includeReasons)
            {
                writer.WriteStartArray("reasons");
                foreach (var reason in item.Reasons ?? [])
                    writer.WriteStringValue(reason);
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
