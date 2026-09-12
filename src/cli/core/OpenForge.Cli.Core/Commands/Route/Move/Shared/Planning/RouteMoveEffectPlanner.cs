using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal static class RouteMoveEffectPlanner
{
    internal static RouteMovePlanProjectionInput Build(
        RouteMoveResolvedDestination destination,
        RouteMoveReferencePlan references,
        RouteMoveNavigationPlan navigation)
    {
        var directories = destination.Inventory.Items
            .Where(item => item.Kind == RouteMoveItemKind.Directory)
            .ToArray();
        ImmutableArray<PlannedDirectoryCreation> creations = [.. directories.Select(item => PlannedDirectoryCreation.Create(
                FileExpectation.Missing(DestinationLogicalPath(destination, item))))
            .OrderBy(creation => PathDepth(creation.LogicalPath))
            .ThenBy(creation => creation.LogicalPath, StringComparer.Ordinal)];
        ImmutableArray<PlannedDirectoryDeletion> deletions = [.. directories.Select(item => PlannedDirectoryDeletion.DeleteIfEmpty(
                item.Snapshot.Expectation))
            .OrderByDescending(deletion => PathDepth(deletion.LogicalPath))
            .ThenBy(deletion => deletion.LogicalPath, StringComparer.Ordinal)];
        var files = BuildFileChanges(destination, references, navigation, out var documents);
        return new RouteMovePlanProjectionInput
        {
            Destination = destination,
            Ownership = destination.Inventory.Ownership,
            References = references,
            Navigation = navigation,
            DirectoryCreations = creations,
            FileChanges = files,
            DirectoryDeletions = deletions,
            RecoveryTargets = BuildRecoveryTargets(
                destination,
                references,
                navigation,
                files,
                documents),
        };
    }

    private static ImmutableArray<PlannedFileChange> BuildFileChanges(
        RouteMoveResolvedDestination destination,
        RouteMoveReferencePlan references,
        RouteMoveNavigationPlan navigation,
        out IReadOnlyDictionary<string, RouteMoveReferenceDocumentPlan>? documents)
    {
        IReadOnlyDictionary<string, RouteMoveReferenceDocumentPlan>? documentMap = null;
        var intended = references.FileChanges.Concat(navigation.FileChanges)
            .GroupBy(change => change.LogicalPath, StringComparer.Ordinal)
            .Select(group => ComposeChange(
                navigation,
                group.Key,
                [.. group],
                documentMap ??= DocumentsByDestination(
                    references,
                    destination.Inventory.Subject.Request.Workspace.LexicalRoot)))
            .ToArray();
        var movedCreates = intended
            .Where(change => change.Kind == PlannedFileChangeKind.Create)
            .OrderBy(change => change.LogicalPath, StringComparer.Ordinal);
        var replacements = intended
            .Where(change => change.Kind != PlannedFileChangeKind.Create)
            .OrderBy(change => change.LogicalPath, StringComparer.Ordinal);
        var sourceDeletes = destination.Inventory.Items
            .Where(item => item.Kind != RouteMoveItemKind.Directory)
            .Select(item => PlannedFileChange.Delete(item.Snapshot.Expectation))
            .OrderByDescending(change => PathDepth(change.LogicalPath))
            .ThenBy(change => change.LogicalPath, StringComparer.Ordinal);
        ImmutableArray<PlannedFileChange> files = [.. movedCreates.Concat(replacements).Concat(sourceDeletes)];
        documents = documentMap;
        return files;
    }

    private static PlannedFileChange ComposeChange(
        RouteMoveNavigationPlan navigation,
        string logicalPath,
        PlannedFileChange[] changes,
        IReadOnlyDictionary<string, RouteMoveReferenceDocumentPlan> documents)
    {
        var reference = documents.GetValueOrDefault(logicalPath);
        var generated = navigation.DocumentEdits.SingleOrDefault(edit => string.Equals(
            edit.DestinationLogicalPath,
            logicalPath,
            StringComparison.Ordinal));
        if (generated is null)
        {
            return changes.Length == 1
                ? changes[0]
                : throw new InvalidOperationException(
                    "Overlapping Route Move file changes require one generated-region projection.");
        }

        var snapshot = reference?.Snapshot ?? generated.Snapshot;
        EnsureSameSnapshot(snapshot, generated.Snapshot);
        var edits = BuildEdits(reference, generated);
        var intended = ApplyEdits(snapshot.Bytes.AsSpan(), edits);
        var create = changes.FirstOrDefault(change => change.Kind == PlannedFileChangeKind.Create);
        return create is not null
            ? PlannedFileChange.Create(create.Expectation, intended)
            : PlannedFileChange.ReplaceGeneratedRegion(snapshot.Expectation, intended);
    }

    private static DocumentEdit[] BuildEdits(
        RouteMoveReferenceDocumentPlan? reference,
        RouteMoveNavigationDocumentEdit generated)
    {
        var edits = new List<DocumentEdit>();
        if (reference is not null)
        {
            edits.AddRange(reference.Edits.Select(edit => new DocumentEdit(
                checked((int)edit.Location.ByteOffset),
                checked((int)edit.Location.ByteLength),
                Encoding.UTF8.GetBytes(edit.Before),
                Encoding.UTF8.GetBytes(edit.Expected))));
        }

        edits.Add(new DocumentEdit(
            checked((int)generated.Location.ByteOffset),
            checked((int)generated.Location.ByteLength),
            [.. generated.BeforeBytes],
            [.. generated.ExpectedBytes]));
        return [.. edits.OrderBy(edit => edit.Start)];
    }

    private static byte[] ApplyEdits(ReadOnlySpan<byte> source, IReadOnlyList<DocumentEdit> edits)
    {
        for (var index = 0; index < edits.Count; index++)
        {
            var edit = edits[index];
            if (edit.Start < 0 || edit.End > source.Length
                || !source.Slice(edit.Start, edit.Length).SequenceEqual(edit.Before)
                || index > 0 && edits[index - 1].End > edit.Start)
            {
                throw new InvalidOperationException(
                    "Route Move document projections must be exact, disjoint spans from one snapshot.");
            }
        }

        var result = source.ToArray();
        foreach (var edit in edits.Reverse())
        {
            var combined = new byte[result.Length - edit.Length + edit.Expected.Length];
            result.AsSpan(0, edit.Start).CopyTo(combined);
            edit.Expected.CopyTo(combined.AsSpan(edit.Start));
            result.AsSpan(edit.End).CopyTo(combined.AsSpan(edit.Start + edit.Expected.Length));
            result = combined;
        }

        return result;
    }

    private static ImmutableArray<RecoveryBundleTarget> BuildRecoveryTargets(
        RouteMoveResolvedDestination destination,
        RouteMoveReferencePlan references,
        RouteMoveNavigationPlan navigation,
        IReadOnlyList<PlannedFileChange> fileChanges,
        IReadOnlyDictionary<string, RouteMoveReferenceDocumentPlan>? documents)
    {
        var workspaceRoot = destination.Inventory.Subject.Request.Workspace.LexicalRoot;
        var snapshots = (documents ?? DocumentsByDestination(references, workspaceRoot))
            .ToDictionary(pair => pair.Key, pair => pair.Value.Snapshot, StringComparer.Ordinal);
        foreach (var edit in navigation.DocumentEdits)
        {
            snapshots[edit.DestinationLogicalPath] = edit.Snapshot;
        }

        foreach (var item in destination.Inventory.Items.Where(item =>
                     item.Kind != RouteMoveItemKind.Directory))
        {
            snapshots[item.Snapshot.LogicalPath] = item.Snapshot;
        }

        return [.. fileChanges.Where(change => change.Kind != PlannedFileChangeKind.Create)
            .Select(change => RecoveryBundleTarget.Create(
                change,
                snapshots[change.LogicalPath]))];
    }

    private static void EnsureSameSnapshot(
        FileStateSnapshot expected,
        FileStateSnapshot actual)
    {
        if (expected.Expectation != actual.Expectation
            || !expected.Bytes.AsSpan().SequenceEqual(actual.Bytes.AsSpan()))
        {
            throw new InvalidOperationException(
                "Route Move document projections must share one exact source snapshot.");
        }
    }

    private static IReadOnlyDictionary<string, RouteMoveReferenceDocumentPlan> DocumentsByDestination(
        RouteMoveReferencePlan plan,
        string workspaceRoot)
        => plan.Documents.ToDictionary(
            document => Path.Combine(
                workspaceRoot,
                document.DestinationSourcePath.Replace('/', Path.DirectorySeparatorChar)),
            StringComparer.Ordinal);

    private static string DestinationLogicalPath(
        RouteMoveResolvedDestination destination,
        RouteMoveInventoryItem item)
    {
        var workspace = destination.Inventory.Subject.Request.Workspace;
        var facts = destination.Subject.Items.Single(subjectItem => string.Equals(
            subjectItem.SourcePath,
            Canonical(workspace.LexicalRoot, item.SourcePath),
            StringComparison.Ordinal));
        return Path.Combine(
            workspace.LexicalRoot,
            facts.DestinationPath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string Canonical(string workspaceRoot, string path)
        => Path.GetRelativePath(workspaceRoot, path)
            .Replace(Path.DirectorySeparatorChar, '/');

    private static int PathDepth(string path)
        => path.Count(character => character == Path.DirectorySeparatorChar
            || character == Path.AltDirectorySeparatorChar);

    private sealed record DocumentEdit(
        int Start,
        int Length,
        byte[] Before,
        byte[] Expected)
    {
        internal int End => Start + Length;
    }
}
