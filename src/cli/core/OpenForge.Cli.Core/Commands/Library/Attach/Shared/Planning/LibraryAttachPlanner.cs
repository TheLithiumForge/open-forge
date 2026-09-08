using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Shared.Planning;

internal static class LibraryAttachPlanner
{
    internal static LibraryAttachPlan Plan(LibraryAttachPlanningInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.ConsumerBoundary);
        input.ConsumerBoundary.Validate(input.Request.Workspace);
        cancellationToken.ThrowIfCancellationRequested();

        var findings = ImmutableArray.CreateBuilder<LibraryAttachFinding>();
        if (input.GeneratedNavigationIssue is { } navigationIssue)
        {
            Add(
                findings,
                navigationIssue.State == LibraryGeneratedNavigationIssueState.Blocked
                    ? LibraryAttachFindingCode.GeneratedNavigationBlocked
                    : LibraryAttachFindingCode.GeneratedNavigationIncomplete,
                navigationIssue.State == LibraryGeneratedNavigationIssueState.Blocked
                    ? CliSemanticStatus.Blocked
                    : CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value,
                navigationIssue.Path,
                navigationIssue.Cause);
        }

        LibrariesRecord? currentRecord = null;
        FileStateSnapshot? recordSnapshot = null;
        switch (input.Record.State)
        {
            case LibrariesRecordReadState.Missing
                when input.Record.Record is null
                    && input.Record.Snapshot is { Kind: FileExpectationKind.Missing } missingSnapshot:
                recordSnapshot = missingSnapshot;
                break;
            case LibrariesRecordReadState.Complete
                when input.Record.Record is { } record
                    && input.Record.Snapshot is { Kind: FileExpectationKind.File } completeSnapshot:
                currentRecord = record;
                recordSnapshot = completeSnapshot;
                if (record.Libraries.Any(library => library.Id == input.Request.LibraryId))
                {
                    Add(findings, LibraryAttachFindingCode.DuplicateId, CliSemanticStatus.Blocked,
                        input.Request.LibraryId.Value, path: null, "The Library ID is already registered.");
                }

                break;
            case LibrariesRecordReadState.Malformed:
                Add(findings, LibraryAttachFindingCode.RecordInvalid, CliSemanticStatus.Invalid,
                    input.Request.LibraryId.Value, path: null,
                    input.Record.Cause ?? "The Library record is malformed.");
                break;
            case LibrariesRecordReadState.Unavailable:
                Add(findings, LibraryAttachFindingCode.RecordUnavailable, CliSemanticStatus.Incomplete,
                    input.Request.LibraryId.Value, path: null,
                    input.Record.Cause ?? "The Library record is unavailable.");
                break;
            case LibrariesRecordReadState.Blocked:
                Add(findings, LibraryAttachFindingCode.RecordBlocked, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, path: null,
                    input.Record.Cause ?? "The Library record is unsafe.");
                break;
            default:
                Add(findings, LibraryAttachFindingCode.RecordBlocked, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, path: null,
                    "The Library record facts are inconsistent.");
                break;
        }

        var entries = ReadSource(input, findings);
        var destinationPaths = entries.Select(entry => entry.SourcePath.Value).ToImmutableArray();
        if (LibraryMutationPlanningPolicy.HasDestinationAlias(input.Request.Workspace, destinationPaths))
        {
            Add(findings, LibraryAttachFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, path: null,
                "Two Library destinations identify the same physical path.");
        }

        if (!LibraryMutationPlanningPolicy.CoversDestinationAncestors(input.ConsumerBoundary, destinationPaths))
        {
            Add(findings, LibraryAttachFindingCode.ConsumerBlocked, CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value, path: null,
                "The Library consumer-directory boundary does not cover every required destination ancestor.");
        }

        var boundary = LibraryMutationPlanningPolicy.EvaluateConsumerBoundary(
            input.ConsumerBoundary,
            allowMissingAncestors: true);
        if (boundary.State != LibraryPlanState.Complete)
        {
            Add(findings, LibraryAttachFindingCode.ConsumerBlocked,
                boundary.State == LibraryPlanState.Blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value, boundary.Path,
                boundary.Cause ?? "The Library consumer-directory boundary is unavailable.");
        }

        var links = EvaluateMappings(input, entries, findings);
        if (LibraryMutationPlanningPolicy.TryFindOwnershipConflict(
            input.Ownership,
            destinationPaths.Concat(RelativePaths(input)),
            out var ownershipConflict))
        {
            Add(findings, LibraryAttachFindingCode.OwnershipConflict, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, ownershipConflict?.Path,
                "The destination is already owned by another managed domain.");
        }

        var state = PlanState(findings);
        if (state != LibraryPlanState.Complete || recordSnapshot is null)
        {
            return Empty(input, state, findings);
        }

        var newLibrary = LibraryRecord.Create(
            input.Request.LibraryId,
            input.Request.SourceRoot,
            [.. entries.Select(entry => entry.SourcePath)]);
        var intendedRecord = LibrariesRecord.Create(
            [.. (currentRecord?.Libraries ?? []).Append(newLibrary)
                .OrderBy(library => library.Id.Value, StringComparer.Ordinal)]);
        var intendedBytes = LibrariesRecordCodec.Write(intendedRecord);
        return new LibraryAttachPlan
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

    private static ImmutableArray<EligibleSourceFile> ReadSource(
        LibraryAttachPlanningInput input,
        ImmutableArray<LibraryAttachFinding>.Builder findings)
    {
        var source = input.Source.Source;
        if (source.Request.Workspace != input.Request.Workspace
            || source.Request.SourceRoot != input.Request.SourceRoot)
        {
            Add(findings, LibraryAttachFindingCode.SourceRootBlocked, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, input.Request.SourceRoot.Value,
                "The source observation does not belong to the requested Library root.");
            return [];
        }

        switch (source.State)
        {
            case LibrarySourceRootState.Available:
                break;
            case LibrarySourceRootState.Missing:
            case LibrarySourceRootState.Invalid:
                Add(findings, LibraryAttachFindingCode.SourceRootInvalid, CliSemanticStatus.Invalid,
                    input.Request.LibraryId.Value, input.Request.SourceRoot.Value,
                    source.Cause ?? "The Library source root is invalid.");
                return [];
            case LibrarySourceRootState.Blocked:
                Add(findings, LibraryAttachFindingCode.SourceRootBlocked, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, input.Request.SourceRoot.Value,
                    source.Cause ?? "The Library source root is unsafe.");
                return [];
            case LibrarySourceRootState.Inaccessible:
            case LibrarySourceRootState.Unavailable:
                Add(findings, LibraryAttachFindingCode.SourceRootUnavailable, CliSemanticStatus.Incomplete,
                    input.Request.LibraryId.Value, input.Request.SourceRoot.Value,
                    source.Cause ?? "The Library source root is unavailable.");
                return [];
            default:
                throw new ArgumentOutOfRangeException(nameof(input), source.State, "The source-root state is not defined.");
        }

        if (input.Source.Inventory is not { } inventory
            || inventory.SourceRoot != input.Request.SourceRoot)
        {
            Add(findings, LibraryAttachFindingCode.InventoryIncomplete, CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value, input.Request.SourceRoot.Value,
                "The complete Library inventory is unavailable.");
            return [];
        }

        if (inventory.State != LibraryInventoryState.Complete)
        {
            Add(findings,
                inventory.State == LibraryInventoryState.Blocked
                    ? LibraryAttachFindingCode.SourceRootBlocked
                    : LibraryAttachFindingCode.InventoryIncomplete,
                inventory.State == LibraryInventoryState.Blocked
                    ? CliSemanticStatus.Blocked
                    : CliSemanticStatus.Incomplete,
                input.Request.LibraryId.Value, input.Request.SourceRoot.Value,
                inventory.Cause ?? "The Library inventory is incomplete.");
        }

        return inventory.Entries;
    }

    private static ImmutableArray<RelativeFileLinkEffect> EvaluateMappings(
        LibraryAttachPlanningInput input,
        ImmutableArray<EligibleSourceFile> entries,
        ImmutableArray<LibraryAttachFinding>.Builder findings)
    {
        var byPath = new Dictionary<string, LibraryMappingObservation>(StringComparer.Ordinal);
        foreach (var observation in input.Mappings)
        {
            if (!byPath.TryAdd(observation.Mapping.SourcePath.Value, observation))
            {
                Add(findings, LibraryAttachFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, observation.LogicalDestinationPath,
                    "The destination was observed more than once.");
            }
        }

        var links = ImmutableArray.CreateBuilder<RelativeFileLinkEffect>();
        foreach (var entry in entries)
        {
            var path = entry.SourcePath.Value;
            if (!byPath.Remove(path, out var observation)
                || !IsExpectedMapping(input, entry, observation))
            {
                Add(findings, LibraryAttachFindingCode.MappingUnavailable, CliSemanticStatus.Incomplete,
                    input.Request.LibraryId.Value, path,
                    "The exact Library mapping was not observed.");
                continue;
            }

            switch (observation.State)
            {
                case LibraryMappingObservationState.Missing:
                    links.Add(RelativeFileLinkEffect.Create(observation.Mapping.DestinationPath.CanonicalPath, observation.ExpectedLink));
                    break;
                case LibraryMappingObservationState.Current:
                case LibraryMappingObservationState.Changed:
                    Add(findings, LibraryAttachFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                        input.Request.LibraryId.Value, path,
                        "Library attach never adopts or replaces an existing destination.");
                    break;
                case LibraryMappingObservationState.Blocked:
                    Add(findings, LibraryAttachFindingCode.MappingBlocked, CliSemanticStatus.Blocked,
                        input.Request.LibraryId.Value, path,
                        observation.Cause ?? "The Library mapping is unsafe.");
                    break;
                case LibraryMappingObservationState.Unavailable:
                    Add(findings, LibraryAttachFindingCode.MappingUnavailable, CliSemanticStatus.Incomplete,
                        input.Request.LibraryId.Value, path,
                        observation.Cause ?? "The Library mapping is unavailable.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input), observation.State, "The mapping state is not defined.");
            }
        }

        foreach (var extra in byPath.Values)
        {
            Add(findings, LibraryAttachFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, extra.LogicalDestinationPath,
                "An unexpected destination mapping was supplied to Library attach.");
        }

        return [.. links.OrderBy(link => link.DestinationPath.Value, StringComparer.Ordinal)];
    }

    private static bool IsExpectedMapping(
        LibraryAttachPlanningInput input,
        EligibleSourceFile entry,
        LibraryMappingObservation observation)
    {
        var expected = LibraryMapping.Create(input.Request.SourceRoot, entry.SourcePath);
        return observation.Mapping.SourcePath == expected.SourcePath
            && observation.Mapping.DestinationPath == expected.DestinationPath
            && observation.Mapping.ExpectedRelativeLink == expected.ExpectedRelativeLink;
    }

    private static IEnumerable<string> RelativePaths(LibraryAttachPlanningInput input)
        => input.GeneratedRegionChanges.Select(change => Path.GetRelativePath(
            input.Request.Workspace.LexicalRoot,
            change.LogicalPath).Replace(Path.DirectorySeparatorChar, '/'));

    private static LibraryPlanState PlanState(IEnumerable<LibraryAttachFinding> findings)
        => findings.Any(finding => finding.Status is CliSemanticStatus.Blocked or CliSemanticStatus.Invalid)
            ? LibraryPlanState.Blocked
            : findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete)
                ? LibraryPlanState.Incomplete
                : LibraryPlanState.Complete;

    private static LibraryAttachPlan Empty(
        LibraryAttachPlanningInput input,
        LibraryPlanState state,
        ImmutableArray<LibraryAttachFinding>.Builder findings)
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

    private static ImmutableArray<LibraryAttachFinding> Order(IEnumerable<LibraryAttachFinding> findings)
        => [.. findings
            .OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.LibraryId, StringComparer.Ordinal)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)];

    private static void Add(
        ImmutableArray<LibraryAttachFinding>.Builder findings,
        LibraryAttachFindingCode code,
        CliSemanticStatus status,
        string? libraryId,
        string? path,
        string cause)
        => findings.Add(new LibraryAttachFinding
        {
            Code = code,
            Status = status,
            LibraryId = libraryId,
            Path = path,
            Cause = cause,
        });
}
