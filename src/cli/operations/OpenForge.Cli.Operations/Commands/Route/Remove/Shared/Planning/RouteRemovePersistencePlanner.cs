using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Ownership;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal static class RouteRemovePersistencePlanner
{
    private static readonly string[] ProtectedPaths =
    [
        ".agents",
        ".git",
        ".agents/open-forge.json",
        ".agents/open-forge.lock.json",
    ];

    internal static RouteRemoveFinding? ValidateObservedState(
        RouteRemoveResolvedSubject subject,
        WorkspaceOwnershipRead ownership,
        WorkspaceSettingsRead settings)
    {
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(ownership);
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || settings.Snapshot is null)
        {
            return Finding(
                RouteRemoveFindingCode.SettingsUnavailable,
                CliSemanticStatus.Blocked,
                WorkspaceSettingsDefinitions.RelativePath,
                settings.Cause ?? "The required workspace settings are unavailable or invalid.");
        }

        if (ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete)
            || RouteOwnershipEvidence.Claims(ownership).Any(claim => !IsCanonicalPath(claim.Path)))
        {
            return Finding(
                RouteRemoveFindingCode.OwnershipUnavailable,
                CliSemanticStatus.Blocked,
                ownership.LogicalPath,
                ownership.Cause ?? "The required workspace ownership record is unavailable, invalid, or is not canonical.");
        }

        var scope = Scope(subject);
        if (scope.Paths.Any(path => !IsCanonicalPath(path)))
        {
            return Finding(
                RouteRemoveFindingCode.SourceUnsafe,
                CliSemanticStatus.Blocked,
                scope.Paths[0],
                "The selected route boundary is not a canonical workspace-relative path.");
        }

        if (scope.Paths.Any(path => ProtectedPaths.Any(protectedPath => IsAtOrBelow(protectedPath, path))))
        {
            return Finding(
                RouteRemoveFindingCode.ProtectedTarget,
                CliSemanticStatus.Blocked,
                scope.Paths.First(path => ProtectedPaths.Any(protectedPath => IsAtOrBelow(protectedPath, path))),
                "The selected route overlaps protected workspace control state.");
        }

        foreach (var library in ownership.Document.Libraries)
        {
            if (!IsCanonicalPath(library.SourceRoot))
            {
                return Finding(
                    RouteRemoveFindingCode.OwnershipUnavailable,
                    CliSemanticStatus.Blocked,
                    ownership.LogicalPath,
                    "A registered Library source root is not canonical, so its protected boundary cannot be established.");
            }

            if (scope.Paths.Any(path => Overlaps(path, library.SourceRoot)))
            {
                return Finding(
                    RouteRemoveFindingCode.ProtectedTarget,
                    CliSemanticStatus.Blocked,
                    library.SourceRoot,
                    "The selected route overlaps a registered Library source tree.");
            }
        }

        return null;
    }

    internal static bool IsOwnershipTrusted(WorkspaceOwnershipRead ownership)
        => (ownership.State is WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete)
            && RouteOwnershipEvidence.Claims(ownership).All(claim => IsCanonicalPath(claim.Path));

    internal static RouteRemovePersistencePlanningResult Build(RouteRemoveCategoryInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        var subject = inventory.Subject;
        var settings = inventory.Settings;
        var ownership = inventory.Ownership;
        var scope = Scope(subject);
        if (scope.Kind == RouteRemoveSubjectKind.Category
            && FindNestedGitMetadataPath(inventory) is { } gitMetadataPath)
        {
            return Stop(
                RouteRemoveFindingCode.ProtectedTarget,
                CliSemanticStatus.Blocked,
                gitMetadataPath,
                "The selected category contains protected nested Git metadata.");
        }

        ImmutableArray<string> filePaths;
        ImmutableArray<string> selectedFiles;
        string? categoryRoot;
        switch (scope.Kind)
        {
            case RouteRemoveSubjectKind.Leaf:
                filePaths = subject.Layers
                    .Select(layer => layer.Layer.CanonicalPath)
                    .ToImmutableArray();
                selectedFiles = filePaths;
                categoryRoot = null;
                break;
            case RouteRemoveSubjectKind.Category:
                filePaths = inventory.Items
                    .Where(item => item.Kind != RouteRemoveItemKind.Directory)
                    .Select(item => ToCanonicalRelativePath(subject.Request.Workspace.LexicalRoot, item.Snapshot.LogicalPath))
                    .ToImmutableArray();
                selectedFiles = [];
                categoryRoot = scope.Paths[0];
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(subject), scope.Kind, "The route removal subject kind is not defined.");
        }

        if (filePaths.Any(path => !IsCanonicalPath(path)))
        {
            return Stop(
                RouteRemoveFindingCode.SourceUnsafe,
                CliSemanticStatus.Blocked,
                scope.Paths[0],
                "The selected route contains a path that is not canonical workspace-relative content.");
        }

        var contentPaths = new HashSet<string>(filePaths, StringComparer.Ordinal);
        foreach (var claim in RouteOwnershipEvidence.Claims(ownership))
        {
            var selectedExact = IsSelected(scope, claim.Path, StringComparison.Ordinal);
            if (!selectedExact && IsSelected(scope, claim.Path, StringComparison.OrdinalIgnoreCase))
            {
                return Stop(
                    RouteRemoveFindingCode.TargetChanged,
                    CliSemanticStatus.Blocked,
                    claim.Path,
                    "A managed claim uses a case alias for selected route content; removal cannot persist a safe exclusion.");
            }

            if (selectedExact)
            {
                contentPaths.Add(claim.Path);
            }
        }

        ImmutableArray<string> rootCategory = categoryRoot is not null
            && string.Equals(SourceLogicalPath.ReadParent(categoryRoot), SourceLogicalPath.AgentsRoot, StringComparison.Ordinal)
            ? ImmutableArray.Create(categoryRoot[(categoryRoot.LastIndexOf('/') + 1)..])
            : ImmutableArray<string>.Empty;
        var removalSelection = new WorkspaceRemovalSelection
        {
            Categories = rootCategory,
            Files = selectedFiles,
            Directories = categoryRoot is null ? [] : [categoryRoot],
            Extensions = [],
            Libraries = [],
        };

        PlannedFileChange? settingsChange;
        try
        {
            settingsChange = WorkspaceSettingsChangePlanner.PlanRemovals(settings, removalSelection);
        }
        catch (InvalidOperationException exception)
        {
            return Stop(
                RouteRemoveFindingCode.SettingsUnavailable,
                CliSemanticStatus.Blocked,
                settings.LogicalPath,
                exception.Message);
        }

        var release = new WorkspaceOwnershipStore().PlanContentPathRelease(
            ownership,
            [.. contentPaths.Order(StringComparer.Ordinal)]);
        if (release.State == OwnershipWritePlanState.Skipped)
        {
            return Stop(
                RouteRemoveFindingCode.OwnershipUnavailable,
                CliSemanticStatus.Blocked,
                ownership.LogicalPath,
                release.Cause ?? "Content ownership could not be released safely.");
        }

        var settingsRecovery = settingsChange is null
            ? null
            : CreateRecoveryTarget(settingsChange, settings.Snapshot
                ?? throw new InvalidOperationException("A planned settings change requires exact prior settings bytes."));
        var ownershipRecovery = release.Change is null
            ? null
            : CreateRecoveryTarget(release.Change, ownership.Snapshot
                ?? throw new InvalidOperationException("A planned ownership release requires exact prior lock bytes."));
        var selectedClaims = RouteOwnershipEvidence.Claims(ownership)
            .Where(claim => contentPaths.Contains(claim.Path, StringComparer.Ordinal))
            .Select(claim => new RouteRemoveOwnershipClaim
            {
                Path = claim.Path,
                Manager = claim.Manager switch
                {
                    OwnedPathManager.Framework => RouteRemoveOwnershipManager.Framework,
                    OwnedPathManager.Extension => RouteRemoveOwnershipManager.Extension,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(claim), claim.Manager, "The ownership manager is not defined."),
                },
                Owner = claim.Owner,
            })
            .ToImmutableArray();

        return new RouteRemovePersistencePlanningResult(
            new RouteRemovePersistencePlan
            {
                Settings = settings,
                Selection = removalSelection,
                Ownership = ownership,
                ContentPathsToRelease = [.. contentPaths.Order(StringComparer.Ordinal)],
                ClaimsToRelease = selectedClaims,
                SettingsChange = settingsChange,
                OwnershipChange = release.Change,
                SettingsRecoveryTarget = settingsRecovery,
                OwnershipRecoveryTarget = ownershipRecovery,
            },
            finding: null);
    }

    private static RouteRemovePersistencePlanningResult Stop(
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string target,
        string cause)
        => new(plan: null, Finding(code, status, target, cause));

    private static RouteRemoveFinding Finding(
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
        => new(code, status, target, cause);

    private static RecoveryBundleTarget CreateRecoveryTarget(
        PlannedFileChange change,
        FileStateSnapshot before)
        => change.Kind switch
        {
            PlannedFileChangeKind.Create => RecoveryBundleTarget.CreateReversible(change, before),
            PlannedFileChangeKind.Replace => RecoveryBundleTarget.Create(change, before),
            _ => throw new ArgumentOutOfRangeException(
                nameof(change), change.Kind, "The planned file change kind is not defined."),
        };

    private static RouteRemovePersistenceScope Scope(RouteRemoveResolvedSubject subject)
        => subject.Kind switch
        {
            RouteRemoveSubjectKind.Leaf => new RouteRemovePersistenceScope(
                RouteRemoveSubjectKind.Leaf,
                subject.Layers.Select(layer => layer.Layer.CanonicalPath).ToImmutableArray()),
            RouteRemoveSubjectKind.Category => new RouteRemovePersistenceScope(
                RouteRemoveSubjectKind.Category,
                [SourceLogicalPath.ReadParent(subject.SelectedSource.Identity.CanonicalBasePath)]),
            _ => throw new ArgumentOutOfRangeException(
                nameof(subject), subject.Kind, "The route removal subject kind is not defined."),
        };

    private static string? FindNestedGitMetadataPath(RouteRemoveCategoryInventory inventory)
    {
        var workspaceRoot = inventory.Subject.Request.Workspace.LexicalRoot;
        foreach (var item in inventory.Items)
        {
            var path = ToCanonicalRelativePath(workspaceRoot, item.Snapshot.LogicalPath);
            var segments = path.Split('/');
            var gitSegmentIndex = Array.FindIndex(
                segments,
                segment => string.Equals(segment, ".git", StringComparison.OrdinalIgnoreCase));
            if (gitSegmentIndex >= 0)
            {
                return string.Join("/", segments.Take(gitSegmentIndex + 1));
            }
        }

        return null;
    }

    private static bool IsSelected(RouteRemovePersistenceScope scope, string path, StringComparison comparison)
    {
        var comparer = comparison switch
        {
            StringComparison.Ordinal => StringComparer.Ordinal,
            StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
            _ => throw new ArgumentOutOfRangeException(
                nameof(comparison), comparison, "The route persistence comparison is not supported."),
        };

        return scope.Kind switch
        {
            RouteRemoveSubjectKind.Leaf => scope.Paths.Contains(path, comparer),
            RouteRemoveSubjectKind.Category => scope.Paths.Any(root => string.Equals(path, root, comparison)
                || path.StartsWith($"{root}/", comparison)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(scope), scope.Kind, "The route persistence subject kind is not defined."),
        };
    }

    private static bool IsCanonicalPath(string path)
        => PortableWorkspacePath.TryNormalize(path, out var normalized)
            && string.Equals(path, normalized, StringComparison.Ordinal);

    private static string ToCanonicalRelativePath(string root, string path)
        => Path.GetRelativePath(root, path)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');

    private static bool Overlaps(string first, string second)
        => IsAtOrBelow(first, second) || IsAtOrBelow(second, first);

    private static bool IsAtOrBelow(string path, string root)
        => string.Equals(path, root, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith($"{root}/", StringComparison.OrdinalIgnoreCase);
}
