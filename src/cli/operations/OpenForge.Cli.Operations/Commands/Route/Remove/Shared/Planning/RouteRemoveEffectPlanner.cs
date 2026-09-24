using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal static class RouteRemoveEffectPlanner
{
    internal static RouteRemovePlanProjectionInput Build(
        RouteRemoveCategoryInventory inventory,
        RouteRemoveReferencePlan references,
        RouteRemoveNavigationPlan navigation,
        RouteRemovePersistencePlan persistence)
    {
        var projected = references.FileChanges.Concat(navigation.FileChanges)
            .GroupBy(change => change.LogicalPath, StringComparer.Ordinal)
            .Select(group => ComposeChange(
                references,
                navigation,
                group.Key,
                group.ToArray()))
            .ToArray();
        var generated = projected
            .Where(change => change.Kind == PlannedFileChangeKind.ReplaceGeneratedRegion)
            .OrderBy(change => change.LogicalPath, StringComparer.Ordinal);
        var referenceChanges = projected
            .Where(change => change.Kind == PlannedFileChangeKind.Replace)
            .OrderBy(change => change.LogicalPath, StringComparer.Ordinal);
        var deletedFiles = inventory.Items
            .Where(item => item.Kind != Models.Result.RouteRemoveItemKind.Directory)
            .Select(item => PlannedFileChange.Delete(item.Snapshot.Expectation))
            .OrderBy(change => change.LogicalPath, StringComparer.Ordinal);
        var fileChanges = generated
            .Concat(referenceChanges)
            .Concat(deletedFiles)
            .ToImmutableArray();
        var directories = inventory.Items
            .Where(item => item.Kind == Models.Result.RouteRemoveItemKind.Directory)
            .Select(item => PlannedDirectoryDeletion.DeleteIfEmpty(item.Snapshot.Expectation))
            .OrderByDescending(deletion => PathDepth(deletion.LogicalPath))
            .ThenBy(deletion => deletion.LogicalPath, StringComparer.Ordinal)
            .ToImmutableArray();
        return new RouteRemovePlanProjectionInput
        {
            Subject = inventory.Subject,
            Ownership = inventory.Ownership,
            Settings = persistence.Settings,
            RemovalSelection = persistence.Selection,
            ContentPathsToRelease = persistence.ContentPathsToRelease,
            ClaimsToRelease = persistence.ClaimsToRelease,
            SettingsChange = persistence.SettingsChange,
            OwnershipChange = persistence.OwnershipChange,
            References = references,
            Navigation = navigation,
            FileChanges = fileChanges,
            DirectoryDeletions = directories,
            RecoveryTargets = BuildPersistenceRecoveryTargets(
                inventory,
                references,
                navigation,
                fileChanges,
                persistence),
        };
    }

    private static ImmutableArray<RecoveryBundleTarget> BuildPersistenceRecoveryTargets(
        RouteRemoveCategoryInventory inventory,
        RouteRemoveReferencePlan references,
        RouteRemoveNavigationPlan navigation,
        ImmutableArray<PlannedFileChange> changes,
        RouteRemovePersistencePlan persistence)
    {
        var targets = BuildRecoveryTargets(inventory, references, navigation, changes).ToBuilder();
        if (persistence.SettingsRecoveryTarget is { } settingsRecovery)
        {
            targets.Add(settingsRecovery);
        }

        if (persistence.OwnershipRecoveryTarget is { } ownershipRecovery)
        {
            targets.Add(ownershipRecovery);
        }

        return targets.ToImmutable();
    }

    private static PlannedFileChange ComposeChange(
        RouteRemoveReferencePlan references,
        RouteRemoveNavigationPlan navigation,
        string logicalPath,
        IReadOnlyList<PlannedFileChange> changes)
    {
        var reference = references.Documents.SingleOrDefault(document => string.Equals(
            document.Snapshot.LogicalPath,
            logicalPath,
            StringComparison.Ordinal));
        var generated = navigation.DocumentEdits.SingleOrDefault(edit => string.Equals(
            edit.LogicalPath,
            logicalPath,
            StringComparison.Ordinal));
        if (generated is null)
        {
            return changes.Count == 1 && changes[0].Kind == PlannedFileChangeKind.Replace
                ? changes[0]
                : throw new InvalidOperationException(
                    "Route Remove reference projections require one complete-file replacement.");
        }

        var expectedCount = reference is null ? 1 : 2;
        if (changes.Count != expectedCount
            || changes.Count(change => change.Kind == PlannedFileChangeKind.ReplaceGeneratedRegion) != 1
            || changes.Count(change => change.Kind == PlannedFileChangeKind.Replace) != expectedCount - 1)
        {
            throw new InvalidOperationException(
                "Route Remove generated projection composition requires its exact planned changes.");
        }

        var snapshot = reference?.Snapshot ?? generated.Snapshot;
        EnsureSameSnapshot(snapshot, generated.Snapshot);
        var intended = ApplyEdits(snapshot.Bytes.AsSpan(), BuildEdits(reference, generated));
        return PlannedFileChange.ReplaceGeneratedRegion(snapshot.Expectation, intended);
    }

    private static DocumentEdit[] BuildEdits(
        RouteRemoveReferenceDocumentPlan? reference,
        RouteRemoveNavigationDocumentEdit generated)
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
            generated.BeforeBytes.ToArray(),
            generated.ExpectedBytes.ToArray()));
        return edits.OrderBy(edit => edit.Start).ToArray();
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
                    "Route Remove document projections must be exact, disjoint spans from one snapshot.");
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

    private static void EnsureSameSnapshot(FileStateSnapshot expected, FileStateSnapshot actual)
    {
        if (expected.Expectation != actual.Expectation
            || !expected.Bytes.AsSpan().SequenceEqual(actual.Bytes.AsSpan()))
        {
            throw new InvalidOperationException(
                "Route Remove document projections must share one exact source snapshot.");
        }
    }

    private static ImmutableArray<RecoveryBundleTarget> BuildRecoveryTargets(
        RouteRemoveCategoryInventory inventory,
        RouteRemoveReferencePlan references,
        RouteRemoveNavigationPlan navigation,
        IEnumerable<PlannedFileChange> changes)
    {
        var snapshots = inventory.Items
            .Where(item => item.Kind != Models.Result.RouteRemoveItemKind.Directory)
            .ToDictionary(item => item.Snapshot.LogicalPath, item => item.Snapshot, StringComparer.Ordinal);
        foreach (var document in references.Documents)
        {
            snapshots[document.Snapshot.LogicalPath] = document.Snapshot;
        }

        foreach (var edit in navigation.DocumentEdits)
        {
            snapshots[edit.Snapshot.LogicalPath] = edit.Snapshot;
        }

        return changes.Select(change => RecoveryBundleTarget.Create(change, snapshots[change.LogicalPath]))
            .ToImmutableArray();
    }

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
