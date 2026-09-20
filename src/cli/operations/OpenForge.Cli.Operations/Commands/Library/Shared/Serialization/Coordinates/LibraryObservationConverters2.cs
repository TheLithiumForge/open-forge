using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Serialization.Coordinates;

internal sealed class LibraryComparisonRelationConverter : JsonConverter<LibraryComparisonRelation>
{
    internal static string ReadWireValue(LibraryComparisonRelation value)
        => value switch
        {
            LibraryComparisonRelation.NotStarted => "not-started",
            LibraryComparisonRelation.Current => "current",
            LibraryComparisonRelation.Added => "added",
            LibraryComparisonRelation.Retired => "retired",
            LibraryComparisonRelation.Missing => "missing",
            LibraryComparisonRelation.Changed => "changed",
            LibraryComparisonRelation.Unavailable => "unavailable",
            LibraryComparisonRelation.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryComparisonRelation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryComparisonRelation value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryMutationRecordStateConverter : JsonConverter<LibraryMutationRecordState>
{
    internal static string ReadWireValue(LibraryMutationRecordState value)
        => value switch
        {
            LibraryMutationRecordState.NotStarted => "not-started",
            LibraryMutationRecordState.Missing => "missing",
            LibraryMutationRecordState.Complete => "complete",
            LibraryMutationRecordState.Invalid => "invalid",
            LibraryMutationRecordState.Unavailable => "unavailable",
            LibraryMutationRecordState.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryMutationRecordState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryMutationRecordState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryMutationInventoryStateConverter : JsonConverter<LibraryMutationInventoryState>
{
    internal static string ReadWireValue(LibraryMutationInventoryState value)
        => value switch
        {
            LibraryMutationInventoryState.NotStarted => "not-started",
            LibraryMutationInventoryState.Complete => "complete",
            LibraryMutationInventoryState.Incomplete => "incomplete",
            LibraryMutationInventoryState.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryMutationInventoryState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryMutationInventoryState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryCollisionKindConverter : JsonConverter<LibraryCollisionKind>
{
    internal static string ReadWireValue(LibraryCollisionKind value)
        => value switch
        {
            LibraryCollisionKind.Occupant => "occupant",
            LibraryCollisionKind.Parent => "parent",
            LibraryCollisionKind.Ownership => "ownership",
            LibraryCollisionKind.Duplicate => "duplicate",
            LibraryCollisionKind.Unsafe => "unsafe",
            LibraryCollisionKind.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryCollisionKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryCollisionKind value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}
