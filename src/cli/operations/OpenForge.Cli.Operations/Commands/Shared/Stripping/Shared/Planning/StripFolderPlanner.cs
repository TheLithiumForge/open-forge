using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Shared.Stripping.Shared.Planning;

/// <summary>
/// Lists the folders a plan leaves empty, deepest first. A folder empties only when
/// every file below it is deleted or moved away and nothing lands in it.
/// </summary>
internal static class StripFolderPlanner
{
    internal static IReadOnlyList<string> Plan(
        CliWorkspace workspace,
        IReadOnlySet<string> vacated,
        IReadOnlySet<string> created,
        string? keep)
    {
        var candidates = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var path in vacated)
        {
            for (var slash = path.LastIndexOf('/'); slash > 0; slash = path.LastIndexOf('/', slash - 1))
            {
                candidates.Add(path[..slash]);
            }
        }

        var removed = new HashSet<string>(StringComparer.Ordinal);
        foreach (var folder in candidates.OrderByDescending(path => path.Count(character => character == '/')).ThenBy(path => path, StringComparer.Ordinal))
        {
            if (keep is not null && (folder == keep || keep.StartsWith(folder + "/", StringComparison.Ordinal)))
            {
                continue;
            }

            if (Empties(workspace, folder, vacated, created, removed))
            {
                removed.Add(folder);
            }
        }

        return [.. candidates.Where(removed.Contains)
            .OrderByDescending(path => path.Count(character => character == '/'))
            .ThenBy(path => path, StringComparer.Ordinal)];
    }

    private static bool Empties(CliWorkspace workspace, string folder, IReadOnlySet<string> vacated, IReadOnlySet<string> created, HashSet<string> removed)
    {
        var directory = new DirectoryInfo(SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, folder));
        if (!directory.Exists || directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            return false;
        }

        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
            var child = $"{folder}/{entry.Name}";
            if (entry.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                return false;
            }

            var stays = entry is DirectoryInfo
                ? !removed.Contains(child)
                : !vacated.Contains(child) || created.Contains(child);
            if (stays)
            {
                return false;
            }
        }

        return true;
    }
}
