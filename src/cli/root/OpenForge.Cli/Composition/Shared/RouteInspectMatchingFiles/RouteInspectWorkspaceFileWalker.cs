using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Composition.Shared.RouteInspectMatchingFiles;

internal static class RouteInspectWorkspaceFileWalker
{
    internal static RouteInspectFileEnumeration Enumerate(string root, CancellationToken cancellationToken)
    {
        const RouteInspectMatchingFilesScope scope = RouteInspectMatchingFilesScope.WorkspaceFiles;
        cancellationToken.ThrowIfCancellationRequested();
        var resolver = new PhysicalPathResolver();
        var physicalRoot = resolver.ResolveRoot(root);
        if (physicalRoot.State != PhysicalPathState.Contained)
        {
            return RouteInspectFileEnumeration.Unavailable(scope, RouteInspectMatchingFilesReason.FilesUnavailable);
        }

        var directories = new Stack<(PhysicalPathResolution Directory, ImmutableArray<RouteInspectIgnoreRule> Rules)>();
        directories.Push((physicalRoot, []));
        var paths = new List<string>();
        var skippedLines = 0;
        while (directories.TryPop(out var pending))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var directory = pending.Directory;
            var rules = pending.Rules;
            var ignorePath = Path.Combine(directory.LogicalPath, ".gitignore");
            FileAttributes? ignoreAttributes = null;
            try
            {
                ignoreAttributes = File.GetAttributes(ignorePath);
            }
            catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
            {
                // This directory has no local ignore rules.
            }

            if (ignoreAttributes is { } attributes && (attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) == 0)
            {
                var baseDirectory = Path.GetRelativePath(root, directory.LogicalPath).Replace(Path.DirectorySeparatorChar, '/');
                if (baseDirectory == ".")
                {
                    baseDirectory = string.Empty;
                }

                using var reader = new StreamReader(ignorePath, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: false);
                var lines = new List<string>();
                while (reader.ReadLine() is { } line)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (lines.Count == 0)
                    {
                        line = line.TrimStart('\uFEFF');
                    }

                    lines.Add(line);
                }

                var parsed = RouteInspectIgnoreRules.Parse(baseDirectory, lines, cancellationToken);
                rules = rules.AddRange(parsed.Rules);
                cancellationToken.ThrowIfCancellationRequested();
                skippedLines = checked(skippedLines + parsed.SkippedLines);
            }

            foreach (var entry in Directory.EnumerateFileSystemEntries(directory.LogicalPath))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.Equals(Path.GetFileName(entry), ".git", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var entryAttributes = File.GetAttributes(entry);
                if ((entryAttributes & FileAttributes.ReparsePoint) != 0)
                {
                    continue;
                }

                var relative = Path.GetRelativePath(root, entry).Replace(Path.DirectorySeparatorChar, '/');
                var isDirectory = (entryAttributes & FileAttributes.Directory) != 0;
                if (!RouteInspectIgnoreRules.IsIncluded(rules, relative, isDirectory, cancellationToken))
                {
                    continue;
                }

                if (isDirectory)
                {
                    if (!RouteInspectMatchingFilesScanner.HasGitMarker(entry))
                    {
                        var physical = resolver.ResolveCandidate(directory.LogicalPath, directory.GetContainedPhysicalPath(), entry);
                        if (physical.State != PhysicalPathState.Contained)
                        {
                            return RouteInspectFileEnumeration.Unavailable(scope, physical.State == PhysicalPathState.External
                                ? RouteInspectMatchingFilesReason.UnsafePath
                                : RouteInspectMatchingFilesReason.FilesUnavailable);
                        }

                        directories.Push((physical, rules));
                    }
                }
                else
                {
                    // Each parent is resolved once; ordinary leaves cannot redirect outside that parent.
                    paths.Add(relative);
                }
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        var result = RouteInspectFileEnumeration.Success(scope, paths, skippedLines);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }
}
