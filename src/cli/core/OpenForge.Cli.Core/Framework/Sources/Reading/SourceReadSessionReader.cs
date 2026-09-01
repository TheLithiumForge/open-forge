using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Sources.Reading;

internal delegate ValueTask<SourceReadSession> SourceReadSessionRead(
    CliWorkspace workspace,
    CancellationToken cancellationToken);

internal sealed class SourceReadSessionReader
{
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal SourceReadSessionReader(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal async ValueTask<SourceReadSession> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        var documentReader = new SourceDocumentReader(workspace);
        var catalogue = await new SourceCatalogueReader()
            .ReadAsync(
                new SourceCatalogueRequest(workspace, [SourceLogicalPath.AgentsRoot]),
                cancellationToken)
            .ConfigureAwait(false);
        var agentsResolution = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, SourceLogicalPath.AgentsRoot));
        var defaultSelectionScope = agentsResolution.State == PhysicalPathState.Contained
            ? new SourceCatalogueSelectionScope(
                SourceLogicalPath.AgentsRoot,
                agentsResolution.GetContainedPhysicalPath())
            : null;
        return new SourceReadSession(catalogue, documentReader, defaultSelectionScope);
    }
}
