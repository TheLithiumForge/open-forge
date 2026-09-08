using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Shared.Planning;

internal static class LibrarySyncPlanner
{
    internal static LibrarySyncPlan Plan(LibrarySyncPlanningInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.ConsumerBoundary);
        input.ConsumerBoundary.Validate(input.Request.Workspace);
        cancellationToken.ThrowIfCancellationRequested();

        var findings = ImmutableArray.CreateBuilder<LibrarySyncFinding>();
        var selected = ReadRecord(input, findings, out var record, out var recordSnapshot);
        if (selected is null && findings.Any(finding => finding.Code == LibrarySyncFindingCode.UnknownId))
        {
            return Empty(input, LibraryPlanState.Blocked, findings);
        }

        if (input.GeneratedNavigationIssue is { } navigationIssue)
        {
            Add(
                findings,
                navigationIssue.State == LibraryGeneratedNavigationIssueState.Blocked
                    ? LibrarySyncFindingCode.GeneratedNavigationBlocked
                    : LibrarySyncFindingCode.GeneratedNavigationIncomplete,
                navigationIssue.State == LibraryGeneratedNavigationIssueState.Blocked
                    ? CliSemanticStatus.Blocked
                    : CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value,
                navigationIssue.Path,
                navigationIssue.Cause);
        }

        var entries = ReadSource(input, selected, findings);
        var currentPaths = entries.Select(entry => entry.SourcePath.Value).ToHashSet(StringComparer.Ordinal);
        var registeredPaths = selected?.Paths.Select(path => path.Value).ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);
        var destinations = currentPaths.Union(registeredPaths).Order(StringComparer.Ordinal).ToImmutableArray();

        if (LibraryMutationPlanningPolicy.HasDestinationAlias(input.Request.Workspace, destinations))
        {
            Add(findings, LibrarySyncFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, path: null,
                "Two Library destinations identify the same physical path.");
        }

        if (!LibraryMutationPlanningPolicy.CoversDestinationAncestors(input.ConsumerBoundary, destinations))
        {
            Add(findings, LibrarySyncFindingCode.ConsumerBlocked, CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value, path: null,
                "The Library consumer-directory boundary does not cover every required destination ancestor.");
        }

        var boundary = LibraryMutationPlanningPolicy.EvaluateConsumerBoundary(
            input.ConsumerBoundary,
            allowMissingAncestors: true);
        if (boundary.State != LibraryPlanState.Complete)
        {
            Add(findings, LibrarySyncFindingCode.ConsumerBlocked,
                boundary.State == LibraryPlanState.Blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value, boundary.Path,
                boundary.Cause ?? "The Library consumer-directory boundary is unavailable.");
        }

        var links = EvaluateMappings(input, selected, currentPaths, registeredPaths, findings);
        if (LibraryMutationPlanningPolicy.TryFindOwnershipConflict(
            input.Ownership,
            destinations.Concat(RelativePaths(input)),
            out var ownershipConflict))
        {
            Add(findings, LibrarySyncFindingCode.OwnershipConflict, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, ownershipConflict?.Path,
                "The destination is already owned by another managed domain.");
        }

        var state = PlanState(findings);
        if (state != LibraryPlanState.Complete
            || selected is null
            || record is null
            || recordSnapshot is null)
        {
            return Empty(input, state, findings);
        }

        var replacement = LibraryRecord.Create(
            selected.Id,
            selected.SourceRoot,
            [.. entries.Select(entry => entry.SourcePath)]);
        var intendedRecord = LibrariesRecord.Create(
            [.. record.Libraries
                .Select(library => library.Id == selected.Id ? replacement : library)
                .OrderBy(library => library.Id.Value, StringComparer.Ordinal)]);
        var intendedBytes = LibrariesRecordCodec.Write(intendedRecord);
        return new LibrarySyncPlan
        {
            Input = input,
            State = LibraryPlanState.Complete,
            Directories = boundary.Directories,
            Links = links,
            GeneratedRegions = OrderGenerated(input.GeneratedRegionChanges),
            RecordChange = LibraryMutationPlanningPolicy.CreateRecordChange(recordSnapshot, intendedBytes, delete: false),
            IntendedRecord = intendedRecord,
            Findings = Order(findings),
        };
    }

    private static LibraryRecord? ReadRecord(
        LibrarySyncPlanningInput input,
        ImmutableArray<LibrarySyncFinding>.Builder findings,
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
                Add(findings, LibrarySyncFindingCode.UnknownId, CliSemanticStatus.Invalid,
                    input.Request.LibraryId.Value, path: null, "The Library ID is not registered.");
            }

            return selected;
        }

        var (code, status, cause) = input.Record.State switch
        {
            LibrariesRecordReadState.Unavailable => (
                LibrarySyncFindingCode.RecordUnavailable,
                CliSemanticStatus.Incomplete,
                input.Record.Cause ?? "The Library record is unavailable."),
            LibrariesRecordReadState.Malformed => (
                LibrarySyncFindingCode.RecordInvalid,
                CliSemanticStatus.Blocked,
                input.Record.Cause ?? "The Library record is malformed."),
            LibrariesRecordReadState.Blocked => (
                LibrarySyncFindingCode.RecordBlocked,
                CliSemanticStatus.Blocked,
                input.Record.Cause ?? "The Library record is unsafe."),
            LibrariesRecordReadState.Missing => (
                LibrarySyncFindingCode.UnknownId,
                CliSemanticStatus.Invalid,
                "The Library ID is not registered."),
            _ => (
                LibrarySyncFindingCode.RecordBlocked,
                CliSemanticStatus.Blocked,
                "The Library record facts are inconsistent."),
        };
        Add(findings, code, status, input.Request.LibraryId.Value, path: null, cause);
        return null;
    }

    private static ImmutableArray<EligibleSourceFile> ReadSource(
        LibrarySyncPlanningInput input,
        LibraryRecord? selected,
        ImmutableArray<LibrarySyncFinding>.Builder findings)
    {
        if (selected is null)
        {
            return [];
        }

        var source = input.Source.Source;
        if (source.Request.Workspace != input.Request.Workspace
            || source.Request.SourceRoot != selected.SourceRoot)
        {
            Add(findings, LibrarySyncFindingCode.SourceRootBlocked, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, selected.SourceRoot.Value,
                "The source observation does not belong to the registered Library root.");
            return [];
        }

        switch (source.State)
        {
            case LibrarySourceRootState.Available:
                break;
            case LibrarySourceRootState.Blocked:
                Add(findings, LibrarySyncFindingCode.SourceRootBlocked, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, selected.SourceRoot.Value,
                    source.Cause ?? "The Library source root is unsafe.");
                return [];
            case LibrarySourceRootState.Missing:
            case LibrarySourceRootState.Invalid:
            case LibrarySourceRootState.Inaccessible:
            case LibrarySourceRootState.Unavailable:
                Add(findings,
                    source.State == LibrarySourceRootState.Invalid
                        ? LibrarySyncFindingCode.SourceRootInvalid
                        : LibrarySyncFindingCode.SourceRootUnavailable,
                    CliSemanticStatus.Incomplete,
                    input.Request.LibraryId.Value, selected.SourceRoot.Value,
                    source.Cause ?? "The Library source root is unavailable.");
                return [];
            default:
                throw new ArgumentOutOfRangeException(nameof(input), source.State, "The source-root state is not defined.");
        }

        if (input.Source.Inventory is not { } inventory
            || inventory.SourceRoot != selected.SourceRoot)
        {
            Add(findings, LibrarySyncFindingCode.InventoryIncomplete, CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value, selected.SourceRoot.Value,
                "The complete Library inventory is unavailable.");
            return [];
        }

        if (inventory.State != LibraryInventoryState.Complete)
        {
            Add(findings,
                inventory.State == LibraryInventoryState.Blocked
                    ? LibrarySyncFindingCode.SourceRootBlocked
                    : LibrarySyncFindingCode.InventoryIncomplete,
                inventory.State == LibraryInventoryState.Blocked
                    ? CliSemanticStatus.Blocked
                    : CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value, selected.SourceRoot.Value,
                inventory.Cause ?? "The Library inventory is incomplete.");
        }

        return inventory.Entries;
    }

    private static ImmutableArray<RelativeFileLinkEffect> EvaluateMappings(
        LibrarySyncPlanningInput input,
        LibraryRecord? selected,
        HashSet<string> currentPaths,
        HashSet<string> registeredPaths,
        ImmutableArray<LibrarySyncFinding>.Builder findings)
    {
        var byPath = new Dictionary<string, LibraryMappingObservation>(StringComparer.Ordinal);
        foreach (var observation in input.Mappings)
        {
            if (!byPath.TryAdd(observation.Mapping.SourcePath.Value, observation))
            {
                Add(findings, LibrarySyncFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, observation.LogicalDestinationPath,
                    "The destination was observed more than once.");
            }

            if (observation.State == LibraryMappingObservationState.Blocked)
            {
                Add(findings, LibrarySyncFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, observation.LogicalDestinationPath,
                    observation.Cause ?? "The Library mapping is unsafe.");
            }
        }

        if (selected is null)
        {
            return [];
        }

        var links = ImmutableArray.CreateBuilder<RelativeFileLinkEffect>();
        foreach (var path in currentPaths.Union(registeredPaths).Order(StringComparer.Ordinal))
        {
            var sourcePath = SourceRelativeEligiblePath.Create(path);
            var expected = LibraryMapping.Create(selected.SourceRoot, sourcePath);
            if (!byPath.Remove(path, out var observation)
                || observation.Mapping.DestinationPath != expected.DestinationPath
                || observation.Mapping.ExpectedRelativeLink != expected.ExpectedRelativeLink)
            {
                Add(findings, LibrarySyncFindingCode.MappingUnavailable, CliSemanticStatus.Incomplete,
                    input.Request.LibraryId.Value, path,
                    "The exact Library mapping was not observed.");
                continue;
            }

            var isCurrent = currentPaths.Contains(path);
            var isRegistered = registeredPaths.Contains(path);
            switch (observation.State)
            {
                case LibraryMappingObservationState.Current when isCurrent && isRegistered:
                    break;
                case LibraryMappingObservationState.Missing when isCurrent:
                    links.Add(RelativeFileLinkEffect.Create(expected.DestinationPath.CanonicalPath, observation.ExpectedLink));
                    break;
                case LibraryMappingObservationState.Current when !isCurrent && isRegistered:
                    links.Add(RelativeFileLinkEffect.Delete(expected.DestinationPath.CanonicalPath, observation.ExpectedLink));
                    break;
                case LibraryMappingObservationState.Missing when !isCurrent && isRegistered:
                    Add(findings, LibrarySyncFindingCode.RetiredLinkMissing, CliSemanticStatus.Blocked,
                        input.Request.LibraryId.Value, path,
                        "A retired registered mapping is missing and cannot prove owned removal.");
                    break;
                case LibraryMappingObservationState.Current:
                case LibraryMappingObservationState.Changed:
                    Add(findings, LibrarySyncFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                        input.Request.LibraryId.Value, path,
                        "Library sync never adopts or replaces an unowned destination.");
                    break;
                case LibraryMappingObservationState.Blocked:
                    break;
                case LibraryMappingObservationState.Unavailable:
                    Add(findings, LibrarySyncFindingCode.MappingUnavailable, CliSemanticStatus.Incomplete,
                        input.Request.LibraryId.Value, path,
                        observation.Cause ?? "The Library mapping is unavailable.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input), observation.State, "The mapping state is not defined.");
            }
        }

        foreach (var extra in byPath.Values)
        {
            Add(findings, LibrarySyncFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, extra.LogicalDestinationPath,
                "An unexpected destination mapping was supplied to Library sync.");
        }

        return [.. links.OrderBy(link => link.DestinationPath.Value, StringComparer.Ordinal)];
    }

    private static IEnumerable<string> RelativePaths(LibrarySyncPlanningInput input)
        => input.GeneratedRegionChanges.Select(change => Path.GetRelativePath(
            input.Request.Workspace.LexicalRoot,
            change.LogicalPath).Replace(Path.DirectorySeparatorChar, '/'));

    private static LibraryPlanState PlanState(IEnumerable<LibrarySyncFinding> findings)
        => findings.Any(finding => finding.Status == CliSemanticStatus.Blocked)
            ? LibraryPlanState.Blocked
            : findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete)
                ? LibraryPlanState.Incomplete
                : LibraryPlanState.Complete;

    private static LibrarySyncPlan Empty(
        LibrarySyncPlanningInput input,
        LibraryPlanState state,
        ImmutableArray<LibrarySyncFinding>.Builder findings)
        => new()
        {
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

    private static ImmutableArray<LibrarySyncFinding> Order(IEnumerable<LibrarySyncFinding> findings)
        => [.. findings.OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.LibraryId, StringComparer.Ordinal)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)];

    private static void Add(
        ImmutableArray<LibrarySyncFinding>.Builder findings,
        LibrarySyncFindingCode code,
        CliSemanticStatus status,
        string? libraryId,
        string? path,
        string cause)
        => findings.Add(new LibrarySyncFinding
        {
            Code = code,
            Status = status,
            LibraryId = libraryId,
            Path = path,
            Cause = cause,
        });
}
