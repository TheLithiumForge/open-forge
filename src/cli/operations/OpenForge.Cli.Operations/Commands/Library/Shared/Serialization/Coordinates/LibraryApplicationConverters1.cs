using OpenForge.Cli.Core.Commands.Library.Models.Application;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Serialization.Coordinates;

internal sealed class LibraryApplicationStateConverter : JsonConverter<LibraryApplicationState>
{
    internal static string ReadWireValue(LibraryApplicationState value)
        => value switch
        {
            LibraryApplicationState.NotStarted => "not-started",
            LibraryApplicationState.NoOp => "no-op",
            LibraryApplicationState.Applied => "applied",
            LibraryApplicationState.Failed => "failed",
            LibraryApplicationState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryApplicationState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryApplicationState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryVerificationStateConverter : JsonConverter<LibraryVerificationState>
{
    internal static string ReadWireValue(LibraryVerificationState value)
        => value switch
        {
            LibraryVerificationState.NotStarted => "not-started",
            LibraryVerificationState.Verified => "verified",
            LibraryVerificationState.Failed => "failed",
            LibraryVerificationState.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryVerificationState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryVerificationState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryRecoveryStateConverter : JsonConverter<LibraryRecoveryState>
{
    internal static string ReadWireValue(LibraryRecoveryState value)
        => value switch
        {
            LibraryRecoveryState.NotRequested => "not-requested",
            LibraryRecoveryState.Prepared => "prepared",
            LibraryRecoveryState.Removed => "removed",
            LibraryRecoveryState.Retained => "retained",
            LibraryRecoveryState.Unknown => "unknown",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryRecoveryState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryRecoveryState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryResidualKindConverter : JsonConverter<LibraryResidualKind>
{
    internal static string ReadWireValue(LibraryResidualKind value)
        => value switch
        {
            LibraryResidualKind.Directory => "directory",
            LibraryResidualKind.Link => "link",
            LibraryResidualKind.GeneratedRegion => "generated-region",
            LibraryResidualKind.Record => "record",
            LibraryResidualKind.Recovery => "recovery",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryResidualKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryResidualKind value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}
