using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Rendering;

internal sealed class ExtensionInspectDataJsonConverter : JsonConverter<ExtensionInspectData>
{
    public override ExtensionInspectData? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => throw new NotSupportedException("Extension Inspect data is output-only.");

    public override void Write(
        Utf8JsonWriter writer,
        ExtensionInspectData value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartObject();
        writer.WriteString("id", value.Id);
        WriteVersion(writer, "installed", value.Installed);
        WriteVersion(writer, "available", value.Available);
        writer.WritePropertyName("source");
        writer.WriteStartObject();
        WriteNullableString(writer, "kind", value.Source.Kind);
        WriteNullableString(writer, "path", value.Source.Path);
        writer.WriteEndObject();
        writer.WriteBoolean("matches", value.Matches);
        writer.WritePropertyName("files");
        writer.WriteStartArray();
        foreach (var file in value.Files)
        {
            writer.WriteStartObject();
            writer.WriteString("path", file.Path);
            writer.WriteString("relation", file.Relation);
            if (value.IncludeFull)
            {
                WriteNullableString(writer, "installedSha256", file.InstalledSha256);
                WriteNullableString(writer, "packageSha256", file.PackageSha256);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        if (value.IncludeStandard)
        {
            writer.WritePropertyName("dependencies");
            writer.WriteStartArray();
            foreach (var dependency in value.Dependencies)
            {
                writer.WriteStartObject();
                writer.WriteString("id", dependency.Id);
                WriteNullableString(writer, "version", dependency.Version);
                writer.WriteString("state", dependency.State);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            WriteNullableString(writer, "name", value.Name);
            WriteNullableString(writer, "description", value.Description);
        }

        if (value.IncludeFull)
        {
            WriteNullableString(writer, "manifestPath", value.ManifestPath);
            writer.WritePropertyName("resolutionOrder");
            writer.WriteStartArray();
            foreach (var id in value.ResolutionOrder)
            {
                writer.WriteStringValue(id);
            }

            writer.WriteEndArray();
            writer.WritePropertyName("registeredIn");
            writer.WriteStartArray();
            foreach (var path in value.RegisteredIn)
            {
                writer.WriteStringValue(path);
            }

            writer.WriteEndArray();
            WriteNullableString(writer, "recordCoverage", value.RecordCoverage);
        }

        writer.WriteEndObject();
    }

    private static void WriteVersion(
        Utf8JsonWriter writer,
        string name,
        ExtensionInspectDataVersion? version)
    {
        writer.WritePropertyName(name);
        if (version is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        WriteNullableString(writer, "version", version.Version);
        writer.WriteEndObject();
    }

    private static void WriteNullableString(
        Utf8JsonWriter writer,
        string name,
        string? value)
    {
        if (value is null)
        {
            writer.WriteNull(name);
        }
        else
        {
            writer.WriteString(name, value);
        }
    }
}
