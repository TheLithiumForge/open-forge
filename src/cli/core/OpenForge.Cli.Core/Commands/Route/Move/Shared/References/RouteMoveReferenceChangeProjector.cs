using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.References;

internal static class RouteMoveReferenceChangeProjector
{
    internal static IReadOnlyDictionary<string, string> BuildMovedPathMap(
        RouteMoveResolvedDestination destination)
    {
        var map = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach (var layer in destination.Subject.Layers)
        {
            map.Add(layer.SourcePath, layer.DestinationPath);
        }

        foreach (var item in destination.Subject.Items)
        {
            if (item.Kind != RouteMoveItemKind.Directory)
            {
                map[item.SourcePath] = item.DestinationPath;
            }
        }

        return map.ToImmutable();
    }

    internal static ImmutableArray<PlannedFileChange> BuildFileChanges(
        RouteMoveResolvedDestination destination,
        IEnumerable<RouteMoveReferenceDocumentPlan> documents)
    {
        var workspace = destination.Inventory.Subject.Request.Workspace;
        var moved = BuildMovedPathMap(destination);
        var bySource = documents.ToDictionary(document => document.SourcePath, StringComparer.Ordinal);
        var changes = new List<PlannedFileChange>();
        foreach (var pair in moved.OrderBy(pair => pair.Value, StringComparer.Ordinal))
        {
            var sourceItem = destination.Inventory.Items.First(item => string.Equals(
                ToCanonical(workspace.LexicalRoot, item.SourcePath),
                pair.Key,
                StringComparison.Ordinal));
            var intendedBytes = bySource.TryGetValue(pair.Key, out var document)
                ? Encoding.UTF8.GetBytes(document.IntendedText)
                : sourceItem.Snapshot.Bytes.ToArray();
            var destinationLogicalPath = Path.Combine(
                workspace.LexicalRoot,
                pair.Value.Replace('/', Path.DirectorySeparatorChar));
            changes.Add(PlannedFileChange.Create(
                FileExpectation.Missing(destinationLogicalPath),
                intendedBytes));
        }

        AddExternalReplacements(changes, documents);
        return changes.OrderBy(change => change.LogicalPath, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    internal static string Apply(
        string source,
        IEnumerable<RouteMoveReferenceReplacement> replacements)
    {
        var result = source;
        foreach (var replacement in replacements.OrderByDescending(value => value.Span.Start))
        {
            result = string.Concat(
                result.AsSpan(0, replacement.Span.Start),
                replacement.Expected,
                result.AsSpan(replacement.Span.End));
        }

        return result;
    }

    private static void AddExternalReplacements(
        ICollection<PlannedFileChange> changes,
        IEnumerable<RouteMoveReferenceDocumentPlan> documents)
    {
        foreach (var document in documents.Where(RequiresExternalReplacement))
        {
            changes.Add(PlannedFileChange.Replace(
                document.Snapshot.Expectation,
                Encoding.UTF8.GetBytes(document.IntendedText)));
        }
    }

    private static bool RequiresExternalReplacement(RouteMoveReferenceDocumentPlan document)
    {
        if (!string.Equals(
            document.SourcePath,
            document.DestinationSourcePath,
            StringComparison.Ordinal))
        {
            return false;
        }

        var intendedHash = FileExpectation.Hash(Encoding.UTF8.GetBytes(document.IntendedText));
        return !string.Equals(
            document.Snapshot.ContentHash,
            intendedHash,
            StringComparison.Ordinal);
    }

    private static string ToCanonical(string workspaceRoot, string logicalPath)
        => Path.GetRelativePath(workspaceRoot, logicalPath)
            .Replace(Path.DirectorySeparatorChar, '/');
}
