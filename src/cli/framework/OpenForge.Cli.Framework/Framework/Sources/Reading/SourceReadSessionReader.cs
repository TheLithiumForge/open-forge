using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Shared.Includes;

namespace OpenForge.Cli.Core.Framework.Sources.Reading;

internal delegate ValueTask<SourceReadSession> SourceReadSessionRead(
    WorkspaceLayout layout,
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
        WorkspaceLayout layout,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(layout);
        var workspace = layout.Workspace;
        var paths = _physicalPathResolver.CreateReadSession();
        var documentReader = new SourceDocumentReader(workspace, paths);
        var catalogue = await new SourceCatalogueReader()
            .ReadSessionAsync(
                new SourceCatalogueRequest(layout, layout.SourceRoots),
                paths,
                cancellationToken)
            .ConfigureAwait(false);
        var scopes = new List<SourceCatalogueSelectionScope>();
        foreach (var root in layout.SourceRoots)
        {
            var resolution = paths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, root));
            if (resolution.State == PhysicalPathState.Contained)
                scopes.Add(new SourceCatalogueSelectionScope(root, resolution.GetContainedPhysicalPath()));
        }
        return new SourceReadSession(catalogue, documentReader, scopes);
    }

    internal async ValueTask<SourceReadSession> ReadIncludedAsync(WorkspaceLayout layout, CancellationToken cancellationToken)
    {
        var session = await ReadAsync(layout, cancellationToken).ConfigureAwait(false);
        var included = await new SourceIncludedEntryReader().ReadEntriesAsync(session.Catalogue, session.DocumentReader, cancellationToken).ConfigureAwait(false);
        return new SourceReadSession(SourceIncludedEntryReader.Extend(session.Catalogue, included.Sources, included.Rows), session.DocumentReader, session.DefaultSelectionScopes);
    }
}
