using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
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
        if (input.Record.State != LibraryRegistrationReadState.Malformed
            && input.Record.OwnershipObservation is { } observation)
        {
            Add(findings, LibraryDetachFindingCode.OwnershipObservation, CliSemanticStatus.Complete,
                input.Request.LibraryId.Value, path: null, observation);
            return Empty(input, LibraryPlanState.Complete, findings);
        }
        var selected = ReadRecord(input, findings, out var record);
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
            Add(findings, LibraryDetachFindingCode.DestinationProtected, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, conflict, "The Library destination is protected, source-owned, or registered to another Library.");
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
            || record is null)
        {
            return Empty(input, state, findings);
        }

        var remaining = record.Libraries.Where(library => library.Id != selected.Id).ToImmutableArray();
        LibraryRegistrationSet? intendedRecord = remaining.Length == 0 ? null : LibraryRegistrationSet.Create(remaining);
        var ownershipChange = LibraryMutationPlanningPolicy.CreateOwnershipChange(
            input.Ownership,
            [.. (intendedRecord?.Libraries ?? []).Select(library => new LibraryOwnership(
                library.Id.Value,
                library.SourceRoot.Value,
                library.DestinationRoot.Value,
                [.. library.Paths.Select(path => path.Value)]))]);
        return new LibraryDetachPlan
        {
            Permissions = null,
            Input = input,
            State = LibraryPlanState.Complete,
            Directories = [],
            Links = links,
            GeneratedRegions = OrderGenerated(input.GeneratedRegionChanges),
            OwnershipChange = ownershipChange,
            IntendedRecord = intendedRecord,
            Findings = Order(findings),
        };
    }

    private static LibraryRegistration? ReadRecord(
        LibraryDetachPlanningInput input,
        ImmutableArray<LibraryDetachFinding>.Builder findings,
        out LibraryRegistrationSet? record)
    {
        record = null;
        if (input.Record.State == LibraryRegistrationReadState.Complete
            && input.Record.Record is { } completeRecord)
        {
            record = completeRecord;
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
            LibraryRegistrationReadState.Unavailable => (
                LibraryDetachFindingCode.RecordUnavailable,
                CliSemanticStatus.Incomplete,
                input.Record.Cause ?? "The Library record is unavailable."),
            LibraryRegistrationReadState.Malformed => (
                LibraryDetachFindingCode.RecordInvalid,
                CliSemanticStatus.Incomplete,
                input.Record.Cause ?? "The Library record is malformed."),
            LibraryRegistrationReadState.Blocked => (
                LibraryDetachFindingCode.RecordBlocked,
                CliSemanticStatus.Blocked,
                input.Record.Cause ?? "The Library record is unsafe."),
            LibraryRegistrationReadState.Missing => (
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
        LibraryRegistration? selected,
        ImmutableArray<LibraryDetachFinding>.Builder findings)
    {
        var byPath = new Dictionary<string, LibraryMappingObservation>(StringComparer.Ordinal);
        foreach (var observation in input.Mappings)
        {
            var occupantKind = ReadOccupantKind(observation.Leaf.State);
            if (!byPath.TryAdd(observation.Mapping.SourcePath.Value, observation))
            {
                Add(findings, LibraryDetachFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, observation.LogicalDestinationPath,
                    "The destination was observed more than once.",
                    occupantKind);
            }

            if (observation.State == LibraryMappingObservationState.Blocked)
            {
                Add(findings, LibraryDetachFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, observation.LogicalDestinationPath,
                    MappingCause(observation, observation.Cause ?? "The Library mapping is unsafe."),
                    occupantKind);
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

            var occupantKind = ReadOccupantKind(observation.Leaf.State);
            switch (observation.State)
            {
                case LibraryMappingObservationState.Current:
                    links.Add(RelativeFileLinkEffect.Delete(expected.DestinationPath.CanonicalPath, observation.ExpectedLink));
                    break;
                case LibraryMappingObservationState.Missing:
                    Add(findings, LibraryDetachFindingCode.RegisteredLinkMissing, CliSemanticStatus.Attention,
                        input.Request.LibraryId.Value, registeredPath.Value,
                        "The registered Library mapping is already absent; no file was removed.");
                    break;
                case LibraryMappingObservationState.Changed when occupantKind == LibraryDetachOccupantKind.OrdinaryFile:
                    Add(findings, LibraryDetachFindingCode.MappingBlocked, CliSemanticStatus.Attention,
                        input.Request.LibraryId.Value, registeredPath.Value,
                        "The changed ordinary destination was kept; no file was removed.",
                        occupantKind);
                    break;
                case LibraryMappingObservationState.Changed:
                    Add(findings, LibraryDetachFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                        input.Request.LibraryId.Value, registeredPath.Value,
                        "Library detach never removes a changed destination occupant.",
                        occupantKind);
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
            var occupantKind = ReadOccupantKind(extra.Leaf.State);
            Add(findings, LibraryDetachFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, extra.LogicalDestinationPath,
                "An unexpected destination mapping was supplied to Library detach.",
                occupantKind);
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
            OwnershipChange = null,
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

    private static LibraryDetachOccupantKind? ReadOccupantKind(NoFollowLeafState state)
        => state switch
        {
            NoFollowLeafState.OrdinaryFile => LibraryDetachOccupantKind.OrdinaryFile,
            NoFollowLeafState.Directory => LibraryDetachOccupantKind.Folder,
            NoFollowLeafState.RelativeFileLink or NoFollowLeafState.Link => LibraryDetachOccupantKind.DifferentLink,
            NoFollowLeafState.Missing
                or NoFollowLeafState.ReparsePoint
                or NoFollowLeafState.Special
                or NoFollowLeafState.Inaccessible
                or NoFollowLeafState.Unknown => null,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The no-follow leaf state is not defined."),
        };

    private static string MappingCause(LibraryMappingObservation observation, string fallback)
        => ReadOccupantKind(observation.Leaf.State) is null
            ? $"{observation.LogicalDestinationPath} could not be classified as a supported destination occupant."
            : fallback;

    private static void Add(
        ImmutableArray<LibraryDetachFinding>.Builder findings,
        LibraryDetachFindingCode code,
        CliSemanticStatus status,
        string? libraryId,
        string? path,
        string cause,
        LibraryDetachOccupantKind? occupantKind = null)
        => findings.Add(new LibraryDetachFinding
        {
            Code = code,
            Status = status,
            LibraryId = libraryId,
            Path = path,
            Cause = cause,
            OccupantKind = occupantKind,
        });
}
