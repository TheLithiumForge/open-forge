using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Serialization.Coordinates;

internal sealed class LibrarySemanticStatusConverter : JsonConverter<CliSemanticStatus>
{
    public override CliSemanticStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, CliSemanticStatus value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(CliStatusDefinitions.Read(value).MachineName);
    }
}
