using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Rendering;

internal sealed class LibraryInventoryViewStateConverter : JsonConverter<LibraryInventoryViewState>
{
    internal static string ReadWireValue(LibraryInventoryViewState value)
        => value switch
        {
            LibraryInventoryViewState.NotStarted => "not-started",
            LibraryInventoryViewState.Complete => "complete",
            LibraryInventoryViewState.Incomplete => "incomplete",
            LibraryInventoryViewState.Invalid => "invalid",
            LibraryInventoryViewState.Blocked => "blocked",
            LibraryInventoryViewState.Failed => "failed",
            LibraryInventoryViewState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryInventoryViewState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryInventoryViewState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}
