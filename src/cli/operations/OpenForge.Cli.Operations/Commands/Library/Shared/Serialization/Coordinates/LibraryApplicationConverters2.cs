using OpenForge.Cli.Core.Commands.Library.Models.Application;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Serialization.Coordinates;

internal sealed class LibraryResidualStateConverter : JsonConverter<LibraryResidualState>
{
    internal static string ReadWireValue(LibraryResidualState value)
        => value switch
        {
            LibraryResidualState.Retained => "retained",
            LibraryResidualState.Unknown => "unknown",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryResidualState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryResidualState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryRecordPublicationStateConverter : JsonConverter<LibraryRecordPublicationState>
{
    internal static string ReadWireValue(LibraryRecordPublicationState value)
        => value switch
        {
            LibraryRecordPublicationState.NotStarted => "not-started",
            LibraryRecordPublicationState.Verified => "verified",
            LibraryRecordPublicationState.Failed => "failed",
            LibraryRecordPublicationState.Unknown => "unknown",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };
    public override LibraryRecordPublicationState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");
    public override void Write(Utf8JsonWriter writer, LibraryRecordPublicationState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}
