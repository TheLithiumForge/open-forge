using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;
using OpenForge.Cli.Core.Commands.Route.Shared.Topology;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Resolution;

internal static class RouteInspectResolutionTestData
{
    internal static RouteInspectGraph Graph(
        IEnumerable<RouteSource> sources,
        IEnumerable<string> loaderRootPaths,
        IEnumerable<RouteOverwriteFact>? additionalOverwriteFacts = null)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(loaderRootPaths);
        var materializedSources = sources.ToArray();
        var overwriteFacts = materializedSources
            .Where(source => source.Overwrite is not null)
            .Select(source => new RouteOverwriteFact(
                RouteOverwriteState.Paired,
                source.Overwrite!,
                [source.CanonicalPath]))
            .Concat(additionalOverwriteFacts ?? [])
            .ToArray();
        var catalogue = new RouteSourceCatalogue(materializedSources, overwriteFacts);
        var topology = new RouteTopologyBuilder().Build(
            materializedSources.Where(source => source.Kind != RouteSourceKind.Loader),
            loaderRootPaths);
        return new RouteInspectGraph(catalogue, topology);
    }

    internal static RouteInspectResolution Resolved(
        RouteInspectGraph graph,
        string selectedPath,
        RouteInspectSelectionMethod selectionMethod = RouteInspectSelectionMethod.AutomaticId,
        RouteInspectRouteState routeState = RouteInspectRouteState.Routed)
    {
        ArgumentNullException.ThrowIfNull(graph);
        var source = graph.Catalogue.FindByPath(selectedPath)
            ?? throw new ArgumentException("The selected test source is absent from the graph.", nameof(selectedPath));
        var selection = Selection(source, selectionMethod);
        var identity = Identity(source, routeState);
        return RouteInspectResolution.Create(
            RouteInspectResolutionState.Resolved,
            selection,
            identity,
            graph,
            []);
    }

    internal static RouteInspectResolution Create(RouteInspectResolutionSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return RouteInspectResolution.Create(spec.State, spec.Selection, spec.Identity, spec.Graph, spec.Issues);
    }

    internal static RouteInspectSelection Selection(
        RouteSource source,
        RouteInspectSelectionMethod selectionMethod)
    {
        return selectionMethod switch
        {
            RouteInspectSelectionMethod.AutomaticId => new RouteInspectSelection(
                RouteInspectReferenceKind.SourceId,
                selectionMethod,
                source.Id,
                []),
            RouteInspectSelectionMethod.ExactPath => new RouteInspectSelection(
                RouteInspectReferenceKind.SourcePath,
                selectionMethod,
                source.CanonicalPath,
                []),
            RouteInspectSelectionMethod.Interactive => new RouteInspectSelection(
                RouteInspectReferenceKind.SourceId,
                selectionMethod,
                source.Id,
                []),
            _ => throw new ArgumentOutOfRangeException(
                nameof(selectionMethod),
                selectionMethod,
                "The fixed selection method is not supported."),
        };
    }

    internal static RouteInspectSelection UnresolvedId(
        string id,
        IEnumerable<string>? candidatePaths = null)
    {
        return new RouteInspectSelection(
            RouteInspectReferenceKind.SourceId,
            RouteInspectSelectionMethod.Unresolved,
            id,
            candidatePaths ?? []);
    }

    internal static RouteInspectSelection MissingReference()
    {
        return new RouteInspectSelection(
            RouteInspectReferenceKind.Missing,
            RouteInspectSelectionMethod.Unresolved,
            null,
            []);
    }

    internal static RouteInspectSelection InvalidReference(string reference)
    {
        return new RouteInspectSelection(
            RouteInspectReferenceKind.Invalid,
            RouteInspectSelectionMethod.Unresolved,
            reference,
            []);
    }

    internal static RouteInspectIdentity Identity(
        RouteSource source,
        RouteInspectRouteState routeState)
    {
        return new RouteInspectIdentity(
            source.Id,
            source.CanonicalPath,
            ReadInspectKind(source.Kind),
            ReadInspectForm(source.Base.Form),
            routeState,
            ReadPhysicalLayers(source));
    }

    internal static RouteInspectRequest Request(string sourceReference)
    {
        return new RouteInspectRequest(Workspace(), sourceReference);
    }

    internal static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "route-inspect-profile-workspace"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory);
    }

    internal static RouteInspectResolutionIssue Issue(
        RouteInspectResolutionIssueCode code,
        string subject,
        IEnumerable<string>? paths = null)
    {
        return new RouteInspectResolutionIssue(
            code,
            subject,
            $"A fixed route-inspect test issue for {subject}.",
            paths);
    }

    private static RouteInspectSourceKind ReadInspectKind(RouteSourceKind kind)
    {
        return kind switch
        {
            RouteSourceKind.Entrypoint => RouteInspectSourceKind.Entrypoint,
            RouteSourceKind.Markdown => RouteInspectSourceKind.Markdown,
            RouteSourceKind.Native => RouteInspectSourceKind.Native,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The fixed source kind is not supported."),
        };
    }

    private static RouteInspectSourceForm ReadInspectForm(RouteSourceForm form)
    {
        return form switch
        {
            RouteSourceForm.CanonicalEntrypoint => RouteInspectSourceForm.CanonicalEntrypoint,
            RouteSourceForm.IndexEntrypoint
                or RouteSourceForm.UnderscoreIndexEntrypoint
                or RouteSourceForm.ReferencesEntrypoint
                or RouteSourceForm.UnderscoreReferencesEntrypoint => RouteInspectSourceForm.CompatibilityEntrypoint,
            RouteSourceForm.Markdown => RouteInspectSourceForm.Markdown,
            RouteSourceForm.Skill => RouteInspectSourceForm.Native,
            _ => throw new ArgumentOutOfRangeException(
                nameof(form),
                form,
                "The fixed source form is not supported."),
        };
    }

    private static IReadOnlyList<RouteInspectPhysicalLayer> ReadPhysicalLayers(RouteSource source)
    {
        var layers = new List<RouteInspectPhysicalLayer>
        {
            new(source.CanonicalPath, source.PhysicalPath, RouteInspectLayerRole.Base),
        };
        if (source.Overwrite is not null)
        {
            layers.Add(new RouteInspectPhysicalLayer(
                source.Overwrite.CanonicalLogicalPath,
                source.Overwrite.PhysicalPath,
                RouteInspectLayerRole.Overwrite));
        }

        return layers;
    }
}

internal sealed record RouteInspectResolutionSpec
{
    internal required RouteInspectResolutionState State { get; init; }

    internal required RouteInspectSelection Selection { get; init; }

    internal RouteInspectIdentity? Identity { get; init; }

    internal RouteInspectGraph? Graph { get; init; }

    internal IEnumerable<RouteInspectResolutionIssue> Issues { get; init; } = [];
}
