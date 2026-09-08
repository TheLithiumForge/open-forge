using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;

internal sealed class LibraryRecordViewStateConverter : JsonConverter<LibraryRecordViewState>
{
    internal static string ReadWireValue(LibraryRecordViewState value)
        => value switch
        {
            LibraryRecordViewState.NotStarted => "not-started",
            LibraryRecordViewState.Missing => "missing",
            LibraryRecordViewState.Complete => "complete",
            LibraryRecordViewState.Invalid => "invalid",
            LibraryRecordViewState.Unavailable => "unavailable",
            LibraryRecordViewState.Blocked => "blocked",
            LibraryRecordViewState.Failed => "failed",
            LibraryRecordViewState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryRecordViewState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryRecordViewState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibrarySourceRootViewStateConverter : JsonConverter<LibrarySourceRootViewState>
{
    internal static string ReadWireValue(LibrarySourceRootViewState value)
        => value switch
        {
            LibrarySourceRootViewState.NotStarted => "not-started",
            LibrarySourceRootViewState.Available => "available",
            LibrarySourceRootViewState.Missing => "missing",
            LibrarySourceRootViewState.Unavailable => "unavailable",
            LibrarySourceRootViewState.Invalid => "invalid",
            LibrarySourceRootViewState.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibrarySourceRootViewState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibrarySourceRootViewState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryLinkViewStateConverter : JsonConverter<LibraryLinkViewState>
{
    internal static string ReadWireValue(LibraryLinkViewState value)
        => value switch
        {
            LibraryLinkViewState.NotStarted => "not-started",
            LibraryLinkViewState.Current => "current",
            LibraryLinkViewState.Missing => "missing",
            LibraryLinkViewState.Changed => "changed",
            LibraryLinkViewState.Unavailable => "unavailable",
            LibraryLinkViewState.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryLinkViewState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryLinkViewState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryCoverageConverter : JsonConverter<LibraryCoverage>
{
    internal static string ReadWireValue(LibraryCoverage value)
        => value switch
        {
            LibraryCoverage.NotStarted => "not-started",
            LibraryCoverage.Complete => "complete",
            LibraryCoverage.Incomplete => "incomplete",
            LibraryCoverage.Blocked => "blocked",
            LibraryCoverage.Failed => "failed",
            LibraryCoverage.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryCoverage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryCoverage value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}
