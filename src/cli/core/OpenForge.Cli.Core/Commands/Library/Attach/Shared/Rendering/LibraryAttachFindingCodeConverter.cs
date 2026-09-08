using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Shared.Rendering;

internal sealed class LibraryAttachFindingCodeConverter : JsonConverter<LibraryAttachFindingCode>
{
    public override LibraryAttachFindingCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryAttachFindingCode value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(LibraryAttachDefinitions.ReadFindingCode(value));
    }
}
