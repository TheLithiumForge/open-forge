using System.Text;
using OpenForge.Cli.Core.Commands.Status.Models.Presentation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusLibraryPresentation
{
    internal static StatusJsonLibrary Project(StatusLibrary library)
    {
        Validate(library);
        return new StatusJsonLibrary
        {
            State = Status(library),
            Record = new StatusJsonLibraryRecord(
                ".agents/open-forge.libraries.json",
                RecordState(library.Observation.Record.State)),
            Records = [.. library.Records.Select(record => new StatusJsonLibraryRegistration
            {
                Id = record.Id.Value,
                SourceRoot = record.SourceRoot.Value,
                SourceRootState = SourceState(record.SourceRootState),
                SourceAvailability = StatusWireVocabulary.SourceAvailability(record.SourceAvailability),
                RegisteredLinks = new StatusJsonLibraryRegisteredLinks
                {
                    Registered = Integer(record.Registered),
                    Counts = LinkCounts(record.Counts),
                    Links = [.. record.Links.Select(link => new StatusJsonLibraryLink(
                        link.Observation.Mapping.SourcePath.Value,
                        link.Observation.Mapping.DestinationPath.Value,
                        link.Observation.Mapping.ExpectedRelativeLink.Value,
                        link.SourceId,
                        LinkState(link.Observation.State)))],
                },
            })],
            Counts = Counts(library.Counts),
        };
    }

    internal static void Append(StringBuilder builder, StatusLibrary library)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Validate(library);
        builder.AppendLine("Libraries");
        builder.AppendLine($"  State: {Status(library)}");
        builder.AppendLine($"  Record: {RecordState(library.Observation.Record.State)} (.agents/open-forge.libraries.json)");
        builder.AppendLine($"  Counts: {CountsText(library.Counts, includeRegistered: true)}");
        if (library.Records.Length == 0)
        {
            builder.AppendLine(library.Observation.Record.State is LibrariesRecordReadState.Missing or LibrariesRecordReadState.Complete
                ? "  none"
                : "  Records: unavailable");
            return;
        }

        foreach (var record in library.Records)
        {
            builder.AppendLine($"  {Text(record.Id.Value)} ({Text(record.SourceRoot.Value)}): "
                + $"source-root={SourceState(record.SourceRootState)}; "
                + $"source={StatusWireVocabulary.SourceAvailability(record.SourceAvailability)}; "
                + $"registered={Value(record.Registered)}");
            builder.AppendLine($"    Counts: {CountsText(record.Counts, includeRegistered: false)}");
            foreach (var link in record.Links)
            {
                builder.AppendLine($"    {Text(link.Observation.Mapping.DestinationPath.Value)}: {LinkState(link.Observation.State)}");
            }
        }
    }

    private static void Validate(StatusLibrary library)
    {
        ArgumentNullException.ThrowIfNull(library);
        if (!Enum.IsDefined(library.State))
        {
            throw new ArgumentOutOfRangeException(nameof(library), library.State, "The Library semantic state is not defined.");
        }
    }

    private static StatusJsonLibraryCounts Counts(StatusLibraryCounts value)
        => new()
        {
            Registered = Integer(value.Registered),
            Current = Integer(value.Current),
            Missing = Integer(value.Missing),
            Changed = Integer(value.Changed),
            Blocked = Integer(value.Blocked),
            Unavailable = Integer(value.Unavailable),
        };

    private static StatusJsonLibraryLinkCounts LinkCounts(StatusLibraryCounts value)
        => new()
        {
            Current = Integer(value.Current),
            Missing = Integer(value.Missing),
            Changed = Integer(value.Changed),
            Blocked = Integer(value.Blocked),
            Unavailable = Integer(value.Unavailable),
        };

    private static StatusJsonIntegerValue Integer(StatusIntegerValue value)
        => new()
        {
            State = StatusWireVocabulary.ValueState(value.State),
            Value = value.Value,
        };

    private static string Status(StatusLibrary library)
    {
        if (library.Observation.Record.State == LibrariesRecordReadState.Missing)
        {
            return "absent";
        }

        return library.State switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => "trusted",
            CliSemanticStatus.Incomplete or CliSemanticStatus.Invalid
                or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted => "incomplete",
            CliSemanticStatus.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(library), library.State, "The Library semantic state is not defined."),
        };
    }

    private static string RecordState(LibrariesRecordReadState value)
        => value switch
        {
            LibrariesRecordReadState.Missing => "missing",
            LibrariesRecordReadState.Complete => "complete",
            LibrariesRecordReadState.Malformed => "invalid",
            LibrariesRecordReadState.Unavailable => "unavailable",
            LibrariesRecordReadState.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library record state is not defined."),
        };

    private static string SourceState(LibrarySourceRootState value)
        => value switch
        {
            LibrarySourceRootState.Available => "available",
            LibrarySourceRootState.Missing => "missing",
            LibrarySourceRootState.Invalid => "invalid",
            LibrarySourceRootState.Inaccessible => "unavailable",
            LibrarySourceRootState.Blocked => "blocked",
            LibrarySourceRootState.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library source-root state is not defined."),
        };

    private static string LinkState(LibraryMappingObservationState value)
        => value switch
        {
            LibraryMappingObservationState.Current => "current",
            LibraryMappingObservationState.Missing => "missing",
            LibraryMappingObservationState.Changed => "changed",
            LibraryMappingObservationState.Blocked => "blocked",
            LibraryMappingObservationState.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library mapping state is not defined."),
        };

    private static string Text(string value)
        => string.Concat(value.Select(character => char.IsControl(character) ? '\uFFFD' : character));

    private static string CountsText(StatusLibraryCounts counts, bool includeRegistered)
    {
        var registered = includeRegistered ? $"registered={Value(counts.Registered)}, " : string.Empty;
        return $"{registered}current={Value(counts.Current)}, missing={Value(counts.Missing)}, "
            + $"changed={Value(counts.Changed)}, blocked={Value(counts.Blocked)}, "
            + $"unavailable={Value(counts.Unavailable)}";
    }

    private static string Value(StatusIntegerValue value)
        => value.State == Framework.OperationalContributors.Models.OperationalValueState.Available
            ? value.Value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "unavailable"
            : StatusWireVocabulary.ValueState(value.State);
}
