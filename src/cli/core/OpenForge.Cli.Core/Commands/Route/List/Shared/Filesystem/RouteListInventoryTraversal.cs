using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed class RouteListInventoryTraversal
{
    private readonly RouteListDirectoryEnumerator _directoryEnumerator = new();
    private readonly PhysicalPathResolver _physicalPathResolver = new();
    private readonly RouteListInventoryFileReader _fileReader = new();

    internal async ValueTask<RouteListFilesystemFinding?> ReadAsync(
        RouteListInventoryRequest request,
        RouteListInventoryAccumulator inventory)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(inventory);
        var directories = new Queue<RouteListDirectoryBoundary>(request.LogicalRoots.Select(
            root => new RouteListDirectoryBoundary(root, [])));

        while (directories.TryDequeue(out var directory))
        {
            if (request.CancellationToken.IsCancellationRequested)
            {
                return RouteListFilesystemFindingPolicy.Interrupted(directory.CanonicalLogicalPath);
            }

            var physical = ResolveCandidate(request.Workspace, directory.CanonicalLogicalPath);
            var physicalFinding = RouteListFilesystemFindingPolicy.FromPhysical(
                directory.CanonicalLogicalPath,
                physical);
            if (physicalFinding is not null)
            {
                inventory.Findings.Add(physicalFinding);
                if (request.CancellationToken.IsCancellationRequested)
                {
                    return RouteListFilesystemFindingPolicy.Interrupted(directory.CanonicalLogicalPath);
                }

                continue;
            }

            inventory.RegisterIdentity(directory.CanonicalLogicalPath, physical.ResolvedPhysicalPath!);
            if (request.CancellationToken.IsCancellationRequested)
            {
                return RouteListFilesystemFindingPolicy.Interrupted(directory.CanonicalLogicalPath);
            }

            var resolvedPhysicalPath = physical.ResolvedPhysicalPath!;
            var rootEntry = RouteListFilesystemEntryReader.Read(
                resolvedPhysicalPath,
                directory.CanonicalLogicalPath);
            var entryFinding = RouteListFilesystemFindingPolicy.FromEntry(rootEntry);
            if (entryFinding is not null)
            {
                inventory.Findings.Add(entryFinding);
                if (request.CancellationToken.IsCancellationRequested)
                {
                    return RouteListFilesystemFindingPolicy.Interrupted(directory.CanonicalLogicalPath);
                }

                continue;
            }

            if (rootEntry.State != RouteListFilesystemEntryState.Directory)
            {
                inventory.Findings.Add(RouteListFilesystemFindingPolicy.DirectoryExpected(directory.CanonicalLogicalPath));
                if (request.CancellationToken.IsCancellationRequested)
                {
                    return RouteListFilesystemFindingPolicy.Interrupted(directory.CanonicalLogicalPath);
                }

                continue;
            }

            if (request.CancellationToken.IsCancellationRequested)
            {
                return RouteListFilesystemFindingPolicy.Interrupted(directory.CanonicalLogicalPath);
            }

            if (ContainsPhysical(directory.AncestorPhysicalPaths, resolvedPhysicalPath))
            {
                inventory.Findings.Add(
                    RouteListFilesystemFindingPolicy.ContainedDirectoryCycle(directory.CanonicalLogicalPath));
                continue;
            }

            if (request.CancellationToken.IsCancellationRequested)
            {
                return RouteListFilesystemFindingPolicy.Interrupted(directory.CanonicalLogicalPath);
            }

            var enumeration = _directoryEnumerator.Enumerate(
                resolvedPhysicalPath,
                directory.CanonicalLogicalPath,
                request.CancellationToken);
            var enumerationFinding = RouteListFilesystemFindingPolicy.FromDirectory(enumeration);
            if (enumerationFinding?.Code == RouteListFindingCode.Interrupted)
            {
                return enumerationFinding;
            }

            if (enumerationFinding is not null)
            {
                inventory.Findings.Add(enumerationFinding);
                continue;
            }

            var childAncestors = directory.AncestorPhysicalPaths
                .Append(resolvedPhysicalPath)
                .ToArray();
            foreach (var entry in enumeration.Entries!)
            {
                if (request.CancellationToken.IsCancellationRequested)
                {
                    return RouteListFilesystemFindingPolicy.Interrupted(entry.CanonicalLogicalPath);
                }

                var candidate = ResolveCandidate(request.Workspace, entry.CanonicalLogicalPath);
                var candidateFinding = RouteListFilesystemFindingPolicy.FromPhysical(
                    entry.CanonicalLogicalPath,
                    candidate);
                if (candidateFinding is not null)
                {
                    inventory.Findings.Add(candidateFinding);
                    if (request.CancellationToken.IsCancellationRequested)
                    {
                        return RouteListFilesystemFindingPolicy.Interrupted(entry.CanonicalLogicalPath);
                    }

                    continue;
                }

                inventory.RegisterIdentity(entry.CanonicalLogicalPath, candidate.ResolvedPhysicalPath!);
                if (request.CancellationToken.IsCancellationRequested)
                {
                    return RouteListFilesystemFindingPolicy.Interrupted(entry.CanonicalLogicalPath);
                }

                var candidatePhysicalPath = candidate.ResolvedPhysicalPath!;
                var candidateEntry = RouteListFilesystemEntryReader.Read(
                    candidatePhysicalPath,
                    entry.CanonicalLogicalPath);
                var candidateEntryFinding = RouteListFilesystemFindingPolicy.FromEntry(candidateEntry);
                if (candidateEntryFinding is not null)
                {
                    inventory.Findings.Add(candidateEntryFinding);
                    if (request.CancellationToken.IsCancellationRequested)
                    {
                        return RouteListFilesystemFindingPolicy.Interrupted(entry.CanonicalLogicalPath);
                    }

                    continue;
                }

                if (request.CancellationToken.IsCancellationRequested)
                {
                    return RouteListFilesystemFindingPolicy.Interrupted(entry.CanonicalLogicalPath);
                }

                if (candidateEntry.State == RouteListFilesystemEntryState.Directory)
                {
                    directories.Enqueue(new RouteListDirectoryBoundary(entry.CanonicalLogicalPath, childAncestors));
                    continue;
                }

                var interrupted = await _fileReader
                    .ReadAsync(request, entry.CanonicalLogicalPath, inventory)
                    .ConfigureAwait(false);
                if (interrupted is not null)
                {
                    return interrupted;
                }
            }
        }

        return request.CancellationToken.IsCancellationRequested
            ? RouteListFilesystemFindingPolicy.Interrupted(request.LogicalRoots[^1])
            : null;
    }

    private PhysicalPathResolution ResolveCandidate(
        CliWorkspace workspace,
        string canonicalLogicalPath)
    {
        return _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            RouteListLogicalPath.ToLexicalPath(
                workspace.LexicalRoot,
                canonicalLogicalPath));
    }

    private static bool ContainsPhysical(IEnumerable<string> paths, string candidate)
    {
        return paths.Contains(candidate, PhysicalIdentityTracker.PathComparer);
    }
}
