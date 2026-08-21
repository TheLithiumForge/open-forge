using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed class RouteListInventoryReader
{
    private readonly RouteListDirectoryEnumerator _directoryEnumerator = new();
    private readonly RouteListMetadataParser _metadataParser = new();
    private readonly PhysicalPathResolver _physicalPathResolver = new();

    internal async ValueTask<RouteListInventoryFacts> ReadAsync(RouteListInventoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var inventory = new RouteListInventoryAccumulator();
        var directories = new Queue<RouteListDirectoryBoundary>(request.LogicalRoots.Select(
            root => new RouteListDirectoryBoundary(root, [])));

        while (directories.TryDequeue(out var directory))
        {
            if (request.CancellationToken.IsCancellationRequested)
            {
                return FormInterrupted(inventory, directory.CanonicalLogicalPath);
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
                    return FormInterrupted(inventory, directory.CanonicalLogicalPath);
                }

                continue;
            }

            inventory.RegisterIdentity(directory.CanonicalLogicalPath, physical.ResolvedPhysicalPath!);
            if (request.CancellationToken.IsCancellationRequested)
            {
                return FormInterrupted(inventory, directory.CanonicalLogicalPath);
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
                    return FormInterrupted(inventory, directory.CanonicalLogicalPath);
                }

                continue;
            }

            if (rootEntry.State != RouteListFilesystemEntryState.Directory)
            {
                inventory.Findings.Add(RouteListFilesystemFindingPolicy.DirectoryExpected(directory.CanonicalLogicalPath));
                if (request.CancellationToken.IsCancellationRequested)
                {
                    return FormInterrupted(inventory, directory.CanonicalLogicalPath);
                }

                continue;
            }

            if (request.CancellationToken.IsCancellationRequested)
            {
                return FormInterrupted(inventory, directory.CanonicalLogicalPath);
            }

            if (ContainsPhysical(directory.AncestorPhysicalPaths, resolvedPhysicalPath))
            {
                inventory.Findings.Add(
                    RouteListFilesystemFindingPolicy.ContainedDirectoryCycle(directory.CanonicalLogicalPath));
                continue;
            }

            if (request.CancellationToken.IsCancellationRequested)
            {
                return FormInterrupted(inventory, directory.CanonicalLogicalPath);
            }

            var enumeration = _directoryEnumerator.Enumerate(
                resolvedPhysicalPath,
                directory.CanonicalLogicalPath,
                request.CancellationToken);
            var enumerationFinding = RouteListFilesystemFindingPolicy.FromDirectory(enumeration);
            if (enumerationFinding?.Code == RouteListFindingCode.Interrupted)
            {
                return FormInterrupted(inventory, enumerationFinding);
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
                    return FormInterrupted(inventory, entry.CanonicalLogicalPath);
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
                        return FormInterrupted(inventory, entry.CanonicalLogicalPath);
                    }

                    continue;
                }

                inventory.RegisterIdentity(entry.CanonicalLogicalPath, candidate.ResolvedPhysicalPath!);
                if (request.CancellationToken.IsCancellationRequested)
                {
                    return FormInterrupted(inventory, entry.CanonicalLogicalPath);
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
                        return FormInterrupted(inventory, entry.CanonicalLogicalPath);
                    }

                    continue;
                }

                if (request.CancellationToken.IsCancellationRequested)
                {
                    return FormInterrupted(inventory, entry.CanonicalLogicalPath);
                }

                if (candidateEntry.State == RouteListFilesystemEntryState.Directory)
                {
                    directories.Enqueue(new RouteListDirectoryBoundary(entry.CanonicalLogicalPath, childAncestors));
                    continue;
                }

                if (!IsReadableSourceCandidate(entry.CanonicalLogicalPath))
                {
                    continue;
                }

                var interrupted = await ReadFileAsync(request, entry.CanonicalLogicalPath, inventory)
                    .ConfigureAwait(false);
                if (interrupted is not null)
                {
                    return FormInterrupted(inventory, interrupted);
                }
            }
        }

        if (request.CancellationToken.IsCancellationRequested)
        {
            return FormInterrupted(inventory, request.LogicalRoots[^1]);
        }

        return FormFacts(inventory);
    }

    private async ValueTask<RouteListFilesystemFinding?> ReadFileAsync(
        RouteListInventoryRequest request,
        string canonicalLogicalPath,
        RouteListInventoryAccumulator inventory)
    {
        if (request.CancellationToken.IsCancellationRequested)
        {
            return RouteListFilesystemFindingPolicy.Interrupted(canonicalLogicalPath);
        }

        var physical = ResolveCandidate(request.Workspace, canonicalLogicalPath);
        var physicalFinding = RouteListFilesystemFindingPolicy.FromPhysical(canonicalLogicalPath, physical);
        if (physicalFinding is not null)
        {
            inventory.Findings.Add(physicalFinding);
            if (request.CancellationToken.IsCancellationRequested)
            {
                return RouteListFilesystemFindingPolicy.Interrupted(canonicalLogicalPath);
            }

            return null;
        }

        inventory.RegisterIdentity(canonicalLogicalPath, physical.ResolvedPhysicalPath!);
        if (request.CancellationToken.IsCancellationRequested)
        {
            return RouteListFilesystemFindingPolicy.Interrupted(canonicalLogicalPath);
        }

        var resolvedPhysicalPath = physical.ResolvedPhysicalPath!;
        var entry = RouteListFilesystemEntryReader.Read(resolvedPhysicalPath, canonicalLogicalPath);
        var entryFinding = RouteListFilesystemFindingPolicy.FromEntry(entry);
        if (entryFinding is not null)
        {
            inventory.Findings.Add(entryFinding);
            if (request.CancellationToken.IsCancellationRequested)
            {
                return RouteListFilesystemFindingPolicy.Interrupted(canonicalLogicalPath);
            }

            return null;
        }

        if (entry.State != RouteListFilesystemEntryState.File)
        {
            inventory.Findings.Add(RouteListFilesystemFindingPolicy.CandidateMissing(canonicalLogicalPath));
            if (request.CancellationToken.IsCancellationRequested)
            {
                return RouteListFilesystemFindingPolicy.Interrupted(canonicalLogicalPath);
            }

            return null;
        }

        if (request.CancellationToken.IsCancellationRequested)
        {
            return RouteListFilesystemFindingPolicy.Interrupted(canonicalLogicalPath);
        }

        var read = await StrictUtf8FileReader.ReadAsync(
            resolvedPhysicalPath,
            canonicalLogicalPath,
            request.CancellationToken).ConfigureAwait(false);
        inventory.Files.Add(new RouteListInventoryFileFact(
            canonicalLogicalPath,
            resolvedPhysicalPath,
            read.State,
            read.Value));
        var finding = RouteListFilesystemFindingPolicy.FromFile(read);
        if (finding?.Code == RouteListFindingCode.Interrupted)
        {
            return finding;
        }

        if (finding is not null)
        {
            inventory.Findings.Add(finding);
        }

        return null;
    }

    private RouteListInventoryFacts FormFacts(RouteListInventoryAccumulator inventory)
    {
        var sources = FormSources(inventory);
        return RouteListInventoryFacts.Create(sources, inventory.Findings, inventory.Aliases);
    }

    private RouteListInventoryFacts FormInterrupted(
        RouteListInventoryAccumulator inventory,
        string canonicalLogicalSubject)
    {
        return FormInterrupted(
            inventory,
            RouteListFilesystemFindingPolicy.Interrupted(canonicalLogicalSubject));
    }

    private RouteListInventoryFacts FormInterrupted(
        RouteListInventoryAccumulator inventory,
        RouteListFilesystemFinding interruption)
    {
        var sources = FormSources(inventory);
        return RouteListInventoryFacts.Interrupted(
            sources,
            inventory.Findings,
            inventory.Aliases,
            interruption);
    }

    private IReadOnlyList<RouteListInventorySource> FormSources(RouteListInventoryAccumulator inventory)
    {
        var sources = new RouteListInventoryFileFacts(inventory.Files)
            .FormSources(_metadataParser, inventory.Findings);
        inventory.Files.Clear();
        return sources;
    }

    private static bool IsReadableSourceCandidate(string canonicalLogicalPath)
    {
        var fileName = RouteListLogicalPath.ReadFileName(canonicalLogicalPath);
        return string.Equals(canonicalLogicalPath, ".agents/loader.md", StringComparison.Ordinal)
            || string.Equals(fileName, "SKILL.md", StringComparison.Ordinal)
            || fileName.EndsWith(".md", StringComparison.Ordinal);
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
