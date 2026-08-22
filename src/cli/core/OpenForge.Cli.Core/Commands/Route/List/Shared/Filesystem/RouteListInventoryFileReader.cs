using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed class RouteListInventoryFileReader
{
    private readonly PhysicalPathResolver _physicalPathResolver = new();

    internal async ValueTask<RouteListFilesystemFinding?> ReadAsync(
        RouteListInventoryRequest request,
        string canonicalLogicalPath,
        RouteListInventoryAccumulator inventory)
    {
        if (!RouteSourceFormClassifier.TryClassify(canonicalLogicalPath, out var form))
        {
            return null;
        }

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
        inventory.Files.Add(new RouteSourceDocument(
            canonicalLogicalPath,
            resolvedPhysicalPath,
            form,
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
}
