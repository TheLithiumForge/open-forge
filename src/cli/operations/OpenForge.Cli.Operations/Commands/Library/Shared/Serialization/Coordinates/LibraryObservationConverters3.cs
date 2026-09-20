using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Serialization.Coordinates;

internal sealed class LibraryExclusionKindConverter : JsonConverter<LibraryExclusionKind>
{
    internal static string ReadWireValue(LibraryExclusionKind value)
        => value switch
        {
            LibraryExclusionKind.Loader => "loader",
            LibraryExclusionKind.Entrypoint => "entrypoint",
            LibraryExclusionKind.Overwrite => "overwrite",
            LibraryExclusionKind.ManagerControl => "manager-control",
            LibraryExclusionKind.Link => "link",
            LibraryExclusionKind.ReparsePoint => "reparse-point",
            LibraryExclusionKind.Special => "special",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryExclusionKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryExclusionKind value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryOwnershipKindConverter : JsonConverter<LibraryOwnershipKind>
{
    internal static string ReadWireValue(LibraryOwnershipKind value)
        => value switch
        {
            LibraryOwnershipKind.Unowned => "unowned",
            LibraryOwnershipKind.Extension => "extension",
            LibraryOwnershipKind.Framework => "framework",
            LibraryOwnershipKind.Library => "library",
            LibraryOwnershipKind.Manager => "manager",
            LibraryOwnershipKind.Unavailable => "unavailable",
            LibraryOwnershipKind.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };
    public override LibraryOwnershipKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");
    public override void Write(Utf8JsonWriter writer, LibraryOwnershipKind value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}
