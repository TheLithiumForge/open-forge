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
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Projection;

internal sealed class IndexProjectionReader
{
    private const string CatalogueCancellationCause =
        "Source discovery was cancelled before Index projection could be established.";

    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly IndexSelectionResolver _selectionResolver;
    private readonly IndexProjectionBuilder _projectionBuilder = new();
    private readonly PhysicalPathResolver _paths;
    private readonly FileExpectationValidator _expectations;

    internal IndexProjectionReader(PhysicalPathResolver physicalPathResolver)
    {
        _paths = physicalPathResolver;
        _expectations = new(physicalPathResolver);
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
        var ownership = await WorkspaceOwnershipReader.ReadAsync(_paths, request.Workspace, cancellationToken).ConfigureAwait(false);
        if (ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete))
            return IndexProjectionReadResult.SelectionIncomplete(new IndexSelectionResolution(
                IndexSelection.NotEstablished(request.HasExplicitSources ? IndexSelectionOrigin.ExplicitSources : IndexSelectionOrigin.AutomaticLoader),
                [], [new IndexFinding(IndexFindingCode.SourceUnsafe, sourceOccurrence: null, source: null,
                    cause: "The route-sharing lock is unreadable or invalid. Restore it before indexing shared navigation.", candidates: [])]));
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

        var sharing = new SourceSharing(ownership.Document.Framework?.GitIgnoredRoutes ?? []);
        if (sharing.FindUnavailableEntrypoint(catalogue) is { } unavailable)
            return IndexProjectionReadResult.SelectionIncomplete(new IndexSelectionResolution(
                IndexSelection.NotEstablished(request.HasExplicitSources ? IndexSelectionOrigin.ExplicitSources : IndexSelectionOrigin.AutomaticLoader),
                [], [new IndexFinding(IndexFindingCode.SourceUnsafe, sourceOccurrence: null, source: null,
                    cause: $"The recorded shared entrypoint '{unavailable}' is missing or ambiguous. Restore its route before indexing.", candidates: [])]));
        var formation = _formationBuilder.Build(sharing.Project(catalogue));
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
        return IndexProjectionReadResult.Projected(projection with { OwnershipExpectation = ownership.Snapshot?.Expectation });
    }

    internal async ValueTask<bool> VerifyOwnershipAsync(IndexPlan plan, CancellationToken token)
    {
        if (plan.Input.Projection.OwnershipExpectation is not { } expectation) return true;
        var result = await _expectations.ValidateAsync(plan.Input.Request.Workspace, expectation, token).ConfigureAwait(false);
        token.ThrowIfCancellationRequested();
        return result.State == FileExpectationValidationState.Matched;
    }
}
