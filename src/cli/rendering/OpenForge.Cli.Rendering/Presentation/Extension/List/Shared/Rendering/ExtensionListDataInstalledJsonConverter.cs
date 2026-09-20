using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Extension.List.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.List.Shared.Rendering;

internal sealed class ExtensionListDataInstalledJsonConverter : JsonConverter<ExtensionListDataInstalled>
{
    public override ExtensionListDataInstalled? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => throw new NotSupportedException("Extension List data is output-only.");

    public override void Write(
        Utf8JsonWriter writer,
        ExtensionListDataInstalled value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteStartObject();
        writer.WriteString("id", value.Id);
        if (value.Version is { } version)
        {
            writer.WriteString("version", version);
        }
        else
        {
            writer.WriteNull("version");
        }

        if (value.Note is { } note)
        {
            writer.WriteString("note", note);
        }
        else
        {
            writer.WriteNull("note");
        }

        if (value.Full is { } full)
        {
            writer.WriteNumber("files", full.Files);
            if (full.RecordedSource is { } recordedSource)
            {
                writer.WriteString("recordedSource", recordedSource);
            }
            else
            {
                writer.WriteNull("recordedSource");
            }

            writer.WriteString("coverage", full.Coverage);
        }

        writer.WriteEndObject();
    }
}
