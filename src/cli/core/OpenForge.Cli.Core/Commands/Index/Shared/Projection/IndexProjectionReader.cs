using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Projection;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Selection;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Projection;

internal sealed class IndexProjectionReader
{
    private const string CatalogueCancellationCause =
        "Source discovery was cancelled before Index projection could be established.";

    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly IndexSelectionResolver _selectionResolver;
    private readonly IndexProjectionBuilder _projectionBuilder = new();

    internal IndexProjectionReader(PhysicalPathResolver physicalPathResolver)
    {
        _selectionResolver = new IndexSelectionResolver(new SourceReferenceResolver(
            (workspace, canonicalPath) => physicalPathResolver.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, canonicalPath))));
    }

    internal async ValueTask<IndexProjectionReadResult> ReadAsync(
        IndexRequest request,
        CancellationToken cancellationToken)
    {
        var catalogue = await _catalogueReader.ReadAsync(
                new SourceCatalogueRequest(
                    request.Workspace,
                    [SourceLogicalPath.AgentsRoot]),
                cancellationToken)
            .ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            return IndexProjectionReadResult.Cancelled(
                request,
                new IndexFinding(
                    IndexFindingCode.Interrupted,
                    sourceOccurrence: null,
                    source: null,
                    cause: CatalogueCancellationCause,
                    candidates: []));
        }

        var formation = _formationBuilder.Build(catalogue);
        var selection = _selectionResolver.Resolve(request, formation);
        if (!selection.IsComplete)
        {
            return IndexProjectionReadResult.SelectionIncomplete(selection);
        }

        var projection = await _projectionBuilder.BuildAsync(
                new IndexProjectionContext
                {
                    Formation = formation,
                    Selection = selection,
                    Reader = new SourceDocumentReader(request.Workspace),
                },
                cancellationToken)
            .ConfigureAwait(false);
        return IndexProjectionReadResult.Projected(projection);
    }
}
