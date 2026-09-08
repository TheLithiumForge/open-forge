using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Shared.Rendering;

internal sealed class LibrarySyncFindingCodeConverter : JsonConverter<LibrarySyncFindingCode>
{
    public override LibrarySyncFindingCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibrarySyncFindingCode value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(LibrarySyncDefinitions.ReadFindingCode(value));
    }
}
