using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
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
        if (input.Settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete))
        {
            Add(findings,
                input.Settings.State == WorkspaceSettingsReadState.Invalid
                    ? LibrarySyncFindingCode.PermissionInvalid
                    : LibrarySyncFindingCode.PermissionUnavailable,
                CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value,
                input.Settings.LogicalPath,
                input.Settings.Cause ?? "Workspace settings are not available for a safe Library operation.");
            return Empty(input, LibraryPlanState.Blocked, findings);
        }
        if (input.Ownership.State is WorkspaceOwnershipReadState.Invalid or WorkspaceOwnershipReadState.Unavailable)
        {
            Add(findings,
                input.Ownership.State == WorkspaceOwnershipReadState.Invalid
                    ? LibrarySyncFindingCode.RecordInvalid
                    : LibrarySyncFindingCode.RecordUnavailable,
                CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value,
                input.Ownership.LogicalPath,
                input.Ownership.Cause ?? "The workspace ownership lock is not available for a safe Library operation.");
            return Empty(input, LibraryPlanState.Blocked, findings);
        }
        if (WorkspaceRemovals.IsLibraryRemoved(input.Request.LibraryId.Value, input.Settings.Document))
        {
            Add(findings, LibrarySyncFindingCode.LibraryRemoved, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, input.Settings.LogicalPath,
                "This Library ID is excluded by workspace settings.");
            return Empty(input, LibraryPlanState.Blocked, findings);
        }
        if (input.Record.State != LibraryRegistrationReadState.Malformed
            && input.Record.OwnershipObservation is { } observation)
        {
            Add(findings, LibrarySyncFindingCode.OwnershipObservation, CliSemanticStatus.Complete,
                input.Request.LibraryId.Value, path: null, observation);
            return Empty(input, LibraryPlanState.Complete, findings);
        }
        var selected = ReadRecord(input, findings, out var record);
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
        foreach (var change in input.GeneratedRegionChanges)
        {
            var path = Path.GetRelativePath(input.Request.Workspace.LexicalRoot, change.LogicalPath)
                .Replace(Path.DirectorySeparatorChar, '/');
            if (WorkspaceRemovals.IsPathRemoved(path, input.Settings.Document))
            {
                Add(findings, LibrarySyncFindingCode.GeneratedNavigationBlocked, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, path,
                    "Generated navigation or one of its required ancestors is excluded by workspace settings.");
            }
        }

        var entries = ReadSource(input, selected, findings);
        var currentPaths = entries.Select(entry => entry.SourcePath.Value).ToHashSet(StringComparer.Ordinal);
        var excludedRegisteredPaths = new HashSet<string>(StringComparer.Ordinal);
        if (selected is { } registeredLibrary)
        {
            foreach (var path in registeredLibrary.Paths)
            {
                var destination = LibraryPathIdentity.Map(
                    registeredLibrary.SourceRoot,
                    registeredLibrary.DestinationRoot,
                    path).DestinationPath.Value;
                if (WorkspaceRemovals.IsPathRemoved(destination, input.Settings.Document))
                {
                    excludedRegisteredPaths.Add(path.Value);
                    AddPathExcluded(findings, input.Request.LibraryId.Value, destination,
                        "The excluded registered destination and its ownership claim were kept untouched.");
                }
            }
        }
        var registeredPaths = selected?.Paths.Where(path => !excludedRegisteredPaths.Contains(path.Value))
                .Select(path => path.Value).ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);
        var projection = selected is null ? null : LibraryRegistration.Create(selected.Id, selected.SourceRoot, selected.DestinationRoot,
            [.. currentPaths.Union(registeredPaths).Order(StringComparer.Ordinal).Select(SourceRelativeEligiblePath.Create)]);
        var destinations = projection is null ? [] : LibraryPathIdentity.Mappings(projection)
            .Select(mapping => mapping.DestinationPath.Value).ToImmutableArray();
        if (projection is not null && LibraryDestinationPolicy.FindConflict(input.Request.Workspace, projection, record, RelativePaths(input)) is { } conflict)
        {
            Add(findings, LibrarySyncFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, conflict, "The Library destination is protected, source-owned, or registered to another Library.");
        }

        if (LibraryMutationPlanningPolicy.HasDestinationAlias(input.Request.Workspace, destinations))
        {
            Add(findings, LibrarySyncFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                input.Request.LibraryId.Value, path: null,
                "Two Library destinations identify the same path.");
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
            || record is null)
        {
            return Empty(input, state, findings);
        }

        var replacement = LibraryRegistration.Create(
            selected.Id,
            selected.SourceRoot, selected.DestinationRoot,
            [.. currentPaths.Union(excludedRegisteredPaths).Order(StringComparer.Ordinal).Select(SourceRelativeEligiblePath.Create)]);
        var intendedRecord = LibraryRegistrationSet.Create(
            [.. record.Libraries
                .Select(library => library.Id == selected.Id ? replacement : library)
                .OrderBy(library => library.Id.Value, StringComparer.Ordinal)]);
        var ownershipChange = LibraryMutationPlanningPolicy.CreateOwnershipChange(
            input.Ownership,
            [.. intendedRecord.Libraries.Select(library => new LibraryOwnership(
                library.Id.Value,
                library.SourceRoot.Value,
                library.DestinationRoot.Value,
                [.. library.Paths.Select(path => path.Value)]))]);
        return new LibrarySyncPlan
        {
            Permissions = null,
            Input = input,
            State = LibraryPlanState.Complete,
            Directories = boundary.Directories,
            Links = links,
            GeneratedRegions = OrderGenerated(input.GeneratedRegionChanges),
            OwnershipChange = ownershipChange,
            IntendedRecord = intendedRecord,
            Findings = Order(findings),
        };
    }

    private static LibraryRegistration? ReadRecord(
        LibrarySyncPlanningInput input,
        ImmutableArray<LibrarySyncFinding>.Builder findings,
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
                Add(findings, LibrarySyncFindingCode.UnknownId, CliSemanticStatus.Invalid,
                    input.Request.LibraryId.Value, path: null, "The Library ID is not registered.");
            }

            return selected;
        }

        var (code, status, cause) = input.Record.State switch
        {
            LibraryRegistrationReadState.Unavailable => (
                LibrarySyncFindingCode.RecordUnavailable,
                CliSemanticStatus.Incomplete,
                input.Record.Cause ?? "The Library record is unavailable."),
            LibraryRegistrationReadState.Malformed => (
                LibrarySyncFindingCode.RecordInvalid,
                CliSemanticStatus.Incomplete,
                input.Record.Cause ?? "The Library record is malformed."),
            LibraryRegistrationReadState.Blocked => (
                LibrarySyncFindingCode.RecordBlocked,
                CliSemanticStatus.Blocked,
                input.Record.Cause ?? "The Library record is unsafe."),
            LibraryRegistrationReadState.Missing => (
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
        LibraryRegistration? selected,
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

        var active = ImmutableArray.CreateBuilder<EligibleSourceFile>();
        foreach (var entry in inventory.Entries)
        {
            var destination = LibraryPathIdentity.Map(
                selected.SourceRoot,
                selected.DestinationRoot,
                entry.SourcePath).DestinationPath.Value;
            if (WorkspaceRemovals.IsPathRemoved(destination, input.Settings.Document))
            {
                AddPathExcluded(findings, input.Request.LibraryId.Value, destination,
                    "The mapped destination is excluded by workspace settings and was kept untouched.");
                continue;
            }
            active.Add(entry);
        }
        return active.ToImmutable();
    }

    private static ImmutableArray<RelativeFileLinkEffect> EvaluateMappings(
        LibrarySyncPlanningInput input,
        LibraryRegistration? selected,
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
        var retainedOccupants = new List<string>();
        foreach (var path in currentPaths.Union(registeredPaths).Order(StringComparer.Ordinal))
        {
            var sourcePath = SourceRelativeEligiblePath.Create(path);
            var expected = LibraryMapping.Create(selected.SourceRoot, selected.DestinationRoot, sourcePath);
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
                case LibraryMappingObservationState.Missing when isCurrent && isRegistered:
                    links.Add(RelativeFileLinkEffect.Create(expected.DestinationPath.CanonicalPath, observation.ExpectedLink));
                    Add(findings, LibrarySyncFindingCode.RegisteredLinkRestored, CliSemanticStatus.Attention,
                        input.Request.LibraryId.Value, expected.DestinationPath.Value,
                        "The registered link was missing and was restored.");
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
                    Add(findings, LibrarySyncFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                        input.Request.LibraryId.Value, path,
                        "Library sync never adopts or replaces an unowned destination.");
                    break;
                case LibraryMappingObservationState.Changed when isCurrent && isRegistered
                    && observation.Leaf.State == NoFollowLeafState.OrdinaryFile:
                    retainedOccupants.Add(path);
                    break;
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

        if (links.Count > 0)
        {
            foreach (var path in retainedOccupants)
            {
                Add(findings, LibrarySyncFindingCode.MappingBlocked, CliSemanticStatus.Incomplete,
                    input.Request.LibraryId.Value, path,
                    "The changed ordinary destination was kept; Library sync never replaces it.");
            }
        }
        else
        {
            foreach (var path in retainedOccupants)
            {
                Add(findings, LibrarySyncFindingCode.DestinationCollision, CliSemanticStatus.Blocked,
                    input.Request.LibraryId.Value, path,
                    "Library sync never adopts or replaces an unowned destination.");
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
    {
        if (findings.Any(finding => finding.Status == CliSemanticStatus.Blocked))
        {
            return LibraryPlanState.Blocked;
        }
        return findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete
            && finding.Code != LibrarySyncFindingCode.MappingBlocked)
            ? LibraryPlanState.Incomplete
            : LibraryPlanState.Complete;
    }

    private static LibrarySyncPlan Empty(
        LibrarySyncPlanningInput input,
        LibraryPlanState state,
        ImmutableArray<LibrarySyncFinding>.Builder findings)
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

    private static void AddPathExcluded(
        ImmutableArray<LibrarySyncFinding>.Builder findings,
        string libraryId,
        string destination,
        string cause)
    {
        if (findings.Any(finding => finding.Code == LibrarySyncFindingCode.PathExcluded
            && string.Equals(finding.Path, destination, StringComparison.Ordinal)))
        {
            return;
        }

        Add(findings, LibrarySyncFindingCode.PathExcluded, CliSemanticStatus.Complete,
            libraryId, destination, cause);
    }
}
