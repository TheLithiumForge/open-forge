using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace;

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
            SourceLogicalPath.AgentsRoot,
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
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        var catalogue = await _catalogueReader.ReadAsync(
                new SourceCatalogueRequest(workspace, [SourceLogicalPath.AgentsRoot]),
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
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(target);
        var canonicalSegments = target.CanonicalSegments
            ?? throw new ArgumentException(
                "Route Init current-chain inspection requires an aligned target.",
                nameof(target));
        var canonicalPaths = RouteInitTargetPlanner.ReadChainPaths(target);
        var chain = Enumerable.Range(1, canonicalSegments.Count)
            .Select(index =>
            {
                var id = string.Join('/', canonicalSegments.Take(index));
                var occupants = current.Catalogue.FindAllById(id);
                return new RouteInitCurrentChainEntry(
                    id,
                    canonicalPaths[index - 1],
                    occupants,
                    occupants.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)));
            })
            .ToArray();

        var relevant = ReadRelevantSources(current, chain);
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

    private static IReadOnlyList<SourceLogicalSource> ReadRelevantSources(
        RouteInitCurrentCatalogueFacts current,
        IReadOnlyList<RouteInitCurrentChainEntry> chain)
    {
        var chainFolders = chain
            .Select(entry => SourceLogicalPath.ReadParent(entry.CanonicalMissingPath))
            .ToHashSet(StringComparer.Ordinal);
        var sources = new Dictionary<string, SourceLogicalSource>(StringComparer.Ordinal);

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
