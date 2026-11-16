using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Stripping.Shared.Planning;

internal sealed record StripFlattenPlan(ImmutableDictionary<string, string> Moves, ImmutableArray<StripCollision> Collisions);

/// <summary>
/// Moves documents below the flatten folder's subfolders into the folder itself.
/// Entrypoints and native Skill files stay. Names compare without regard to case.
/// </summary>
internal static class StripFlattenPlanner
{
    internal static StripFlattenPlan Plan(
        WorkspaceLayout layout,
        string folder,
        IEnumerable<string> documents,
        IReadOnlySet<string> removed)
    {
        var prefix = folder + "/";
        var moves = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var document in documents)
        {
            if (!document.StartsWith(prefix, StringComparison.Ordinal) || removed.Contains(document))
            {
                continue;
            }

            var relative = document[prefix.Length..];
            if (!relative.Contains('/') || StaysInPlace(layout, document))
            {
                continue;
            }

            moves.Add(document, prefix + relative[(relative.LastIndexOf('/') + 1)..]);
        }

        var staying = ReadDirectFiles(layout, folder)
            .Where(path => !removed.Contains(path) && !moves.ContainsKey(path))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var collisions = moves
            .GroupBy(move => move.Value, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1 || staying.Contains(group.Key))
            .Select(group => new StripCollision(
                staying.FirstOrDefault(path => string.Equals(path, group.Key, StringComparison.OrdinalIgnoreCase)) ?? group.Select(move => move.Value).Min(StringComparer.Ordinal)!,
                [.. group.Select(move => move.Key).Order(StringComparer.Ordinal)],
                staying.Contains(group.Key)))
            .OrderBy(collision => collision.Path, StringComparer.Ordinal)
            .ToImmutableArray();
        return new StripFlattenPlan(moves.ToImmutableDictionary(StringComparer.Ordinal), collisions);
    }

    internal static bool StaysInPlace(WorkspaceLayout layout, string document)
        => SourceFormClassifier.TryClassify(layout, document, out var form)
            && (form == SourceDocumentForm.Skill || SourceFormClassifier.IsEntrypoint(form));

    private static IEnumerable<string> ReadDirectFiles(WorkspaceLayout layout, string folder)
    {
        var directory = new DirectoryInfo(SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, folder));
        return directory.Exists
            ? directory.EnumerateFileSystemInfos().Where(entry => entry is not DirectoryInfo).Select(entry => $"{folder}/{entry.Name}")
            : [];
    }
}
