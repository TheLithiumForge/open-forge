using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Rendering;

internal sealed class LibraryInspectFindingCodeConverter : JsonConverter<LibraryInspectFindingCode>
{
    public override LibraryInspectFindingCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryInspectFindingCode value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(LibraryInspectDefinitions.ReadFindingCode(value));
    }
}
