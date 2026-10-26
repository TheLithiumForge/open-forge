using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed record RouteInitCurrentSourceRead(
    SourceLogicalSource Source,
    SourceDocumentReadResult Base,
    SourceDocumentReadResult? Overwrite);

internal sealed record RouteInitCurrentChainEntry
{
    internal RouteInitCurrentChainEntry(
        string id,
        string canonicalMissingPath,
        IEnumerable<SourceLogicalSource> identityOccupants,
        IEnumerable<SourceLogicalSource> recognizedEntrypoints)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (!SourceLogicalPath.IsCanonicalSource(canonicalMissingPath))
        {
            throw new ArgumentException(
                "A Route Init chain entry requires one canonical missing-target path.",
                nameof(canonicalMissingPath));
        }

        Id = id;
        CanonicalMissingPath = canonicalMissingPath;
        IdentityOccupants = Copy(identityOccupants, nameof(identityOccupants));
        RecognizedEntrypoints = Copy(recognizedEntrypoints, nameof(recognizedEntrypoints));
    }

    internal string Id { get; }

    internal string CanonicalMissingPath { get; }

    internal IReadOnlyList<SourceLogicalSource> IdentityOccupants { get; }

    internal IReadOnlyList<SourceLogicalSource> RecognizedEntrypoints { get; }

    internal bool IsMissing => RecognizedEntrypoints.Count == 0;

    internal bool IsAmbiguous => RecognizedEntrypoints.Count > 1;

    internal SourceLogicalSource? Existing => RecognizedEntrypoints.Count == 1
        ? RecognizedEntrypoints[0]
        : null;

    private static IReadOnlyList<SourceLogicalSource> Copy(
        IEnumerable<SourceLogicalSource> sources,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(sources, parameterName);
        return new ReadOnlyCollection<SourceLogicalSource>(sources
            .Select(source => source ?? throw new ArgumentException(
                "Route Init current source sets cannot contain null members.",
                parameterName))
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray());
    }
}

internal sealed record RouteInitCurrentCatalogueFacts(
    SourceCatalogue Catalogue,
    GeneratedNavigationFormation? Formation)
{
    internal IReadOnlyList<SourceLogicalSource> ObservedSources => Catalogue.Sources;

    internal bool IsCancelled => Catalogue.IsCancelled;

    internal bool IsAgentsRootMissing => Catalogue.Issues.Any(issue =>
        issue.Code == SourceCatalogueIssueCode.RootMissing
        && string.Equals(
            issue.AttemptedCanonicalPath,
            Catalogue.Layout.MainRoot,
            StringComparison.Ordinal));
}

internal sealed record RouteInitCurrentStateFacts
{
    internal RouteInitCurrentStateFacts(
        RouteInitCurrentCatalogueFacts current,
        RouteInitTargetFacts target,
        IEnumerable<RouteInitCurrentChainEntry> chain,
        IEnumerable<RouteInitCurrentSourceRead> reads)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(target);
        Current = current;
        Target = target;
        Chain = new ReadOnlyCollection<RouteInitCurrentChainEntry>(chain.ToArray());
        Reads = new ReadOnlyCollection<RouteInitCurrentSourceRead>(reads.ToArray());
    }

    internal RouteInitCurrentCatalogueFacts Current { get; }

    internal RouteInitTargetFacts Target { get; }

    internal IReadOnlyList<RouteInitCurrentChainEntry> Chain { get; }

    internal IReadOnlyList<RouteInitCurrentSourceRead> Reads { get; }
}

internal sealed class RouteInitCurrentStateReader
{
    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();

    internal async ValueTask<RouteInitCurrentCatalogueFacts> ReadCatalogueAsync(
        WorkspaceLayout layout,
        RouteInitTargetFacts target,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(layout);
        var catalogue = await _catalogueReader.ReadAsync(
                new SourceCatalogueRequest(layout,
                    target.CanonicalPath is { } path && layout.TryGetMaintenanceBoundary(path, out var boundary)
                        && boundary.Kind == WorkspaceMaintenanceBoundaryKind.Outside
                        ? [boundary.DirectoryPath] : layout.SourceRoots),
                cancellationToken)
            .ConfigureAwait(false);
        return new RouteInitCurrentCatalogueFacts(
            catalogue,
            catalogue.IsCancelled
                ? null
                : _formationBuilder.Build(catalogue));
    }

    internal async ValueTask<RouteInitCurrentStateFacts> ReadChainAsync(
        RouteInitCurrentCatalogueFacts current,
        RouteInitTargetFacts target,
        WorkspaceLayoutRead layoutRead,
        CancellationToken cancellationToken,
        string? restorationDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(target);
        var layout = current.Catalogue.Layout;
        var canonicalPaths = RouteInitTargetPlanner.ReadChainPaths(layout, target);
        var outside = target.CanonicalPath is { } path && layout.TryGetMaintenanceBoundary(path, out var boundary)
            && boundary.Kind == WorkspaceMaintenanceBoundaryKind.Outside;
        var chain = canonicalPaths
            .Select(canonical =>
            {
                var id = SourceIdentity.DeriveId(layout, canonical)
                    ?? throw new InvalidOperationException("A chain entrypoint requires its exact source identity.");
                var occupants = current.Catalogue.FindAllById(id);
                var recognized = (outside
                    ? current.ObservedSources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)
                        && SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath) == SourceLogicalPath.ReadParent(canonical))
                    : occupants.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form))).ToArray();
                return new RouteInitCurrentChainEntry(
                    outside && recognized.Length == 1 ? recognized[0].Identity.AutomaticId : id,
                    canonical,
                    occupants,
                    recognized);
            })
            .ToArray();

        var relevant = ReadRelevantSources(current, chain, restorationDirectory);
        foreach (var canonical in canonicalPaths.Concat(relevant.SelectMany(source =>
            source.Overwrite is { } overwrite
                ? new[] { source.Base.CanonicalPath, overwrite.CanonicalPath } : [source.Base.CanonicalPath])))
        {
            RequireOutsidePhysicalTarget(layoutRead, canonical, cancellationToken);
        }
        var reader = new SourceDocumentReader(current.Catalogue.Workspace);
        var reads = new List<RouteInitCurrentSourceRead>(relevant.Count);
        foreach (var source in relevant)
        {
            var baseRead = await reader.ReadAsync(source.Base, cancellationToken).ConfigureAwait(false);
            SourceDocumentReadResult? overwriteRead = null;
            if (source.Overwrite is { } overwrite)
            {
                overwriteRead = await reader.ReadAsync(overwrite, cancellationToken).ConfigureAwait(false);
            }

            reads.Add(new RouteInitCurrentSourceRead(source, baseRead, overwriteRead));
        }

        return new RouteInitCurrentStateFacts(current, target, chain, reads);
    }

    internal static void RequireOutsidePhysicalTarget(WorkspaceLayoutRead observation, string path, CancellationToken token)
    {
        if (observation.Layout is not { } layout || !layout.TryGetMaintenanceBoundary(path, out var boundary)
            || boundary.Kind != WorkspaceMaintenanceBoundaryKind.Outside) return;
        var outside = observation.OutsideRoutes.SingleOrDefault(route => route.Descriptor.DirectoryPath == boundary.DirectoryPath);
        if (outside is not { State: WorkspaceOutsideRouteReadState.Available, PhysicalDirectory: { } physicalBoundary })
            throw new RouteInitPlanningException(
                outside?.State == WorkspaceOutsideRouteReadState.Unavailable
                    ? RouteInitFindingCode.InspectionIncomplete : RouteInitFindingCode.TargetUnsafe,
                "The selected outside Route Init directory is unavailable.",
                incomplete: outside?.State == WorkspaceOutsideRouteReadState.Unavailable);

        var workspace = layout.Workspace;
        var paths = new PhysicalPathResolver().CreateReadSession();
        var lexicalBoundary = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, boundary.DirectoryPath);
        var currentBoundary = paths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, lexicalBoundary);
        if (currentBoundary.State != PhysicalPathState.Contained
            || !PhysicalIdentityTracker.PathComparer.Equals(currentBoundary.ResolvedPhysicalPath, physicalBoundary))
            throw new RouteInitPlanningException(RouteInitFindingCode.TargetUnsafe,
                "The selected outside Route Init directory changed its retained physical coordinate.", incomplete: false);
        var lexicalPath = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path);
        var current = lexicalPath;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var leaf = NoFollowLeafObserver.Observe(paths, workspace, current, token);
            var required = current == lexicalPath ? NoFollowLeafState.OrdinaryFile : NoFollowLeafState.Directory;
            var resolved = paths.ResolveCandidate(lexicalBoundary, physicalBoundary, current);
            var expected = Path.GetFullPath(Path.Combine(physicalBoundary, Path.GetRelativePath(lexicalBoundary, current)));
            if (leaf.State != NoFollowLeafState.Missing && leaf.State != required
                || current == lexicalBoundary && leaf.State != NoFollowLeafState.Directory
                || resolved.State is not (PhysicalPathState.Contained or PhysicalPathState.Missing)
                || resolved.State == PhysicalPathState.Contained
                    && !PhysicalIdentityTracker.PathComparer.Equals(resolved.ResolvedPhysicalPath, expected))
                throw new RouteInitPlanningException(RouteInitFindingCode.TargetUnsafe,
                    "The outside Route Init target is not an ordinary leaf at its retained physical coordinate.", incomplete: false);
            if (current == lexicalBoundary) break;
            current = Path.GetDirectoryName(current) ?? throw new InvalidOperationException("An outside chain requires bounded ancestry.");
        }
    }

    private static IReadOnlyList<SourceLogicalSource> ReadRelevantSources(
        RouteInitCurrentCatalogueFacts current,
        IReadOnlyList<RouteInitCurrentChainEntry> chain,
        string? restorationDirectory)
    {
        var chainFolders = chain
            .Select(entry => SourceLogicalPath.ReadParent(entry.CanonicalMissingPath))
            .ToHashSet(StringComparer.Ordinal);
        var sources = new Dictionary<string, SourceLogicalSource>(StringComparer.Ordinal);
        if (restorationDirectory is not null)
        {
            foreach (var source in current.ObservedSources.Where(source =>
                source.Identity.CanonicalBasePath.StartsWith(restorationDirectory + "/", StringComparison.Ordinal)))
            {
                sources.TryAdd(source.Identity.CanonicalBasePath, source);
                if (SourceFormClassifier.IsEntrypoint(source.Base.Form))
                {
                    chainFolders.Add(SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath));
                }
            }
        }

        if (current.Formation?.Loader is { } loader)
        {
            sources.Add(loader.Identity.CanonicalBasePath, loader);
            foreach (var rootPath in current.Formation.Topology.LoaderRootPaths)
            {
                if (current.Formation.FindSource(rootPath) is { } root)
                {
                    sources.TryAdd(root.Identity.CanonicalBasePath, root);
                }
            }
        }

        foreach (var source in chain.SelectMany(entry => entry.RecognizedEntrypoints))
        {
            sources.TryAdd(source.Identity.CanonicalBasePath, source);
        }

        foreach (var source in current.ObservedSources.Where(source =>
                     IsDirectChildOfAny(source, chainFolders)))
        {
            sources.TryAdd(source.Identity.CanonicalBasePath, source);
        }

        return sources.Values
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsDirectChildOfAny(
        SourceLogicalSource source,
        IReadOnlySet<string> parentFolders)
    {
        if (source.Base.Form == SourceDocumentForm.Loader)
        {
            return false;
        }

        var containingDirectory = SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath);
        var representedParentDirectory = source.Base.Form == SourceDocumentForm.Markdown
            ? containingDirectory
            : SourceLogicalPath.ReadParent(containingDirectory);
        return parentFolders.Contains(representedParentDirectory);
    }
}
