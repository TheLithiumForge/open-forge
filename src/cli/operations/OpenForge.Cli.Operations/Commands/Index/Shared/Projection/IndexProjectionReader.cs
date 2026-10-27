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
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Projection;

internal sealed class IndexProjectionReader
{
    private const string CatalogueCancellationCause =
        "Source discovery was cancelled before Index projection could be established.";

    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly IndexSelectionResolver _selectionResolver;
    private readonly IndexProjectionBuilder _projectionBuilder = new();
    private readonly PhysicalPathResolver _paths;
    private readonly RoutedMaintenancePlanner _maintenance;
    private readonly SourceReferenceResolver _references;

    internal IndexProjectionReader(PhysicalPathResolver physicalPathResolver, IRoutedMaintenanceRenameEvidence? renameEvidence = null)
    {
        _paths = physicalPathResolver;
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
        var layoutRead = await new WorkspaceLayoutReader(_paths).ReadAsync(request.Workspace, cancellationToken).ConfigureAwait(false);
        if (layoutRead.State is not (WorkspaceLayoutReadState.Ready or WorkspaceLayoutReadState.Uninstalled)
            || layoutRead.Layout is not { } layout)
            return Boundary(request, IndexFindingCode.SourceUnsafe,
                layoutRead.Issues.FirstOrDefault()?.Cause ?? "Workspace layout is unavailable.");
        var ownership = layoutRead.Ownership;
        if (ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete))
            return IndexProjectionReadResult.SelectionIncomplete(new IndexSelectionResolution(
                IndexSelection.NotEstablished(request.HasExplicitSources ? IndexSelectionOrigin.ExplicitSources : IndexSelectionOrigin.AutomaticLoader),
                [], [new IndexFinding(IndexFindingCode.SourceUnsafe, sourceOccurrence: null, source: null,
                    cause: "The route-sharing lock is unreadable or invalid. Restore it before indexing shared navigation.", candidates: [])]));
        var session = await new SourceReadSessionReader(_paths).ReadIncludedAsync(layout, cancellationToken).ConfigureAwait(false);
        var catalogue = session.Catalogue;
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
        var settings = layoutRead.Settings;
        catalogue = sharing.Project(catalogue);
        var formation = _formationBuilder.Build(catalogue);
        var explicitSources = request.SourceReferences.Select(reference => _references.Resolve(reference, catalogue)).ToArray();
        var missingAnchors = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < explicitSources.Length; index++)
        {
            var path = SourceReferenceParser.Parse(request.SourceReferences[index]).AttemptedPath;
            if (path is not null && explicitSources[index].Source is { } resolved && resolved.Identity.CanonicalBasePath != path
                && (SourceLogicalPath.IsOutsideSource(layout, path) || SourceLogicalPath.IsOutsideSource(layout, resolved.Identity.CanonicalBasePath)))
                return Boundary(request, IndexFindingCode.SourceUnsafe, "The selected outside coordinate does not identify its retained source.");
            if (explicitSources[index].State == SourceReferenceResolutionState.Resolved) continue;
            if (path is not null && layout.OutsideRoutes.Any(route => route.CataloguePath == path)
                && layoutRead.OutsideRoutes.Any(route => route.Descriptor.CataloguePath == path && route.State == WorkspaceOutsideRouteReadState.Available))
                missingAnchors.Add(path);
            else return IndexProjectionReadResult.SelectionIncomplete(_selectionResolver.Resolve(request, formation));
        }
        var chosen = explicitSources.Select(resolution => resolution.Source).OfType<SourceLogicalSource>().ToArray();
        if (chosen.Any(source => !SourceLogicalPath.IsPrimarySource(layout, source.Identity.CanonicalBasePath)
            && !SourceLogicalPath.IsOutsideSource(layout, source.Identity.CanonicalBasePath)))
            return Boundary(request, IndexFindingCode.SourceUnsafe, "The selected included source has no registered maintenance boundary.");
        var roots = IndexLoaderRoots.Read(formation).ToList();
        if (request.HasExplicitSources && !chosen.Any(source => source.Base.Form == SourceDocumentForm.Loader))
            roots.RemoveAll(path => SourceLogicalPath.IsOutsideSource(layout, path));
        foreach (var path in chosen.Select(source => source.Identity.CanonicalBasePath).Concat(missingAnchors))
            if (layout.TryGetMaintenanceBoundary(path, out var boundary)
                && boundary.Kind == OpenForge.Cli.Core.Framework.Workspace.Models.Layout.WorkspaceMaintenanceBoundaryKind.Outside)
                roots.Add(boundary.AnchorPath);
        if (roots.Any(path => SourceLogicalPath.IsOutsideSource(layout, path)
            && !layoutRead.OutsideRoutes.Any(route => route.State == WorkspaceOutsideRouteReadState.Available
                && SourceLogicalPath.ReadParent(path) == route.Descriptor.DirectoryPath)))
            return Boundary(request, IndexFindingCode.SourceUnsafe, "A selected outside root is not available for maintenance.");
        var selected = SelectMaintenanceSources(formation, chosen, roots, request.HasExplicitSources)
            .Concat(formation.Sources.Where(source => missingAnchors.Any(anchor => source.Identity.CanonicalBasePath.StartsWith(SourceLogicalPath.ReadParent(anchor) + "/", StringComparison.Ordinal))))
            .Concat((verificationTargets ?? []).SelectMany(path =>
                new[] { path }.Concat(formation.Topology.FindByPath(path)?.ChildPaths ?? []))
                .Select(formation.FindSource).OfType<SourceLogicalSource>())
            .DistinctBy(source => source.Identity.CanonicalBasePath).ToImmutableArray();
        if (chosen.Any(source => SourceLogicalPath.IsOutsideSource(layout, source.Identity.CanonicalBasePath)
            && !layoutRead.OutsideRoutes.Any(route => route.State == WorkspaceOutsideRouteReadState.Available
                && source.Identity.CanonicalBasePath.StartsWith(route.Descriptor.DirectoryPath + "/", StringComparison.Ordinal))))
            return Boundary(request, IndexFindingCode.SourceUnsafe, "The selected outside registration is not available for maintenance.");
        selected = selected.Where(source => !WorkspaceRemovals.IsPathRemoved(layout, source.Identity.CanonicalBasePath, settings.Document)).ToImmutableArray();
        var policy = new RoutedMetadataPolicyContext
        {
            Layout = layout,
            LayoutRead = layoutRead,
            Settings = settings.Document,
            Ownership = ownership.Document,
        };
        var intent = await _maintenance.PrepareAsync(new()
        {
            Catalogue = catalogue,
            RootPaths = [.. roots.Distinct(StringComparer.Ordinal)],
            SelectedSources = selected,
            SelectedSourcePaths = [.. selected.Select(source => source.Identity.CanonicalBasePath)],
            Policy = policy,
        }, cancellationToken).ConfigureAwait(false);
        var admission = new IndexOutsideAdmission(catalogue, policy);
        if (chosen.Any(source => SourceLogicalPath.IsOutsideSource(layout, source.Identity.CanonicalBasePath)
            && !admission.Allows(source, SourceFormClassifier.IsEntrypoint(source.Base.Form)
                ? IndexProjectionDocumentRole.Target : IndexProjectionDocumentRole.Metadata, cancellationToken)))
            return Boundary(request, IndexFindingCode.SourceUnsafe, "The selected outside source was not admitted for maintenance.");
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
                    Reader = session.DocumentReader,
                    Maintenance = intent,
                    Policy = policy,
                },
                cancellationToken)
            .ConfigureAwait(false);
        return IndexProjectionReadResult.Projected(projection with
        {
            LayoutRead = layoutRead,
            Maintenance = intent,
        });
    }

    internal async ValueTask<bool> VerifyMaintenanceAsync(IndexPlan plan, CancellationToken token)
    {
        var expectations = new FileExpectationValidator(_paths.CreateReadSession());
        foreach (var expectation in plan.Input.Projection.ReadExpectations
            .Concat((plan.Input.Projection.Maintenance?.Originals.Values ?? []).Select(snapshot => snapshot.Expectation)))
        {
            var result = await expectations.ValidateAsync(plan.Input.Request.Workspace, expectation, token).ConfigureAwait(false);
            if (result.State != FileExpectationValidationState.Matched) return false;
        }
        return plan.Input.Projection.Maintenance is not { } intent || await _maintenance.VerifyProofsAsync(intent, token).ConfigureAwait(false);
    }

    private static ImmutableArray<SourceLogicalSource> SelectMaintenanceSources(OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation.GeneratedNavigationFormation formation,
        SourceLogicalSource[] selected, IReadOnlyList<string> roots, bool explicitSelection)
    {
        if (!explicitSelection || selected.Any(source => source.Base.Form == SourceDocumentForm.Loader))
            return [.. formation.Sources.Where(source => roots.Any(root => source.Identity.CanonicalBasePath.StartsWith(SourceLogicalPath.ReadParent(root) + "/", StringComparison.Ordinal)))];
        var direct = formation.Sources.Where(source => selected.Any(chosen =>
        {
            var directory = SourceLogicalPath.ReadParent(chosen.Identity.CanonicalBasePath);
            if (OpenForge.Cli.Core.Framework.Sources.Identity.SourceFormClassifier.IsEntrypoint(chosen.Base.Form)
                && source.Identity.CanonicalBasePath.StartsWith(directory + "/", StringComparison.Ordinal)) return true;
            return SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath) == directory;
        })).ToList();
        foreach (var chosen in selected)
        {
            var parent = formation.Sources.Where(source => (!SourceLogicalPath.IsOutsideSource(formation.Catalogue.Layout, chosen.Identity.CanonicalBasePath)
                    || roots.Any(root => source.Identity.CanonicalBasePath.StartsWith(SourceLogicalPath.ReadParent(root) + "/", StringComparison.Ordinal)
                        && chosen.Identity.CanonicalBasePath.StartsWith(SourceLogicalPath.ReadParent(root) + "/", StringComparison.Ordinal)))
                && SourceFormClassifier.IsEntrypoint(source.Base.Form)
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
