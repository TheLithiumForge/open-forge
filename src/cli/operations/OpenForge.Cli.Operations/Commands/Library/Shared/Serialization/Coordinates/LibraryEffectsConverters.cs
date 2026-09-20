using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Serialization.Coordinates;

internal sealed class LibraryPlanStateConverter : JsonConverter<LibraryPlanState>
{
    internal static string ReadWireValue(LibraryPlanState value)
        => value switch
        {
            LibraryPlanState.NotStarted => "not-started",
            LibraryPlanState.Complete => "complete",
            LibraryPlanState.Incomplete => "incomplete",
            LibraryPlanState.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryPlanState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryPlanState value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryRecordEffectConverter : JsonConverter<LibraryRecordEffect>
{
    internal static string ReadWireValue(LibraryRecordEffect value)
        => value switch
        {
            LibraryRecordEffect.None => "none",
            LibraryRecordEffect.Create => "create",
            LibraryRecordEffect.Replace => "replace",
            LibraryRecordEffect.Delete => "delete",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryRecordEffect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryRecordEffect value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryLinkEffectKindConverter : JsonConverter<LibraryLinkEffectKind>
{
    internal static string ReadWireValue(LibraryLinkEffectKind value)
        => value switch
        {
            LibraryLinkEffectKind.Create => "create",
            LibraryLinkEffectKind.Delete => "delete",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };

    public override LibraryLinkEffectKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");

    public override void Write(Utf8JsonWriter writer, LibraryLinkEffectKind value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}

internal sealed class LibraryExpectedStateKindConverter : JsonConverter<LibraryExpectedStateKind>
{
    internal static string ReadWireValue(LibraryExpectedStateKind value)
        => value switch
        {
            LibraryExpectedStateKind.Missing => "missing",
            LibraryExpectedStateKind.OrdinaryFile => "ordinary-file",
            LibraryExpectedStateKind.RelativeFileLink => "relative-file-link",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library value is not defined."),
        };
    public override LibraryExpectedStateKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Library result documents are output only.");
    public override void Write(Utf8JsonWriter writer, LibraryExpectedStateKind value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(ReadWireValue(value));
    }
}
