using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Export.Models.Copy;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Commands.Export.Shared.Copy;

/// <summary>
/// Matches the project's files against the shared plan. The export Interface contract's What The
/// Copy Holds section defines which files are listed and what each plan action means for the copy.
/// </summary>
internal static class ExportCopyPlanner
{
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    internal static async ValueTask<ExportCopy> PlanAsync(StripPlan plan, string root, string destination, ExportGitFileList gitFiles, CancellationToken token)
    {
        var inside = Path.GetRelativePath(root, destination).Replace(Path.DirectorySeparatorChar, '/');
        var excluded = inside.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(inside) ? null : inside;
        var listed = await gitFiles(root, token).ConfigureAwait(false) ?? Walk(root, token);
        var files = new SortedSet<string>(StringComparer.Ordinal);
        var linked = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var path in listed)
        {
            token.ThrowIfCancellationRequested();
            if (Within(path, WorkspaceRuntimePaths.DirectoryName) || excluded is not null && Within(path, excluded))
            {
                continue;
            }

            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar)));
            }
            catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
            {
                // Git still lists a tracked file that was deleted from the work tree.
                continue;
            }

            if (attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                linked.Add(path);
            }
            else if (!attributes.HasFlag(FileAttributes.Directory))
            {
                files.Add(path);
            }
        }

        var bytes = plan.Files.Where(effect => effect.Change.HasIntendedBytes)
            .ToDictionary(effect => effect.Entry, effect => effect.Change.IntendedBytes);
        var planned = plan.Entries.Select((entry, index) => (Entry: entry, Index: index))
            .Where(item => item.Entry.Action != StripPlanAction.DeleteDirectory)
            .ToDictionary(item => item.Entry.Path, StringComparer.Ordinal);
        var copied = new List<ExportFile>();
        foreach (var path in files)
        {
            if (!planned.TryGetValue(path, out var item))
            {
                copied.Add(new ExportFile(path, path, null));
                continue;
            }

            switch (item.Entry.Action)
            {
                case StripPlanAction.Rewrite:
                    copied.Add(new ExportFile(path, path, bytes[item.Index]));
                    break;
                case StripPlanAction.Move:
                    copied.Add(new ExportFile(path, item.Entry.To!, bytes[item.Index]));
                    break;
                case StripPlanAction.Delete:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(plan), item.Entry.Action, "The plan action has no meaning in a copy.");
            }
        }

        return new ExportCopy(
            [.. copied.OrderBy(file => file.Target, StringComparer.Ordinal)],
            [.. plan.Entries.Where(entry => entry.Action != StripPlanAction.DeleteDirectory && files.Contains(entry.Path))],
            [.. linked]);
    }

    private static bool Within(string path, string folder)
        => string.Equals(path, folder, PathComparison) || path.StartsWith(folder + "/", PathComparison);

    // Without Git, every file except those under .git. Links are listed so the caller can report them.
    private static List<string> Walk(string root, CancellationToken token)
    {
        var paths = new List<string>();
        var pending = new Stack<(DirectoryInfo Directory, string Prefix)>();
        pending.Push((new DirectoryInfo(root), string.Empty));
        while (pending.TryPop(out var current))
        {
            foreach (var entry in current.Directory.EnumerateFileSystemInfos())
            {
                token.ThrowIfCancellationRequested();
                var path = current.Prefix + entry.Name;
                if (entry is DirectoryInfo directory && !entry.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    if (!string.Equals(entry.Name, WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparison.OrdinalIgnoreCase))
                    {
                        pending.Push((directory, path + "/"));
                    }

                    continue;
                }

                paths.Add(path);
            }
        }

        return paths;
    }
}
