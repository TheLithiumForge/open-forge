using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Library.List.Shared.Rendering;

internal sealed class LibraryListInventoryStateConverter : JsonConverter<LibraryListInventoryState>
{
    internal static string ReadWireValue(LibraryListInventoryState value)
        => value switch
        {
            LibraryListInventoryState.NotStarted => "not-started",
            LibraryListInventoryState.NotRequested => "not-requested",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryListInventoryState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryListInventoryState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}
