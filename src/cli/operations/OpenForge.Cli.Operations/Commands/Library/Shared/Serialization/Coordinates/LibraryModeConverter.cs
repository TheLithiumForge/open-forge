using OpenForge.Cli.Core.Commands.Library.Models.Request;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Serialization.Coordinates;

internal sealed class LibraryModeConverter : JsonConverter<LibraryMode>
{
    internal static string ReadWireValue(LibraryMode value)
        => value switch
        {
            LibraryMode.DryRun => "dry-run",
            LibraryMode.Apply => "apply",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryMode value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}
