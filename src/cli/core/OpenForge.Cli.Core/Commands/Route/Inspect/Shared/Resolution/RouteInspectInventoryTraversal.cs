using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed class RouteInspectInventoryTraversal
{
    private readonly RouteInspectInventoryBoundary _boundary;
    private readonly RouteInspectInventoryFileReader _fileReader = new();

    internal RouteInspectInventoryTraversal(PhysicalPathResolver physicalPathResolver)
    {
        _boundary = new RouteInspectInventoryBoundary(physicalPathResolver);
    }

    internal async ValueTask<RouteInspectInventory> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        var files = new List<RouteInspectInventoryFile>();
        var unsafePaths = new HashSet<string>(StringComparer.Ordinal);
        var boundaryIssues = new List<RouteInspectResolutionIssue>();
        if (cancellationToken.IsCancellationRequested)
        {
            return new RouteInspectInventory(files, unsafePaths, boundaryIssues, true);
        }

        const string agentsRoot = ".agents";
        var root = _boundary.ResolveCandidate(workspace, agentsRoot);
        if (root.State == PhysicalPathState.Missing)
        {
            return new RouteInspectInventory(files, unsafePaths, boundaryIssues, false);
        }

        if (root.State != PhysicalPathState.Contained)
        {
            boundaryIssues.Add(_boundary.CreateBoundaryIssue(agentsRoot, root));
            return new RouteInspectInventory(files, unsafePaths, boundaryIssues, false);
        }

        if (!_boundary.IsDirectory(root.ResolvedPhysicalPath!))
        {
            boundaryIssues.Add(new RouteInspectResolutionIssue(
                RouteInspectResolutionIssueCode.ReadUnavailable,
                agentsRoot,
                "The .agents source boundary is not an accessible directory."));
            return new RouteInspectInventory(files, unsafePaths, boundaryIssues, false);
        }

        var directories = new Queue<(
            string LogicalPath,
            string PhysicalPath,
            IReadOnlyList<string> AncestorPhysicalPaths)>();
        directories.Enqueue((agentsRoot, root.ResolvedPhysicalPath!, [root.ResolvedPhysicalPath!]));
        while (directories.TryDequeue(out var directory))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new RouteInspectInventory(files, unsafePaths, boundaryIssues, true);
            }

            var enumeration = DirectoryEntryEnumerator.Enumerate(
                directory.PhysicalPath,
                directory.LogicalPath,
                cancellationToken);
            if (enumeration.State == DirectoryEnumerationState.Cancelled)
            {
                return new RouteInspectInventory(files, unsafePaths, boundaryIssues, true);
            }

            if (enumeration.State != DirectoryEnumerationState.Complete)
            {
                boundaryIssues.Add(new RouteInspectResolutionIssue(
                    RouteInspectResolutionIssueCode.ReadUnavailable,
                    directory.LogicalPath,
                    "A source directory could not be enumerated."));
                continue;
            }

            var entries = enumeration.Entries!
                .OrderBy(entry => Path.GetFileName(entry), StringComparer.Ordinal)
                .ToArray();
            foreach (var physicalEntry in entries)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return new RouteInspectInventory(files, unsafePaths, boundaryIssues, true);
                }

                var name = Path.GetFileName(physicalEntry);
                if (!RouteLogicalPath.IsCanonicalSegment(name))
                {
                    continue;
                }

                var logicalPath = _boundary.Combine(directory.LogicalPath, name);
                var resolved = _boundary.ResolveCandidate(workspace, logicalPath);
                if (resolved.State != PhysicalPathState.Contained)
                {
                    if (RouteSourceFormClassifier.TryClassify(logicalPath, out var form))
                    {
                        if (form == RouteSourceForm.Loader)
                        {
                            boundaryIssues.Add(_boundary.CreateBoundaryIssue(logicalPath, resolved));
                        }
                        else
                        {
                            unsafePaths.Add(logicalPath);
                        }
                    }

                    continue;
                }

                if (!_boundary.TryReadAttributes(resolved.ResolvedPhysicalPath!, out var attributes))
                {
                    if (RouteSourceFormClassifier.TryClassify(logicalPath, out var form))
                    {
                        if (form == RouteSourceForm.Loader)
                        {
                            boundaryIssues.Add(new RouteInspectResolutionIssue(
                                RouteInspectResolutionIssueCode.ReadUnavailable,
                                logicalPath,
                                "The Loader source boundary could not be read."));
                        }
                        else
                        {
                            unsafePaths.Add(logicalPath);
                        }
                    }

                    continue;
                }

                if ((attributes & FileAttributes.Directory) != 0)
                {
                    if (directory.AncestorPhysicalPaths.Contains(
                            resolved.ResolvedPhysicalPath!,
                            PhysicalIdentityTracker.PathComparer))
                    {
                        continue;
                    }

                    directories.Enqueue((
                        logicalPath,
                        resolved.ResolvedPhysicalPath!,
                        directory.AncestorPhysicalPaths.Append(resolved.ResolvedPhysicalPath!).ToArray()));
                    continue;
                }

                var file = await _fileReader
                    .ReadAsync(resolved.ResolvedPhysicalPath!, logicalPath, cancellationToken)
                    .ConfigureAwait(false);
                if (file is null)
                {
                    continue;
                }

                if (file.ReadState == FileReadState.Cancelled)
                {
                    return new RouteInspectInventory(files, unsafePaths, boundaryIssues, true);
                }

                files.Add(file);
            }
        }

        return new RouteInspectInventory(files, unsafePaths, boundaryIssues, false);
    }
}
