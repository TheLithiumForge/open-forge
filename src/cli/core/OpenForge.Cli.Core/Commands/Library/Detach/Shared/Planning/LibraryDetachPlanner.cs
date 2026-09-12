using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Shared.Planning;

internal static class LibraryDetachPlanner
{
    internal static LibraryDetachPlan Plan(LibraryDetachPlanningInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.ConsumerBoundary);
        input.ConsumerBoundary.Validate(input.Request.Workspace);
        cancellationToken.ThrowIfCancellationRequested();

        var findings = ImmutableArray.CreateBuilder<LibraryDetachFinding>();
        var selected = ReadRecord(input, findings, out var record, out var recordSnapshot);
        if (selected is null && findings.Any(finding => finding.Code == LibraryDetachFindingCode.UnknownId))
        {
            return Empty(input, LibraryPlanState.Blocked, findings);
        }

        if (input.GeneratedNavigationIssue is { } navigationIssue)
        {
            Add(
                findings,
                navigationIssue.State == LibraryGeneratedNavigationIssueState.Blocked
                    ? LibraryDetachFindingCode.GeneratedNavigationBlocked
                    : LibraryDetachFindingCode.GeneratedNavigationIncomplete,
                navigationIssue.State == LibraryGeneratedNavigationIssueState.Blocked
                    ? CliSemanticStatus.Blocked
                    : CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value,
                navigationIssue.Path,
                navigationIssue.Cause);
        }

        var destinations = selected is null ? [] : LibraryPathIdentity.Mappings(selected)
            .Select(mapping => mapping.DestinationPath.Value).ToImmutableArray();
        if (selected is not null && LibraryDestinationPolicy.FindConflict(input.Request.Workspace, selected, record, RelativePaths(input)) is { } conflict)
        {
            Add(findings, LibraryDetachFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, conflict, "The Library destination is protected, source-owned, or registered to another Library.");
        }
        if (LibraryMutationPlanningPolicy.HasDestinationAlias(input.Request.Workspace, destinations))
        {
            Add(findings, LibraryDetachFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, path: null,
                "Two Library destinations identify the same physical path.");
        }

        if (!LibraryMutationPlanningPolicy.CoversDestinationAncestors(input.ConsumerBoundary, destinations))
        {
            Add(findings, LibraryDetachFindingCode.ConsumerBlocked, CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value, path: null,
                "The Library consumer-directory boundary does not cover every registered destination ancestor.");
        }

        var boundary = LibraryMutationPlanningPolicy.EvaluateConsumerBoundary(
            input.ConsumerBoundary,
            allowMissingAncestors: false);
        if (boundary.State != LibraryPlanState.Complete)
        {
            Add(findings, LibraryDetachFindingCode.ConsumerBlocked,
                boundary.State == LibraryPlanState.Blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value, boundary.Path,
                boundary.Cause ?? "The Library consumer-directory boundary is unavailable.");
        }

        var links = EvaluateMappings(input, selected, findings);
        if (LibraryMutationPlanningPolicy.TryFindOwnershipConflict(
            input.Ownership,
            destinations.Concat(RelativePaths(input)),
            out var ownershipConflict))
        {
            Add(findings, LibraryDetachFindingCode.OwnershipConflict, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, ownershipConflict?.Path,
                "The destination is also owned by another managed domain.");
        }

        var state = PlanState(findings);
        if (state != LibraryPlanState.Complete
            || selected is null
            || record is null
            || recordSnapshot is null)
        {
            return Empty(input, state, findings);
        }

        var remaining = record.Libraries.Where(library => library.Id != selected.Id).ToImmutableArray();
        LibrariesRecord? intendedRecord = remaining.Length == 0 ? null : LibrariesRecord.Create(remaining);
        var recordChange = intendedRecord is null
            ? LibraryMutationPlanningPolicy.CreateRecordChange(recordSnapshot, [], delete: true)
            : LibraryMutationPlanningPolicy.CreateRecordChange(
                recordSnapshot,
                LibrariesRecordCodec.Write(intendedRecord),
                delete: false);
        return new LibraryDetachPlan
        {
            Permissions = null,
            Input = input,
            State = LibraryPlanState.Complete,
            Directories = [],
            Links = links,
            GeneratedRegions = OrderGenerated(input.GeneratedRegionChanges),
            RecordChange = recordChange,
            IntendedRecord = intendedRecord,
            Findings = Order(findings),
        };
    }

    private static LibraryRecord? ReadRecord(
        LibraryDetachPlanningInput input,
        ImmutableArray<LibraryDetachFinding>.Builder findings,
        out LibrariesRecord? record,
        out FileStateSnapshot? snapshot)
    {
        record = null;
        snapshot = null;
        if (input.Record.State == LibrariesRecordReadState.Complete
            && input.Record.Record is { } completeRecord
            && input.Record.Snapshot is { Kind: FileExpectationKind.File } completeSnapshot)
        {
            record = completeRecord;
            snapshot = completeSnapshot;
            var selected = completeRecord.Libraries.FirstOrDefault(library => library.Id == input.Request.LibraryId);
            if (selected is null)
            {
                Add(findings, LibraryDetachFindingCode.UnknownId, CliSemanticStatus.Invalid,
                    input.Request.LibraryId.Value, path: null, "The Library ID is not registered.");
            }

            return selected;
        }

        var (code, status, cause) = input.Record.State switch
        {
            LibrariesRecordReadState.Unavailable => (
                LibraryDetachFindingCode.RecordUnavailable,
                CliSemanticStatus.Incomplete,
                input.Record.Cause ?? "The Library record is unavailable."),
            LibrariesRecordReadState.Malformed => (
                LibraryDetachFindingCode.RecordInvalid,
                CliSemanticStatus.Blocked,
                input.Record.Cause ?? "The Library record is malformed."),
            LibrariesRecordReadState.Blocked => (
                LibraryDetachFindingCode.RecordBlocked,
                CliSemanticStatus.Blocked,
                input.Record.Cause ?? "The Library record is unsafe."),
            LibrariesRecordReadState.Missing => (
                LibraryDetachFindingCode.UnknownId,
                CliSemanticStatus.Invalid,
                "The Library ID is not registered."),
            _ => (
                LibraryDetachFindingCode.RecordBlocked,
                CliSemanticStatus.Blocked,
                "The Library record facts are inconsistent."),
        };
        Add(findings, code, status, input.Request.LibraryId.Value, path: null, cause);
        return null;
    }

    private static ImmutableArray<RelativeFileLinkEffect> EvaluateMappings(
        LibraryDetachPlanningInput input,
        LibraryRecord? selected,
        ImmutableArray<LibraryDetachFinding>.Builder findings)
    {
        var byPath = new Dictionary<string, LibraryMappingObservation>(StringComparer.Ordinal);
        foreach (var observation in input.Mappings)
        {
            if (!byPath.TryAdd(observation.Mapping.SourcePath.Value, observation))
            {
                Add(findings, LibraryDetachFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, observation.LogicalDestinationPath,
                    "The destination was observed more than once.");
            }

            if (observation.State == LibraryMappingObservationState.Blocked)
            {
                Add(findings, LibraryDetachFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, observation.LogicalDestinationPath,
                    observation.Cause ?? "The Library mapping is unsafe.");
            }
        }

        if (selected is null)
        {
            return [];
        }

        var links = ImmutableArray.CreateBuilder<RelativeFileLinkEffect>();
        foreach (var registeredPath in selected.Paths)
        {
            var expected = LibraryMapping.Create(selected.SourceRoot, selected.DestinationRoot, registeredPath);
            if (!byPath.Remove(registeredPath.Value, out var observation)
                || observation.Mapping.DestinationPath != expected.DestinationPath
                || observation.Mapping.ExpectedRelativeLink != expected.ExpectedRelativeLink)
            {
                Add(findings, LibraryDetachFindingCode.MappingUnavailable, CliSemanticStatus.Incomplete,
                    input.Request.LibraryId.Value, registeredPath.Value,
                    "The exact registered Library mapping was not observed.");
                continue;
            }

            switch (observation.State)
            {
                case LibraryMappingObservationState.Current:
                    links.Add(RelativeFileLinkEffect.Delete(expected.DestinationPath.CanonicalPath, observation.ExpectedLink));
                    break;
                case LibraryMappingObservationState.Missing:
                    Add(findings, LibraryDetachFindingCode.RegisteredLinkMissing, CliSemanticStatus.Blocked,
                        input.Request.LibraryId.Value, registeredPath.Value,
                        "A registered Library mapping is missing and cannot prove owned removal.");
                    break;
                case LibraryMappingObservationState.Changed:
                    Add(findings, LibraryDetachFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                        input.Request.LibraryId.Value, registeredPath.Value,
                        "Library detach never removes a changed destination occupant.");
                    break;
                case LibraryMappingObservationState.Blocked:
                    break;
                case LibraryMappingObservationState.Unavailable:
                    Add(findings, LibraryDetachFindingCode.MappingUnavailable, CliSemanticStatus.Incomplete,
                        input.Request.LibraryId.Value, registeredPath.Value,
                        observation.Cause ?? "The Library mapping is unavailable.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input), observation.State, "The mapping state is not defined.");
            }
        }

        foreach (var extra in byPath.Values)
        {
            Add(findings, LibraryDetachFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, extra.LogicalDestinationPath,
                "An unexpected destination mapping was supplied to Library detach.");
        }

        return [.. links.OrderBy(link => link.DestinationPath.Value, StringComparer.Ordinal)];
    }

    private static IEnumerable<string> RelativePaths(LibraryDetachPlanningInput input)
        => input.GeneratedRegionChanges.Select(change => Path.GetRelativePath(
            input.Request.Workspace.LexicalRoot,
            change.LogicalPath).Replace(Path.DirectorySeparatorChar, '/'));

    private static LibraryPlanState PlanState(IEnumerable<LibraryDetachFinding> findings)
    {
        if (findings.Any(finding => finding.Status == CliSemanticStatus.Blocked))
        {
            return LibraryPlanState.Blocked;
        }
        return findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete)
            ? LibraryPlanState.Incomplete
            : LibraryPlanState.Complete;
    }

    private static LibraryDetachPlan Empty(
        LibraryDetachPlanningInput input,
        LibraryPlanState state,
        ImmutableArray<LibraryDetachFinding>.Builder findings)
        => new()
        {
            Permissions = null,
            Input = input,
            State = state,
            Directories = [],
            Links = [],
            GeneratedRegions = [],
            RecordChange = null,
            IntendedRecord = null,
            Findings = Order(findings),
        };

    private static ImmutableArray<PlannedFileChange> OrderGenerated(ImmutableArray<PlannedFileChange> changes)
        => [.. changes.OrderBy(change => change.LogicalPath, StringComparer.Ordinal)];

    private static ImmutableArray<LibraryDetachFinding> Order(IEnumerable<LibraryDetachFinding> findings)
        => [.. findings.OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.LibraryId, StringComparer.Ordinal)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)];

    private static void Add(
        ImmutableArray<LibraryDetachFinding>.Builder findings,
        LibraryDetachFindingCode code,
        CliSemanticStatus status,
        string? libraryId,
        string? path,
        string cause)
        => findings.Add(new LibraryDetachFinding
        {
            Code = code,
            Status = status,
            LibraryId = libraryId,
            Path = path,
            Cause = cause,
        });
}
