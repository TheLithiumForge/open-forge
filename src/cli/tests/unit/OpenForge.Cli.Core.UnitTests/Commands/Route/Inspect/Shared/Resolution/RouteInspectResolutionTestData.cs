using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Resolution;

internal static class RouteInspectResolutionTestData
{
    internal static RouteInspectGraph Graph(
        IEnumerable<RouteSource> sources,
        IEnumerable<string> loaderRootPaths,
        IEnumerable<RouteOverwriteFact>? additionalOverwriteFacts = null)
    {
        var materializedSources = sources.ToArray();
        var materializedLoaderRootPaths = loaderRootPaths.ToArray();
        var sourcePairs = materializedSources
            .Select(source => (
                RouteSource: source,
                LogicalSource: LogicalSource(source)))
            .ToArray();
        var projections = sourcePairs
            .Select(pair => Projection(pair.RouteSource, pair.LogicalSource))
            .ToArray();
        var overwriteFacts = materializedSources
            .Where(source => source.Overwrite is not null)
            .Select(source =>
            {
                var overwrite = source.Overwrite
                    ?? throw new InvalidOperationException("The fixed source must retain its overwrite layer.");
                return new RouteOverwriteFact(
                    RouteOverwriteState.Paired,
                    overwrite,
                    [source.CanonicalPath]);
            })
            .Concat(additionalOverwriteFacts ?? [])
            .ToArray();
        var projectionSet = new RouteSourceProjectionSet(projections, overwriteFacts);
        var routeSourcePairs = sourcePairs
            .Where(pair => pair.RouteSource.Kind != RouteSourceKind.Loader)
            .ToArray();
        var topology = Topology(routeSourcePairs, materializedLoaderRootPaths);
        var identityCounts = routeSourcePairs
            .GroupBy(pair => pair.LogicalSource.Identity.AutomaticId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        const bool areLoaderRootFactsComplete = true;
        var routeFacts = new SourceRouteFacts(
            topology,
            routeSourcePairs.Select(pair => new SourceRouteFact(
                pair.LogicalSource.Identity,
                ReadRouteState(
                    topology,
                    pair.LogicalSource.Identity.CanonicalBasePath,
                    areLoaderRootFactsComplete),
                identityCounts[pair.LogicalSource.Identity.AutomaticId] == 1)),
            [],
            areLoaderRootFactsComplete,
            isCancelled: false);
        return new RouteInspectGraph(projectionSet, routeFacts);
    }

    internal static RouteInspectResolution Resolved(
        RouteInspectGraph graph,
        string selectedPath,
        RouteInspectSelectionMethod selectionMethod = RouteInspectSelectionMethod.AutomaticId,
        RouteInspectRouteState routeState = RouteInspectRouteState.Routed)
    {
        var source = graph.ProjectionSet.FindByPath(selectedPath)
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
        return new RouteInspectRequest(
            Workspace(),
            sourceReference,
            allowInteractiveSourceSelection: false);
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

    private static RouteInspectSourceForm ReadInspectForm(SourceDocumentForm form)
    {
        return form switch
        {
            SourceDocumentForm.CanonicalEntrypoint => RouteInspectSourceForm.CanonicalEntrypoint,
            SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint => RouteInspectSourceForm.CompatibilityEntrypoint,
            SourceDocumentForm.Markdown => RouteInspectSourceForm.Markdown,
            SourceDocumentForm.Skill => RouteInspectSourceForm.Native,
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

    private static SourceLogicalSource LogicalSource(RouteSource source)
    {
        return new SourceLogicalSource(
            new SourceLogicalIdentity(source.Id, source.CanonicalPath),
            Layer(source.Base),
            source.Overwrite is null ? null : Layer(source.Overwrite));
    }

    private static RouteSourceProjection Projection(
        RouteSource source,
        SourceLogicalSource logicalSource)
    {
        var baseRead = Read(logicalSource.Base, source.Base);
        SourceDocumentReadResult? overwriteRead = null;
        if (source.Overwrite is not null)
        {
            var logicalOverwrite = logicalSource.Overwrite
                ?? throw new InvalidOperationException("The neutral fixture must retain the Route overwrite layer.");
            overwriteRead = Read(logicalOverwrite, source.Overwrite);
        }

        return new RouteSourceProjection(logicalSource, source, baseRead, overwriteRead);
    }

    private static SourceDocumentReadResult Read(
        SourceLayer layer,
        RouteSourceDocument document)
    {
        var verificationState = document.ReadState == FileReadState.Missing
            ? SourceLayerVerificationState.Missing
            : SourceLayerVerificationState.Verified;
        var verification = new SourceLayerVerification(
            layer,
            verificationState,
            verificationState == SourceLayerVerificationState.Verified
                ? layer.PhysicalPath
                : null,
            null);
        var read = document.ReadState switch
        {
            FileReadState.Complete => FileReadResult<string>.Complete(
                layer.CanonicalPath,
                document.Body ?? throw new InvalidOperationException("A complete Route document requires its body.")),
            FileReadState.Missing => FileReadResult<string>.Missing(layer.CanonicalPath),
            FileReadState.Cancelled => FileReadResult<string>.Cancelled(layer.CanonicalPath),
            FileReadState.InvalidEncoding
                or FileReadState.InvalidSyntax
                or FileReadState.AccessDenied
                or FileReadState.InputOutputFailure => FileReadResult<string>.Failed(
                    document.ReadState,
                    layer.CanonicalPath,
                    new FilesystemFailure(
                        ReadFailureKind(document.ReadState),
                        $"Fixed {document.ReadState} Route projection fixture.")),
            _ => throw new ArgumentOutOfRangeException(
                nameof(document),
                document.ReadState,
                "The Route document read state is not defined."),
        };
        return new SourceDocumentReadResult(layer, verification, read);
    }

    private static SourceLayer Layer(RouteSourceDocument document)
    {
        return new SourceLayer(
            document.CanonicalLogicalPath,
            document.PhysicalPath,
            document.Form,
            document.Form == SourceDocumentForm.OverwriteCompanion
                ? SourceLayerKind.Overwrite
                : SourceLayerKind.Base);
    }

    private static SourceRouteTopology Topology(
        IReadOnlyList<(RouteSource RouteSource, SourceLogicalSource LogicalSource)> sourcePairs,
        IReadOnlyList<string> loaderRootPaths)
    {
        var entrypointsByDirectory = sourcePairs
            .Where(pair => pair.RouteSource.Kind == RouteSourceKind.Entrypoint)
            .GroupBy(
                pair => ReadParent(pair.LogicalSource.Identity.CanonicalBasePath),
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(pair => pair.LogicalSource.Identity.CanonicalBasePath, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);
        var admittedPairs = sourcePairs
            .Where(pair => IsAdmitted(pair, entrypointsByDirectory))
            .ToArray();
        var relationships = admittedPairs.ToDictionary(
            pair => pair.LogicalSource.Identity.CanonicalBasePath,
            pair => ReadParentRelationship(pair, entrypointsByDirectory),
            StringComparer.Ordinal);
        var childrenByParent = admittedPairs
            .Select(pair => (
                Path: pair.LogicalSource.Identity.CanonicalBasePath,
                Parent: relationships[pair.LogicalSource.Identity.CanonicalBasePath]))
            .Where(item => item.Parent.State == SourceRouteParentState.Resolved)
            .GroupBy(item => item.Parent.Paths[0], StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.Path).OrderBy(path => path, StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);
        var nodes = admittedPairs.Select(pair =>
        {
            var path = pair.LogicalSource.Identity.CanonicalBasePath;
            var parent = relationships[path];
            return new SourceRouteNode(
                pair.LogicalSource.Identity,
                parent.State,
                parent.Paths,
                childrenByParent.GetValueOrDefault(path) ?? []);
        });
        return new SourceRouteTopology(nodes, loaderRootPaths);
    }

    private static bool IsAdmitted(
        (RouteSource RouteSource, SourceLogicalSource LogicalSource) pair,
        IReadOnlyDictionary<string, (RouteSource RouteSource, SourceLogicalSource LogicalSource)[]> entrypointsByDirectory)
    {
        if (pair.RouteSource.Kind == RouteSourceKind.Entrypoint)
        {
            return true;
        }

        var containingDirectory = ReadParent(pair.LogicalSource.Identity.CanonicalBasePath);
        var representedParentDirectory = pair.RouteSource.Kind == RouteSourceKind.Markdown
            ? containingDirectory
            : ReadParent(containingDirectory);
        return entrypointsByDirectory.ContainsKey(representedParentDirectory);
    }

    private static (SourceRouteParentState State, string[] Paths) ReadParentRelationship(
        (RouteSource RouteSource, SourceLogicalSource LogicalSource) pair,
        IReadOnlyDictionary<string, (RouteSource RouteSource, SourceLogicalSource LogicalSource)[]> entrypointsByDirectory)
    {
        var containingDirectory = ReadParent(pair.LogicalSource.Identity.CanonicalBasePath);
        var representedParentDirectory = pair.RouteSource.Kind == RouteSourceKind.Markdown
            ? containingDirectory
            : ReadParent(containingDirectory);
        if (!entrypointsByDirectory.TryGetValue(representedParentDirectory, out var candidates))
        {
            return (SourceRouteParentState.None, []);
        }

        var candidatePaths = candidates
            .Select(candidate => candidate.LogicalSource.Identity.CanonicalBasePath)
            .Where(candidatePath => !string.Equals(
                candidatePath,
                pair.LogicalSource.Identity.CanonicalBasePath,
                StringComparison.Ordinal))
            .ToArray();
        return candidatePaths.Length switch
        {
            0 => (SourceRouteParentState.None, []),
            1 => (SourceRouteParentState.Resolved, candidatePaths),
            _ => (SourceRouteParentState.Ambiguous, candidatePaths),
        };
    }

    private static SourceRouteState ReadRouteState(
        SourceRouteTopology topology,
        string canonicalPath,
        bool areLoaderRootFactsComplete)
    {
        var nodesByPath = topology.Nodes.ToDictionary(
            node => node.Identity.CanonicalBasePath,
            StringComparer.Ordinal);
        if (!nodesByPath.TryGetValue(canonicalPath, out var current))
        {
            return areLoaderRootFactsComplete
                ? SourceRouteState.Unrouted
                : SourceRouteState.Unavailable;
        }

        while (true)
        {
            if (topology.LoaderRootPaths.Contains(current.Identity.CanonicalBasePath, StringComparer.Ordinal))
            {
                return SourceRouteState.Routed;
            }

            if (current.ParentState == SourceRouteParentState.Ambiguous)
            {
                return SourceRouteState.Ambiguous;
            }

            if (current.ParentState == SourceRouteParentState.None)
            {
                return areLoaderRootFactsComplete
                    ? SourceRouteState.Unrouted
                    : SourceRouteState.Unavailable;
            }

            current = nodesByPath[current.ParentPaths[0]];
        }
    }

    private static string ReadParent(string canonicalPath)
    {
        var separatorIndex = canonicalPath.LastIndexOf('/');
        if (separatorIndex <= 0)
        {
            throw new ArgumentException("The fixed source path has no canonical parent.", nameof(canonicalPath));
        }

        return canonicalPath[..separatorIndex];
    }

    private static FilesystemFailureKind ReadFailureKind(FileReadState state)
    {
        return state switch
        {
            FileReadState.InvalidEncoding => FilesystemFailureKind.InvalidEncoding,
            FileReadState.InvalidSyntax => FilesystemFailureKind.InvalidSyntax,
            FileReadState.AccessDenied => FilesystemFailureKind.AccessDenied,
            FileReadState.InputOutputFailure => FilesystemFailureKind.InputOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The read state is not a failure state."),
        };
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
