using OpenForge.Cli.Core.Framework.Libraries;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
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
        else if (observation.Record.State is (LibraryRegistrationReadState.Malformed or LibraryRegistrationReadState.Blocked))
        {
            findings.Add(Finding(
                StatusFindingCode.LibraryRecordMalformed,
                CliSemanticStatus.Blocked,
                ".agents/open-forge.lock.json",
                observation.Record.Cause ?? "The Library record is unsafe or malformed."));
        }
        else if (observation.Record.State == LibraryRegistrationReadState.Unavailable)
        {
            findings.Add(Finding(
                StatusFindingCode.LibraryRecordUnavailable,
                CliSemanticStatus.Incomplete,
                ".agents/open-forge.lock.json",
                observation.Record.Cause ?? "The Library record is unavailable."));
        }

        if (observation.Record.OwnershipObservation is { } observationCause
            && observation.Record.State == LibraryRegistrationReadState.Missing)
        {
            findings.Add(Finding(StatusFindingCode.LibraryOwnershipObservation, CliSemanticStatus.Complete,
                ".agents/open-forge.lock.json", observationCause));
        }
        var records = new List<StatusLibraryRegistration>();
        foreach (var library in observation.Record.Record?.Libraries ?? [])
        {
            var source = observation.Sources.FirstOrDefault(value => value.Request.SourceRoot == library.SourceRoot);
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
                    source.Cause ?? "The registered Library source root is unavailable.",
                    library.Id.Value));
            }

            var registeredMappings = LibraryPathIdentity.Mappings(library).ToHashSet();
            var links = observation.Mappings
                .Where(mapping => registeredMappings.Contains(mapping.Mapping))
                .OrderBy(mapping => mapping.Mapping.DestinationPath.Value, StringComparer.Ordinal)
                .Select(mapping =>
                {
                    AddMappingFinding(findings, library.Id.Value, mapping);
                    return new StatusLibraryLink(
                        mapping,
                        SourceIdentity.DeriveId(mapping.Mapping.DestinationPath.Value))
                    {
                        State = StatusStateMap.LibraryTarget(mapping.State),
                    };
                })
                .ToImmutableArray();
            var counts = Counts(links.Select(link => link.Observation));
            records.Add(new StatusLibraryRegistration
            {
                Id = library.Id,
                SourceRoot = library.SourceRoot,
                DestinationRoot = library.DestinationRoot,
                SourceRootState = source is null ? StatusLibrarySourceRootState.Unavailable : StatusStateMap.LibrarySourceRoot(source.State),
                SourceAvailability = source?.State == LibrarySourceRootState.Available
                    ? StatusSourceAvailability.Available
                    : StatusSourceAvailability.Unavailable,
                Registered = Count(library.Paths.Length),
                Counts = counts,
                Links = links,
                SourceRootCause = source?.Cause,
            });
        }

        var allLinks = records.SelectMany(record => record.Links).Select(link => link.Observation).ToArray();
        var status = Status(findings);
        return new StatusLibrary
        {
            State = status,
            Observation = observation,
            RecordCause = observation.Record.Cause,
            OwnershipObservation = observation.Record.OwnershipObservation,
            RecordState = StatusStateMap.LibraryRecord(observation.Record.State),
            Records = [.. records],
            Counts = observation.Record.State is LibraryRegistrationReadState.Complete or LibraryRegistrationReadState.Missing
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
            Record = new LibraryRegistrationRead
            {
                State = LibraryRegistrationReadState.Unavailable,
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
            RecordCause = observation.Record.Cause,
            OwnershipObservation = observation.Record.OwnershipObservation,
            RecordState = StatusLibraryRecordState.Unavailable,
            Records = [],
            Counts = UnavailableCounts(),
            Findings =
            [
                Finding(
                    StatusFindingCode.LibraryRecordUnavailable,
                    CliSemanticStatus.Incomplete,
                    ".agents/open-forge.lock.json",
                    cause),
            ],
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
            findings.Add(Finding(value, status, mapping.LogicalDestinationPath, cause, libraryId));
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
        => new(StatusValueState.Available, value);

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
        => new(StatusValueState.Unavailable, null);

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
        string cause,
        string? owner = null)
        => new() { Code = code, Status = status, Subject = subject, Cause = cause, Owner = owner };
}
