using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;

namespace OpenForge.Cli.Core.Commands.Library.List.Shared.Serialization;

internal sealed class LibraryListFindingCodeConverter : JsonConverter<LibraryListFindingCode>
{
    public override LibraryListFindingCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryListFindingCode value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(LibraryListDefinitions.ReadFindingCode(value));
    }
}
