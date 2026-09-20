using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Shared.Serialization;

internal sealed class LibraryDetachFindingCodeConverter : JsonConverter<LibraryDetachFindingCode>
{
    public override LibraryDetachFindingCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryDetachFindingCode value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(LibraryDetachDefinitions.ReadFindingCode(value));
    }
}
