using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

internal sealed class RouteSourceCatalogue
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<RouteSource>> _sourcesById;
    private readonly IReadOnlyDictionary<string, RouteSource> _sourcesByPath;
    private readonly IReadOnlyDictionary<string, RouteOverwriteFact> _overwriteFactsByPath;

    internal RouteSourceCatalogue(
        IEnumerable<RouteSource> sources,
        IEnumerable<RouteOverwriteFact> overwriteFacts)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(overwriteFacts);
        var materialized = sources
            .Select(source => source ?? throw new ArgumentException("A source catalogue cannot contain null.", nameof(sources)))
            .OrderBy(source => source.CanonicalPath, StringComparer.Ordinal)
            .ToArray();

        var byPath = new Dictionary<string, RouteSource>(StringComparer.Ordinal);
        foreach (var source in materialized)
        {
            AddPath(byPath, source.CanonicalPath, source);
            if (source.OverwritePath is not null)
            {
                AddPath(byPath, source.OverwritePath, source);
            }
        }

        var byId = materialized
            .GroupBy(source => source.Id, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<RouteSource>)new ReadOnlyCollection<RouteSource>(
                    group.OrderBy(source => source.CanonicalPath, StringComparer.Ordinal).ToArray()),
                StringComparer.Ordinal);

        var materializedOverwriteFacts = overwriteFacts
            .Select(fact => fact ?? throw new ArgumentException("Overwrite facts cannot contain null.", nameof(overwriteFacts)))
            .OrderBy(fact => fact.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        var overwriteFactsByPath = materializedOverwriteFacts.ToDictionary(
            fact => fact.CanonicalPath,
            StringComparer.Ordinal);
        ValidateOverwriteFacts(materialized, byPath, materializedOverwriteFacts);

        Sources = new ReadOnlyCollection<RouteSource>(materialized);
        IdentityCollisions = new ReadOnlyCollection<RouteSourceIdentityCollision>(
            materialized
                .GroupBy(source => source.Id, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => new RouteSourceIdentityCollision(
                    group.Key,
                    group.Select(source => source.CanonicalPath)))
                .OrderBy(collision => collision.Id, StringComparer.Ordinal)
                .ToArray());
        OverwriteFacts = new ReadOnlyCollection<RouteOverwriteFact>(materializedOverwriteFacts);
        _sourcesById = new ReadOnlyDictionary<string, IReadOnlyList<RouteSource>>(byId);
        _sourcesByPath = new ReadOnlyDictionary<string, RouteSource>(byPath);
        _overwriteFactsByPath = new ReadOnlyDictionary<string, RouteOverwriteFact>(overwriteFactsByPath);
    }

    internal IReadOnlyList<RouteSource> Sources { get; }

    internal IReadOnlyList<RouteSourceIdentityCollision> IdentityCollisions { get; }

    internal IReadOnlyList<RouteOverwriteFact> OverwriteFacts { get; }

    internal IReadOnlyList<RouteSource> FindById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _sourcesById.TryGetValue(id, out var sources)
            ? sources
            : Array.Empty<RouteSource>();
    }

    internal RouteSource? FindByPath(string canonicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        return _sourcesByPath.TryGetValue(canonicalPath, out var source)
            ? source
            : null;
    }

    internal RouteOverwriteFact? FindOverwriteByPath(string canonicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        return _overwriteFactsByPath.TryGetValue(canonicalPath, out var fact)
            ? fact
            : null;
    }

    private static void AddPath(
        IDictionary<string, RouteSource> byPath,
        string path,
        RouteSource source)
    {
        if (!byPath.TryAdd(path, source))
        {
            throw new ArgumentException("Source base and overwrite paths must be unique.", nameof(source));
        }
    }

    private static void ValidateOverwriteFacts(
        IReadOnlyList<RouteSource> sources,
        IReadOnlyDictionary<string, RouteSource> sourcesByPath,
        IReadOnlyList<RouteOverwriteFact> facts)
    {
        var sourceOverwritePaths = sources
            .Where(source => source.OverwritePath is not null)
            .Select(source => source.OverwritePath!)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var fact in facts)
        {
            if (fact.State != RouteOverwriteState.Paired)
            {
                if (sourcesByPath.ContainsKey(fact.CanonicalPath)
                    || sourceOverwritePaths.Contains(fact.CanonicalPath))
                {
                    throw new ArgumentException("An orphan or ambiguous overwrite path cannot collide with a source path.", nameof(facts));
                }

                continue;
            }

            var basePath = fact.CandidateBasePaths.Single();
            if (!sourcesByPath.TryGetValue(basePath, out var source)
                || source.Overwrite is null
                || !string.Equals(source.Overwrite.CanonicalLogicalPath, fact.CanonicalPath, StringComparison.Ordinal)
                || !string.Equals(source.Overwrite.PhysicalPath, fact.PhysicalPath, StringComparison.Ordinal))
            {
                throw new ArgumentException("Every paired overwrite fact must resolve to its source overwrite document.", nameof(facts));
            }
        }

        foreach (var source in sources)
        {
            if (source.Overwrite is null)
            {
                continue;
            }

            var matches = facts.Where(fact => fact.State == RouteOverwriteState.Paired
                    && fact.CandidateBasePaths.Count == 1
                    && string.Equals(fact.CandidateBasePaths[0], source.CanonicalPath, StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1)
            {
                throw new ArgumentException("Every source overwrite must have exactly one paired overwrite fact.", nameof(facts));
            }
        }
    }
}
