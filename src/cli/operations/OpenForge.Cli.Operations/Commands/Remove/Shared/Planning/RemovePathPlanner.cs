using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Remove.Shared.Effects;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Mutation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Remove.Shared.Planning;

internal sealed class RemovePathPlanner
{
    private readonly PhysicalPathResolver _resolver;
    private readonly WorkspaceOwnershipStore _ownershipStore;
    private readonly RemovePathInventory _inventory;
    private readonly RemoveNavigationPlanner _navigationPlanner = new();

    internal RemovePathPlanner(
        PhysicalPathResolver resolver,
        WorkspaceOwnershipStore ownershipStore)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(ownershipStore);
        _resolver = resolver;
        _ownershipStore = ownershipStore;
        _inventory = new RemovePathInventory(resolver);
    }

    internal async ValueTask<RemovePathPlanningOutcome> PlanAsync(
        RemovePathRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!PortableWorkspacePath.TryNormalize(request.Path, out var normalized)
            || !string.Equals(normalized, request.Path, StringComparison.Ordinal))
        {
            return Block(request, RemoveFindingCode.InvalidInput, CliSemanticStatus.Invalid,
                "The path target must be one canonical portable workspace-relative path.");
        }

        if (RemovePathSafety.IsProtectedPath(normalized))
        {
            return Block(request, RemoveFindingCode.ProtectedPath, CliSemanticStatus.Blocked,
                "This path contains workspace settings, ownership, Git metadata, or other protected workspace state.");
        }

        if (global::OpenForge.Cli.Core.Framework.Sources.Identity.SourceIdentity.IsRecognizedEntrypointPath(normalized))
        {
            var directory = Path.GetDirectoryName(normalized)?.Replace('\\', '/') ?? normalized;
            return new RemovePathPlanningOutcome.Stopped(RemoveResult.Refused(
                request.Workspace,
                normalized,
                "path",
                RemoveFindingCode.EntryPointRequiresRoute,
                CliSemanticStatus.Blocked,
                "This file is a routed category entrypoint. Select its category directory or use --kind route.",
                $"open-forge remove \"{directory}\" --kind route",
                "Remove the category through its routed entrypoint so companions and navigation are handled together."));
        }

        var settings = await WorkspaceSettingsReader.ReadAsync(_resolver, request.Workspace, cancellationToken).ConfigureAwait(false);
        if (settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete))
        {
            return Block(request, RemoveFindingCode.SettingsUnavailable, CliSemanticStatus.Blocked,
                settings.Cause ?? "Workspace settings could not be safely read.");
        }

        var ownership = await WorkspaceOwnershipReader.ReadAsync(_resolver, request.Workspace, cancellationToken).ConfigureAwait(false);
        if (ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete))
        {
            return Block(request, RemoveFindingCode.OwnershipUnavailable, CliSemanticStatus.Blocked,
                ownership.Cause ?? "Workspace ownership could not be safely read.");
        }

        var registrations = LibraryRegistrationReader.Read(ownership);
        if (registrations.State is not (LibraryRegistrationReadState.Missing or LibraryRegistrationReadState.Complete))
        {
            return Block(request, RemoveFindingCode.OwnershipUnavailable, CliSemanticStatus.Blocked,
                registrations.Cause ?? "Library source mappings could not be interpreted safely.");
        }
        if (RemovePathSafety.IsLibrarySourcePath(normalized, ownership.Document))
        {
            return Block(request, RemoveFindingCode.ProtectedPath, CliSemanticStatus.Blocked,
                "A registered Library source tree is protected from workspace path removal.");
        }

        var alias = RemovePathSafety.FindKnownAlias(normalized, settings.Document, ownership.Document, registrations.Record)
            ?? RemovePathSafety.FindFilesystemAlias(_resolver, request.Workspace, normalized, cancellationToken);
        if (alias is not null && !string.Equals(alias, normalized, StringComparison.Ordinal))
        {
            return new RemovePathPlanningOutcome.Stopped(RemoveResult.Refused(
                request.Workspace,
                normalized,
                "path",
                RemoveFindingCode.TargetUnsafe,
                CliSemanticStatus.Blocked,
                $"The target has a portable case alias. Use the exact spelling '{alias}'.",
                $"open-forge remove \"{alias}\" --kind path",
                "Use the established exact spelling so the stored exclusion matches the workspace catalogue."));
        }

        var logicalTarget = Path.GetFullPath(Path.Combine(
            request.Workspace.LexicalRoot,
            normalized.Replace('/', Path.DirectorySeparatorChar)));
        var observation = NoFollowLeafObserver.Observe(_resolver, request.Workspace, logicalTarget, cancellationToken);
        if (observation.State is not (NoFollowLeafState.Missing or NoFollowLeafState.OrdinaryFile
            or NoFollowLeafState.Directory or NoFollowLeafState.RelativeFileLink))
        {
            return Block(request, RemoveFindingCode.TargetUnsafe, CliSemanticStatus.Blocked,
                observation.Failure?.DirectCause ?? "The selected path is unavailable or is not an ordinary file or directory.");
        }

        var files = new List<FileStateSnapshot>();
        var links = new List<RemovePathLibraryLink>();
        var directories = new List<FileStateSnapshot>();
        if (observation.State == NoFollowLeafState.OrdinaryFile)
        {
            var capture = await _inventory.CaptureFileAsync(request.Workspace, logicalTarget, cancellationToken).ConfigureAwait(false);
            if (capture is RemovePathFileCapture.Captured captured)
            {
                files.Add(captured.Snapshot);
            }
            else if (capture is RemovePathFileCapture.Cancelled)
            {
                return Block(request, RemoveFindingCode.Interrupted, CliSemanticStatus.Interrupted,
                    "Directory and file inventory was cancelled.");
            }
            else if (capture is RemovePathFileCapture.Unavailable unavailable)
            {
                return Block(request, RemoveFindingCode.TargetChanged, CliSemanticStatus.Blocked, unavailable.Cause);
            }
        }
        else if (observation.State == NoFollowLeafState.RelativeFileLink)
        {
            var link = RemovePathInventory.ResolveLibraryLink(normalized, observation, registrations.Record);
            if (link is null)
            {
                return Block(request, RemoveFindingCode.TargetUnsafe, CliSemanticStatus.Blocked,
                    "The selected relative link is not an exact recorded Library destination mapping.");
            }
            links.Add(link);
        }
        else if (observation.State == NoFollowLeafState.Directory)
        {
            var inventory = await _inventory.InventoryDirectoryAsync(
                request.Workspace,
                logicalTarget,
                registrations.Record,
                cancellationToken).ConfigureAwait(false);
            if (inventory is RemovePathDirectoryInventory.Cancelled cancelled)
            {
                return Block(request, RemoveFindingCode.Interrupted, CliSemanticStatus.Interrupted, cancelled.Cause);
            }
            if (inventory is RemovePathDirectoryInventory.Unavailable unavailable)
            {
                return Block(request, RemoveFindingCode.TargetUnsafe, CliSemanticStatus.Blocked, unavailable.Cause);
            }
            var complete = (RemovePathDirectoryInventory.Complete)inventory;
            files.AddRange(complete.Files);
            links.AddRange(complete.Links);
            directories.AddRange(complete.Directories);
        }

        var targetIsDirectory = observation.State == NoFollowLeafState.Directory;
        var recordedDirectoryIntent = observation.State == NoFollowLeafState.Missing
            && settings.Document.RemovedDirectories.Contains(normalized, StringComparer.Ordinal);
        var directoryIntent = targetIsDirectory || recordedDirectoryIntent;
        var removal = directoryIntent
            ? new WorkspaceRemovalSelection { Directories = [normalized] }
            : new WorkspaceRemovalSelection { Files = [normalized] };
        var rawSettingsChange = WorkspaceSettingsChangePlanner.PlanRemovals(settings, removal);
        RemovePathMetadataChange? settingsChange;
        if (rawSettingsChange is null)
        {
            settingsChange = null;
        }
        else if (settings.Snapshot is { } settingsBefore)
        {
            settingsChange = new RemovePathMetadataChange(rawSettingsChange, settingsBefore);
        }
        else
        {
            return Block(request, RemoveFindingCode.SettingsUnavailable, CliSemanticStatus.Blocked,
                "The exact settings bytes required for recovery are unavailable.");
        }

        var selectedPaths = files.Select(file => RelativePath(request.Workspace, file.LogicalPath))
            .Concat(links.Select(link => link.Path))
            .ToHashSet(StringComparer.Ordinal);
        if (targetIsDirectory || observation.State == NoFollowLeafState.Missing)
        {
            selectedPaths.Add(normalized);
        }
        if (directoryIntent)
        {
            foreach (var claimedPath in ReadOwnedContentPaths(ownership.Document)
                .Where(path => RemovePathSafety.IsSameOrDescendant(path, normalized)))
            {
                selectedPaths.Add(claimedPath);
            }
        }

        var libraryPaths = links.Select(link => link.Release).ToList();
        if (registrations.Record is { } registrationSet)
        {
            foreach (var library in registrationSet.Libraries)
            {
                foreach (var mapping in LibraryPathIdentity.Mappings(library))
                {
                    var destination = mapping.DestinationPath.Value;
                    var selected = directoryIntent
                        ? RemovePathSafety.IsSameOrDescendant(destination, normalized)
                        : string.Equals(destination, normalized, StringComparison.Ordinal);
                    if (!selected)
                    {
                        continue;
                    }
                    selectedPaths.Add(destination);
                    libraryPaths.Add(new LibraryPathRelease(library.Id.Value, mapping.SourcePath.Value));
                }
            }
        }

        var orderedSelectedPaths = selectedPaths.Order(StringComparer.Ordinal).ToImmutableArray();
        var navigationOutcome = await _navigationPlanner.BuildAsync(
            request.Workspace,
            orderedSelectedPaths,
            cancellationToken).ConfigureAwait(false);
        if (navigationOutcome is RemoveNavigationPlanningOutcome.Unavailable navigationUnavailable)
        {
            return Block(request, RemoveFindingCode.NavigationUnavailable, CliSemanticStatus.Blocked,
                navigationUnavailable.Cause);
        }
        var navigation = ((RemoveNavigationPlanningOutcome.Available)navigationOutcome).Plan;

        var ownershipPlan = _ownershipStore.PlanContentPathRelease(
            ownership,
            orderedSelectedPaths,
            libraryPaths.Distinct().ToImmutableArray());
        if (ownershipPlan.State == OwnershipWritePlanState.Skipped)
        {
            return Block(request, RemoveFindingCode.OwnershipUnavailable, CliSemanticStatus.Blocked,
                ownershipPlan.Cause ?? "Workspace ownership could not be updated safely.");
        }
        RemovePathMetadataChange? ownershipChange;
        if (ownershipPlan.Change is null)
        {
            ownershipChange = null;
        }
        else if (ownership.Snapshot is { } ownershipBefore)
        {
            ownershipChange = new RemovePathMetadataChange(ownershipPlan.Change, ownershipBefore);
        }
        else
        {
            return Block(request, RemoveFindingCode.OwnershipUnavailable, CliSemanticStatus.Blocked,
                "The exact ownership bytes required for recovery are unavailable.");
        }

        var agentsPath = Path.Combine(request.Workspace.LexicalRoot, ".agents");
        var agentsDirectory = NoFollowLeafObserver.Observe(_resolver, request.Workspace, agentsPath, cancellationToken);
        var agentsCreation = agentsDirectory.State == NoFollowLeafState.Missing
            && (settingsChange is not null || ownershipChange is not null)
                ? PlannedDirectoryCreation.Create(FileStateSnapshot.Missing(agentsPath).Expectation)
                : null;
        if (agentsDirectory.State is not (NoFollowLeafState.Missing or NoFollowLeafState.Directory))
        {
            return Block(request, RemoveFindingCode.SettingsUnavailable, CliSemanticStatus.Blocked,
                "The .agents directory is not an ordinary directory and cannot host removal state.");
        }

        return new RemovePathPlanningOutcome.Planned(new RemovePathPlan(
            Request: request,
            Target: normalized,
            TargetIsDirectory: targetIsDirectory,
            IsMissing: observation.State == NoFollowLeafState.Missing,
            Settings: settings,
            Ownership: ownership,
            SettingsChange: settingsChange,
            OwnershipChange: ownershipChange,
            AgentsCreation: agentsCreation,
            Files: [.. files.OrderBy(file => file.LogicalPath, StringComparer.Ordinal)],
            Links: [.. links.OrderBy(link => link.Path, StringComparer.Ordinal)],
            Directories: [.. directories.OrderByDescending(directory => Depth(directory.LogicalPath))
                .ThenBy(directory => directory.LogicalPath, StringComparer.Ordinal)],
            Navigation: navigation));
    }

    internal RemoveResult Preview(RemovePathPlan plan, bool dryRun)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var effects = new List<RemoveEffect>();
        if (plan.AgentsCreation is { } creation)
        {
            effects.Add(new RemoveEffect(
                RelativePath(plan.Request.Workspace, creation.LogicalPath),
                "directory",
                "create",
                dryRun ? "planned" : "done"));
        }
        effects.AddRange(plan.Files.Select(file => new RemoveEffect(
            RelativePath(plan.Request.Workspace, file.LogicalPath), "file", "delete", dryRun ? "planned" : "done")));
        effects.AddRange(plan.Links.Select(link => new RemoveEffect(
            link.Path, "link", "delete", dryRun ? "planned" : "done")));
        effects.AddRange(plan.Navigation.Changes.Select(change => new RemoveEffect(
            RelativePath(plan.Request.Workspace, change.LogicalPath), "navigation", "update", dryRun ? "planned" : "done")));
        effects.AddRange(plan.Directories.Select(directory => new RemoveEffect(
            RelativePath(plan.Request.Workspace, directory.LogicalPath), "directory", "delete", dryRun ? "planned" : "done")));
        if (plan.SettingsChange is not null)
        {
            effects.Add(new RemoveEffect(
                WorkspaceSettingsDefinitions.RelativePath,
                "setting",
                RemovePathEffectActions.SettingsPersistence(plan.SettingsChange.Change),
                dryRun ? "planned" : "done"));
        }
        if (plan.OwnershipChange is not null)
        {
            effects.Add(new RemoveEffect(WorkspaceOwnershipDefinitions.RelativePath, "record", "release-ownership", dryRun ? "planned" : "done"));
        }
        return RemoveResult.Complete(
            plan.Request.Workspace,
            plan.Target,
            "path",
            dryRun,
            effects,
            dryRun ? [] : plan.Files.Select(file => RelativePath(plan.Request.Workspace, file.LogicalPath))
                .Concat(plan.Links.Select(link => link.Path)),
            recoveryDisposition: "not-required");
    }

    internal RemoveResult Refuse(
        RemovePathPlan plan,
        RemoveFindingCode code,
        CliSemanticStatus status,
        string message,
        string? command = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return RemoveResult.Refused(
            plan.Request.Workspace,
            plan.Target,
            "path",
            code,
            status,
            message,
            command,
            message,
            effects: Preview(plan, dryRun: true).Effects);
    }

    private static IEnumerable<string> ReadOwnedContentPaths(WorkspaceOwnershipDocument ownership)
    {
        if (ownership.Framework is { } framework)
        {
            foreach (var path in framework.Paths)
            {
                yield return path;
            }
            foreach (var region in framework.Regions)
            {
                yield return region.Path;
            }
        }
        foreach (var extension in ownership.Extensions)
        {
            foreach (var path in extension.Paths)
            {
                yield return path;
            }
            foreach (var region in extension.Regions)
            {
                yield return region.Path;
            }
        }
    }

    private static RemovePathPlanningOutcome Block(
        RemovePathRequest request,
        RemoveFindingCode code,
        CliSemanticStatus status,
        string message)
        => new RemovePathPlanningOutcome.Stopped(RemoveResult.Refused(
            request.Workspace,
            request.Path,
            "path",
            code,
            status,
            message,
            dryRun: request.IsDryRun));

    private static string RelativePath(CliWorkspace workspace, string logicalPath)
        => Path.GetRelativePath(workspace.LexicalRoot, logicalPath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');

    private static int Depth(string path)
        => path.Count(character => character is '/' or '\\');
}
