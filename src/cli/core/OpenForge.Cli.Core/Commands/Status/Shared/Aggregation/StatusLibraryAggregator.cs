using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusLibraryAggregator
{
    internal static StatusLibrary Build(LibraryStatusView observation)
    {
        ArgumentNullException.ThrowIfNull(observation);
        var findings = new List<StatusFinding>();
        if (observation.State == OperationalViewState.Interrupted)
        {
            findings.Add(Finding(
                StatusFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                subject: null,
                "The Library observation was interrupted."));
        }
        else if (observation.Record.State is LibrariesRecordReadState.Malformed or LibrariesRecordReadState.Blocked)
        {
            findings.Add(Finding(
                StatusFindingCode.LibraryRecordMalformed,
                CliSemanticStatus.Blocked,
                ".agents/open-forge.libraries.json",
                observation.Record.Cause ?? "The Library record is unsafe or malformed."));
        }
        else if (observation.Record.State == LibrariesRecordReadState.Unavailable)
        {
            findings.Add(Finding(
                StatusFindingCode.LibraryRecordUnavailable,
                CliSemanticStatus.Incomplete,
                ".agents/open-forge.libraries.json",
                observation.Record.Cause ?? "The Library record is unavailable."));
        }

        var records = new List<StatusLibraryRegistration>();
        foreach (var library in observation.Record.Record?.Libraries ?? [])
        {
            var source = observation.Sources.SingleOrDefault(value => value.Request.SourceRoot == library.SourceRoot);
            if (source is not null && source.State != LibrarySourceRootState.Available)
            {
                var blocked = source.State is LibrarySourceRootState.Invalid or LibrarySourceRootState.Blocked;
                findings.Add(Finding(
                    source.State == LibrarySourceRootState.Invalid
                        ? StatusFindingCode.LibrarySourceRootInvalid
                        : source.State == LibrarySourceRootState.Blocked
                            ? StatusFindingCode.LibrarySourceRootAliased
                            : StatusFindingCode.LibrarySourceRootUnavailable,
                    blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete,
                    library.SourceRoot.Value,
                    source.Cause ?? "The registered Library source root is unavailable."));
            }

            var paths = library.Paths.Select(path => path.Value).ToHashSet(StringComparer.Ordinal);
            var links = observation.Mappings
                .Where(mapping => paths.Contains(mapping.Mapping.SourcePath.Value))
                .OrderBy(mapping => mapping.Mapping.DestinationPath.Value, StringComparer.Ordinal)
                .Select(mapping =>
                {
                    AddMappingFinding(findings, library.Id.Value, mapping);
                    return new StatusLibraryLink(
                        mapping,
                        SourceIdentity.DeriveId(mapping.Mapping.DestinationPath.Value) ?? string.Empty);
                })
                .ToImmutableArray();
            var counts = Counts(links.Select(link => link.Observation));
            records.Add(new StatusLibraryRegistration
            {
                Id = library.Id,
                SourceRoot = library.SourceRoot,
                SourceRootState = source?.State ?? LibrarySourceRootState.Unavailable,
                SourceAvailability = source?.State == LibrarySourceRootState.Available
                    ? OperationalSourceAvailability.Available
                    : OperationalSourceAvailability.Unavailable,
                Registered = Count(library.Paths.Length),
                Counts = counts,
                Links = links,
            });
        }

        var allLinks = records.SelectMany(record => record.Links).Select(link => link.Observation).ToArray();
        var status = Status(findings);
        return new StatusLibrary
        {
            State = status,
            Observation = observation,
            Records = [.. records],
            Counts = observation.Record.State is LibrariesRecordReadState.Complete or LibrariesRecordReadState.Missing
                ? Counts(allLinks, observation.Record.Record?.Libraries.Sum(record => record.Paths.Length) ?? 0)
                : UnavailableCounts(),
            Findings = [.. findings.OrderBy(finding => finding.Code).ThenBy(finding => finding.Subject, StringComparer.Ordinal)],
        };
    }

    internal static StatusLibrary Unavailable(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        var observation = new LibraryStatusView
        {
            State = OperationalViewState.Incomplete,
            Ownership = null,
            LinkCapability = null,
            Record = new LibrariesRecordRead
            {
                State = LibrariesRecordReadState.Unavailable,
                Record = null,
                Snapshot = null,
                Cause = cause,
            },
            Sources = [],
            Mappings = [],
        };
        return new StatusLibrary
        {
            State = CliSemanticStatus.Incomplete,
            Observation = observation,
            Records = [],
            Counts = UnavailableCounts(),
            Findings = [],
        };
    }

    private static void AddMappingFinding(
        List<StatusFinding> findings,
        string libraryId,
        LibraryMappingObservation mapping)
    {
        var (code, status, cause) = mapping.State switch
        {
            LibraryMappingObservationState.Current => (Code: (StatusFindingCode?)null, CliSemanticStatus.Complete, string.Empty),
            LibraryMappingObservationState.Missing => (StatusFindingCode.LibraryProjectionMissing, CliSemanticStatus.Attention, "The registered Library projection is missing."),
            LibraryMappingObservationState.Changed =>
                (StatusFindingCode.LibraryProjectionChanged, CliSemanticStatus.Attention, "The registered Library projection differs from its exact link identity."),
            LibraryMappingObservationState.Unavailable =>
                (StatusFindingCode.LibraryProjectionUnavailable, CliSemanticStatus.Incomplete, mapping.Cause ?? "The registered Library projection is unavailable."),
            LibraryMappingObservationState.Blocked => (StatusFindingCode.LibraryProjectionBlocked, CliSemanticStatus.Blocked, mapping.Cause ?? "The registered Library projection is unsafe."),
            _ => throw new ArgumentOutOfRangeException(nameof(mapping), mapping.State, "The Library mapping state is not defined."),
        };
        if (code is { } value)
        {
            findings.Add(Finding(value, status, $"{libraryId}:{mapping.LogicalDestinationPath}", cause));
        }
    }

    private static StatusLibraryCounts Counts(
        IEnumerable<LibraryMappingObservation> mappings,
        int? registered = null)
    {
        var values = mappings.ToArray();
        return new StatusLibraryCounts
        {
            Registered = Count(registered ?? values.Length),
            Current = Count(values.Count(value => value.State == LibraryMappingObservationState.Current)),
            Missing = Count(values.Count(value => value.State == LibraryMappingObservationState.Missing)),
            Changed = Count(values.Count(value => value.State == LibraryMappingObservationState.Changed)),
            Blocked = Count(values.Count(value => value.State == LibraryMappingObservationState.Blocked)),
            Unavailable = Count(values.Count(value => value.State == LibraryMappingObservationState.Unavailable)),
        };
    }

    private static StatusIntegerValue Count(int value)
        => new(OperationalValueState.Available, value);

    private static StatusLibraryCounts UnavailableCounts()
        => new()
        {
            Registered = UnavailableCount(),
            Current = UnavailableCount(),
            Missing = UnavailableCount(),
            Changed = UnavailableCount(),
            Blocked = UnavailableCount(),
            Unavailable = UnavailableCount(),
        };

    private static StatusIntegerValue UnavailableCount()
        => new(OperationalValueState.Unavailable, null);

    private static CliSemanticStatus Status(IEnumerable<StatusFinding> findings)
    {
        var statuses = findings.Select(finding => finding.Status).ToArray();
        if (statuses.Contains(CliSemanticStatus.Interrupted))
        {
            return CliSemanticStatus.Interrupted;
        }

        if (statuses.Contains(CliSemanticStatus.Failed))
        {
            return CliSemanticStatus.Failed;
        }

        if (statuses.Contains(CliSemanticStatus.Invalid))
        {
            return CliSemanticStatus.Invalid;
        }

        if (statuses.Contains(CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (statuses.Contains(CliSemanticStatus.Incomplete))
        {
            return CliSemanticStatus.Incomplete;
        }

        return statuses.Contains(CliSemanticStatus.Attention)
            ? CliSemanticStatus.Attention
            : CliSemanticStatus.Complete;
    }

    private static StatusFinding Finding(
        StatusFindingCode code,
        CliSemanticStatus status,
        string? subject,
        string cause)
        => new() { Code = code, Status = status, Subject = subject, Cause = cause };
}
