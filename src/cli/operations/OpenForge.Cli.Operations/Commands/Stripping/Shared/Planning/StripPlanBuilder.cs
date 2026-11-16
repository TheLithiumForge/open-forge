using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Application;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Stripping.Shared.Application;
using OpenForge.Cli.Core.Commands.Stripping.Shared.Documents;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Stripping.Shared.Planning;

/// <summary>
/// Forms one complete, deterministic strip plan from the current workspace bytes.
/// The strip Behavior contract defines the order of reads, edits and effects.
/// </summary>
internal sealed class StripPlanBuilder(PhysicalPathResolver resolver, IStripRemovalSource? removals = null)
{
    private const string CurrentDirectoryPrefix = "./";

    private readonly StripFileReader _files = new(resolver);

    internal async ValueTask<StripPlanBuild> BuildAsync(StripRequest request, CancellationToken token)
    {
        var read = await new WorkspaceLayoutReader(resolver).ReadAsync(request.Workspace, token).ConfigureAwait(false);
        if (read.Layout is not { } layout || read.State is not (WorkspaceLayoutReadState.Ready or WorkspaceLayoutReadState.Uninstalled))
        {
            return Stop(request, CliSemanticStatus.Blocked, StripFindingCode.WorkspaceUnavailable,
                read.Issues.FirstOrDefault()?.Cause ?? read.Settings.Cause ?? "The workspace layout cannot be read safely.");
        }

        string? folder = null;
        if (request.Flatten is { } flatten && (folder = ReadFlattenFolder(layout, flatten)) is null)
        {
            return Stop(request, CliSemanticStatus.Invalid, StripFindingCode.InvalidInput, StripResultFactory.Cannot(request.Command,
                $"'{flatten}' is not an existing folder below the main root, the native Skills folder or a registered outside route."));
        }

        if (request.Command == StripCommandKind.Strip && !read.Ownership.IsTrustworthy && read.Ownership.State != WorkspaceOwnershipReadState.Absent)
        {
            return Stop(request, CliSemanticStatus.Blocked, StripFindingCode.WorkspaceUnavailable,
                read.Ownership.Cause ?? "The ownership lock cannot be read, so strip cannot tell which files Open Forge manages.");
        }

        var inventory = StripDocumentInventory.Read(layout, token);
        var documents = ImmutableArray.CreateBuilder<StripDocument>();
        foreach (var path in inventory.Documents)
        {
            var snapshot = await _files.ReadAsync(request.Workspace, path, token).ConfigureAwait(false);
            if (snapshot is not { Kind: FileExpectationKind.File } file || !StripFileReader.TryDecode(file, out var source))
            {
                return Stop(request, CliSemanticStatus.Blocked, StripFindingCode.DocumentUnsupported,
                    $"'{path}' cannot be read as UTF-8 text.", path);
            }

            documents.Add(new StripDocument(path, file, source));
        }

        var removalSet = removals is null
            ? StripRemovalSet.Empty
            : await removals.ReadAsync(new StripRemovalInput
            {
                Request = request,
                LayoutRead = read,
                Layout = layout,
                Documents = documents.ToImmutable(),
                Read = (path, readToken) => _files.ReadAsync(request.Workspace, path, readToken),
            }, token).ConfigureAwait(false);
        if (removalSet.Stop is { } stop)
        {
            return new StripPlanBuild(StripResultFactory.Stop(request, StatusOf(stop.Code), stop), null);
        }

        var deleted = removalSet.Removals.Where(removal => removal.After is null)
            .Select(removal => removal.Path).ToHashSet(StringComparer.Ordinal);
        // Strip leaves managed files alone so the installed Framework keeps working. Uninstall strips the edited ones it keeps.
        var remaining = documents.Where(document => !deleted.Contains(document.Path)
            && (request.Command == StripCommandKind.Uninstall || !read.Ownership.Document.HasManagedPath(document.Path))).ToArray();
        // Flatten is for categories, subcategories and custom routes. A managed file it would leave below the folder,
        // other than an entrypoint or Skill file that stays in place, means the folder is not the user's to flatten.
        if (folder is not null && ManagedBelow(layout, read, folder, deleted) is { } managedPath)
        {
            return Stop(request, CliSemanticStatus.Blocked, StripFindingCode.FlattenManaged,
                $"{folder} holds {managedPath}, which Open Forge manages. Flatten a category, subcategory or custom route instead. Nothing was changed.",
                managedPath);
        }

        var flattenPlan = folder is null
            ? new StripFlattenPlan(ImmutableDictionary<string, string>.Empty, [])
            : StripFlattenPlanner.Plan(layout, folder, remaining.Select(document => document.Path), deleted);
        if (!flattenPlan.Collisions.IsEmpty)
        {
            // Every collision is its own finding, with its destination and the files that would land there.
            var collisionFindings = flattenPlan.Collisions.Select(collision => StripResultFactory.Error(StripFindingCode.FlattenCollision,
                $"Flattening would place {collision.Count} files at {collision.Path}: {string.Join(", ", collision.Sources)}. Nothing was changed.",
                collision.Path)).ToImmutableArray();
            return new StripPlanBuild(StripResultFactory.Stop(request, CliSemanticStatus.Blocked, collisionFindings, flattenPlan.Collisions), null);
        }

        var lines = new List<PlannedLine>();
        foreach (var removal in removalSet.Removals)
        {
            lines.Add(removal.After is { } after
                ? PlannedLine.Rewrite(removal.Path, removal.Reason, [StripRewritePart.ManagedSection], removal.Before, after)
                : PlannedLine.Delete(removal.Path, removal.Reason, removal.Before));
        }

        var editor = new StripDocumentEditor();
        string Moved(string path) => flattenPlan.Moves.TryGetValue(path, out var to) ? to : path;
        foreach (var document in remaining)
        {
            token.ThrowIfCancellationRequested();
            var rewrite = editor.Edit(document, request, new StripLinkContext
            {
                LexicalRoot = request.Workspace.LexicalRoot,
                Path = document.Path,
                StripLinks = request.Includes(StripPart.Links),
                Moved = Moved,
            });
            if (rewrite.Unsupported is { } cause)
            {
                return Stop(request, CliSemanticStatus.Blocked, StripFindingCode.DocumentUnsupported, $"'{document.Path}': {cause}", document.Path);
            }

            if (flattenPlan.Moves.TryGetValue(document.Path, out var destination))
            {
                lines.Add(PlannedLine.Move(document.Path, destination, rewrite.Parts, document.Snapshot, rewrite.Bytes,
                    _files.LexicalPath(request.Workspace, destination)));
            }
            else if (rewrite.Changed)
            {
                lines.Add(PlannedLine.Rewrite(document.Path, StripPlanReason.Strip, rewrite.Parts, document.Snapshot, rewrite.Bytes));
            }
        }

        var vacated = deleted.Concat(flattenPlan.Moves.Keys).ToHashSet(StringComparer.Ordinal);
        var created = flattenPlan.Moves.Values.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var folders = StripFolderPlanner.Plan(request.Workspace, vacated, created, folder)
            .Select(path => (Path: path, Expectation: _files.ReadDirectory(request.Workspace, path)))
            .Where(item => item.Expectation is not null)
            .ToArray();

        // The runtime folder is the last step: it goes only when it holds nothing but the lease's own files.
        var kept = removalSet.Kept;
        var runtimeFindings = ImmutableArray<StripFinding>.Empty;
        StripPlanEntry? runtime = null;
        if (removalSet.RemovesRuntimeFolder)
        {
            var state = StripRuntimeFolder.Classify(request.Workspace);
            if (state.State is StripRuntimeFolderState.Absent or StripRuntimeFolderState.Removable)
            {
                runtime = new StripPlanEntry { Action = StripPlanAction.DeleteDirectory, Path = StripRuntimeFolder.Path, Reason = StripPlanReason.State };
            }
            else
            {
                kept = kept.Add(new StripKeptFile(StripRuntimeFolder.Path, StripRuntimeFolder.KeptReason(state)));
                runtimeFindings = [StripResultFactory.Info(StripFindingCode.RuntimeRetained, StripRuntimeFolder.Retained(state), StripRuntimeFolder.Path)];
            }
        }

        lines.Sort((left, right) => string.CompareOrdinal(left.Entry.Path, right.Entry.Path));
        var entries = lines.Select(line => line.Entry)
            .Concat(folders.Select(item => new StripPlanEntry { Action = StripPlanAction.DeleteDirectory, Path = item.Path, Reason = StripPlanReason.EmptyFolder }))
            .Concat(runtime is null ? [] : [runtime])
            .ToImmutableArray();
        var effects = new List<StripFileEffect>();
        AddEffects(lines, effects, EffectStage.Create);
        AddEffects(lines, effects, EffectStage.Replace);
        AddEffects(lines, effects, EffectStage.Delete);
        AddEffects(lines, effects, EffectStage.StateDelete);
        var findings = inventory.Linked
            .Select(path => StripResultFactory.Info(StripFindingCode.LinkedFileSkipped, $"'{path}' is a link and was left unchanged.", path))
            .Concat(removalSet.Findings)
            .Concat(runtimeFindings)
            .ToImmutableArray();
        return new StripPlanBuild(null, new StripPlan
        {
            Request = request,
            LayoutRead = read,
            Entries = entries,
            Files = [.. effects],
            Directories = [.. folders.Select((item, index) => new StripDirectoryEffect(item.Expectation!, lines.Count + index))],
            Kept = kept,
            Findings = findings,
            RuntimeFolderEntry = runtime is null ? null : entries.Length - 1,
        });
    }

    private static void AddEffects(List<PlannedLine> lines, List<StripFileEffect> effects, EffectStage stage)
    {
        var ordered = stage == EffectStage.StateDelete
            ? lines.Select((line, index) => (line, index)).OrderBy(item => item.line.IsOwnershipLock ? 1 : 0).ThenBy(item => item.line.Entry.Path, StringComparer.Ordinal)
            : lines.Select((line, index) => (line, index));
        foreach (var (line, index) in ordered)
        {
            foreach (var effect in line.Effects.Where(effect => effect.Stage == stage))
            {
                effects.Add(new StripFileEffect(effect.Change, effect.Before, index));
            }
        }
    }

    private static string? ManagedBelow(WorkspaceLayout layout, WorkspaceLayoutRead read, string folder, IReadOnlySet<string> deleted)
        => read.Ownership.Document.ManagedPaths().Select(owned => owned.Path)
            .Where(path => path.StartsWith(folder + "/", StringComparison.Ordinal) && !deleted.Contains(path)
                && !StripFlattenPlanner.StaysInPlace(layout, path)
                && File.Exists(Path.Combine(layout.Workspace.LexicalRoot, path.Replace('/', Path.DirectorySeparatorChar))))
            .Order(StringComparer.Ordinal)
            .FirstOrDefault();

    private static string? ReadFlattenFolder(WorkspaceLayout layout, string value)
    {
        var path = value.Replace('\\', '/');
        if (path.StartsWith(CurrentDirectoryPrefix, StringComparison.Ordinal))
        {
            path = path[CurrentDirectoryPrefix.Length..];
        }

        path = path.TrimEnd('/');
        if (!PortableWorkspacePath.TryNormalize(path, out var normalized) || normalized != path
            || !layout.InventoryRoots.Any(root => path == root || path.StartsWith(root + "/", StringComparison.Ordinal)))
        {
            return null;
        }

        var directory = new DirectoryInfo(Path.Combine(layout.Workspace.LexicalRoot, path.Replace('/', Path.DirectorySeparatorChar)));
        return directory.Exists && !directory.Attributes.HasFlag(FileAttributes.ReparsePoint) ? path : null;
    }

    private static CliSemanticStatus StatusOf(StripFindingCode code)
        => code == StripFindingCode.InvalidInput ? CliSemanticStatus.Invalid : CliSemanticStatus.Blocked;

    private static StripPlanBuild Stop(StripRequest request, CliSemanticStatus status, StripFindingCode code, string message, string? path = null)
        => new(StripResultFactory.Stop(request, status, StripResultFactory.Error(code, message, path)), null);

    private enum EffectStage
    {
        Create,
        Replace,
        Delete,
        StateDelete,
    }

    private sealed record PlannedEffect(EffectStage Stage, PlannedFileChange Change, FileStateSnapshot Before);

    private sealed record PlannedLine(StripPlanEntry Entry, ImmutableArray<PlannedEffect> Effects)
    {
        internal bool IsOwnershipLock => Entry.Path.EndsWith("/" + WorkspaceLayoutNames.OwnershipFileName, StringComparison.Ordinal);

        internal static PlannedLine Delete(string path, StripPlanReason reason, FileStateSnapshot before)
            => new(new StripPlanEntry { Action = StripPlanAction.Delete, Path = path, Reason = reason, Before = before.ContentHash },
                [new(reason == StripPlanReason.State ? EffectStage.StateDelete : EffectStage.Delete, PlannedFileChange.Delete(before.Expectation), before)]);

        internal static PlannedLine Rewrite(string path, StripPlanReason reason, ImmutableArray<StripRewritePart> parts, FileStateSnapshot before, ImmutableArray<byte> after)
            => new(new StripPlanEntry
            {
                Action = StripPlanAction.Rewrite,
                Path = path,
                Reason = reason,
                Parts = parts,
                Before = before.ContentHash,
                After = FileExpectation.Hash(after.AsSpan()),
            }, [new(EffectStage.Replace, PlannedFileChange.Replace(before.Expectation, after.AsSpan()), before)]);

        internal static PlannedLine Move(string path, string to, ImmutableArray<StripRewritePart> parts, FileStateSnapshot before, ImmutableArray<byte> after, string destination)
            => new(new StripPlanEntry
            {
                Action = StripPlanAction.Move,
                Path = path,
                To = to,
                Reason = StripPlanReason.Flatten,
                Parts = parts,
                Before = before.ContentHash,
                After = FileExpectation.Hash(after.AsSpan()),
            },
            [
                new(EffectStage.Create, PlannedFileChange.Create(FileExpectation.Missing(destination), after.AsSpan()), FileStateSnapshot.Missing(destination)),
                new(EffectStage.Delete, PlannedFileChange.Delete(before.Expectation), before),
            ]);
    }
}
