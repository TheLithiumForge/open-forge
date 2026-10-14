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
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Models;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

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
    private readonly RoutedMaintenancePlanner _maintenance;
    private readonly SourceReferenceResolver _references;

    internal IndexProjectionReader(PhysicalPathResolver physicalPathResolver, IRoutedMaintenanceRenameEvidence? renameEvidence = null)
    {
        _paths = physicalPathResolver;
        _expectations = new(physicalPathResolver);
        _maintenance = new(physicalPathResolver, renameEvidence);
        _references = new SourceReferenceResolver(
            (workspace, canonicalPath) => physicalPathResolver.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, canonicalPath)));
        _selectionResolver = new IndexSelectionResolver(_references);
    }

    internal async ValueTask<IndexProjectionReadResult> ReadAsync(
        IndexRequest request,
        CancellationToken cancellationToken,
        IReadOnlyList<string>? verificationTargets = null)
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
        var settings = await WorkspaceSettingsReader.ReadAsync(_paths, request.Workspace, cancellationToken).ConfigureAwait(false);
        if (settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete))
            return Boundary(request, IndexFindingCode.SourceUnsafe, settings.Cause ?? "Workspace settings are unavailable.");
        catalogue = sharing.Project(catalogue);
        var formation = _formationBuilder.Build(catalogue);
        var roots = formation.Loader is null ? [] : formation.Topology.LoaderRootPaths.ToImmutableArray();
        var explicitSources = request.SourceReferences.Select(reference => _references.Resolve(reference, catalogue)).ToArray();
        if (explicitSources.Any(resolution => resolution.State != SourceReferenceResolutionState.Resolved))
            return IndexProjectionReadResult.SelectionIncomplete(_selectionResolver.Resolve(request, formation));
        var selected = SelectMaintenanceSources(formation, request.HasExplicitSources
            ? explicitSources.Select(resolution => resolution.Source ?? throw new InvalidOperationException()).ToArray() : []);
        selected = selected.Where(source => !WorkspaceRemovals.IsPathRemoved(source.Identity.CanonicalBasePath, settings.Document)).ToImmutableArray();
        var intent = await _maintenance.PrepareAsync(new()
        {
            Catalogue = catalogue,
            RootPaths = roots,
            SelectedSources = selected,
            Policy = new()
            {
                Settings = settings.Document,
                Ownership = ownership.Document,
                NativeSkillPaths = [.. catalogue.Sources.Where(source => source.Base.Form == SourceDocumentForm.Skill).Select(source => source.Identity.CanonicalBasePath)]
            },
        }, cancellationToken).ConfigureAwait(false);
        if (intent.Limitations.Any(item => !item.Malformed))
        {
            var limitation = intent.Limitations.First(item => !item.Malformed);
            return Boundary(request, limitation.Topology ? IndexFindingCode.TopologyAmbiguous : IndexFindingCode.MetadataUnsafe, limitation.Cause);
        }
        formation = intent.Formation;
        var selectionFormation = new OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.RouteScaffoldSourceBuilder().Replace(catalogue,
            intent.Entrypoints.Select(item => item.Source), intent.RemovedPaths);
        var selection = _selectionResolver.Resolve(request, selectionFormation);
        if (!selection.IsComplete)
        {
            return IndexProjectionReadResult.SelectionIncomplete(selection);
        }

        if (verificationTargets is not null)
            selection = new(selection.Selection, selection.Targets.Concat(verificationTargets.Select(path => formation.FindSource(path)
                ?? throw new InvalidOperationException("An Index verification target disappeared."))).DistinctBy(source => source.Identity.CanonicalBasePath), []);

        if (intent.Entrypoints.Length > 0 && request.HasExplicitSources)
        {
            var targets = selection.Targets.Concat(intent.Entrypoints.Select(item => item.Source)).ToList();
            foreach (var created in intent.Entrypoints)
            {
                var parent = formation.Topology.FindByPath(created.Source.Identity.CanonicalBasePath)?.ParentPaths.FirstOrDefault();
                if (parent is not null && formation.FindSource(parent) is { } exposing) targets.Add(exposing);
            }
            selection = new(selection.Selection, targets.DistinctBy(source => source.Identity.CanonicalBasePath).OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal), []);
        }

        var projection = await _projectionBuilder.BuildAsync(
                new IndexProjectionContext
                {
                    Formation = formation,
                    Selection = selection,
                    Reader = new SourceDocumentReader(request.Workspace),
                    Maintenance = intent,
                },
                cancellationToken)
            .ConfigureAwait(false);
        return IndexProjectionReadResult.Projected(projection with
        {
            OwnershipExpectation = ownership.Snapshot?.Expectation,
            SettingsExpectation = settings.Snapshot?.Expectation ?? OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files.FileExpectation.Missing(settings.LogicalPath),
            Maintenance = intent,
        });
    }

    internal async ValueTask<bool> VerifyOwnershipAsync(IndexPlan plan, CancellationToken token)
    {
        foreach (var expectation in new[] { plan.Input.Projection.OwnershipExpectation, plan.Input.Projection.SettingsExpectation }.Where(item => item is not null))
        {
            var result = await _expectations.ValidateAsync(plan.Input.Request.Workspace, expectation ?? throw new InvalidOperationException(), token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            if (result.State != FileExpectationValidationState.Matched) return false;
        }
        foreach (var snapshot in plan.Input.Projection.Maintenance?.Originals.Values ?? [])
        {
            var result = await _expectations.ValidateAsync(plan.Input.Request.Workspace, snapshot.Expectation, token).ConfigureAwait(false);
            if (result.State != FileExpectationValidationState.Matched) return false;
        }
        return plan.Input.Projection.Maintenance is not { } intent || await _maintenance.VerifyProofsAsync(intent, token).ConfigureAwait(false);
    }

    private static ImmutableArray<SourceLogicalSource> SelectMaintenanceSources(OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation.GeneratedNavigationFormation formation, SourceLogicalSource[] selected)
    {
        if (selected.Length == 0 || selected.Any(source => source.Base.Form == SourceDocumentForm.Loader))
            return [.. formation.Sources.Where(source => formation.Topology.LoaderRootPaths.Any(root => source.Identity.CanonicalBasePath.StartsWith(SourceLogicalPath.ReadParent(root) + "/", StringComparison.Ordinal)))];
        var direct = formation.Sources.Where(source => selected.Any(chosen =>
        {
            var directory = SourceLogicalPath.ReadParent(chosen.Identity.CanonicalBasePath);
            if (OpenForge.Cli.Core.Framework.Sources.Identity.SourceFormClassifier.IsEntrypoint(chosen.Base.Form)
                && source.Identity.CanonicalBasePath.StartsWith(directory + "/", StringComparison.Ordinal)) return true;
            return SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath) == directory;
        })).ToList();
        foreach (var chosen in selected)
        {
            var parent = formation.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)
                && source.Identity.CanonicalBasePath != chosen.Identity.CanonicalBasePath
                && chosen.Identity.CanonicalBasePath.StartsWith(SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath) + "/", StringComparison.Ordinal))
                .OrderByDescending(source => source.Identity.CanonicalBasePath.Length).FirstOrDefault();
            if (parent is null) continue;
            direct.Add(parent);
            var children = formation.Topology.FindByPath(parent.Identity.CanonicalBasePath)?.ChildPaths ?? [];
            direct.AddRange(children.Select(path => formation.FindSource(path) ?? throw new InvalidOperationException()));
        }
        return [.. direct.DistinctBy(source => source.Identity.CanonicalBasePath)];
    }

    private static IndexProjectionReadResult Boundary(IndexRequest request, IndexFindingCode code, string cause)
        => IndexProjectionReadResult.SelectionIncomplete(new(IndexSelection.NotEstablished(request.HasExplicitSources
            ? IndexSelectionOrigin.ExplicitSources : IndexSelectionOrigin.AutomaticLoader), [],
            [new(code, sourceOccurrence: null, source: null, cause: cause, candidates: [])]));
}
