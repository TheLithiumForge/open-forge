using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

internal sealed class RouteSourceProjectionSet
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<RouteSource>> _sourcesById;
    private readonly IReadOnlyDictionary<string, RouteSource> _sourcesByPath;
    private readonly IReadOnlyDictionary<string, RouteOverwriteFact> _overwriteFactsByPath;

    internal RouteSourceProjectionSet(
        IEnumerable<RouteSourceProjection> projections,
        IEnumerable<RouteOverwriteFact> overwriteFacts)
    {
        ArgumentNullException.ThrowIfNull(projections);
        ArgumentNullException.ThrowIfNull(overwriteFacts);

        var orderedProjections = projections
            .Select(projection => projection ?? throw new ArgumentException("Projection sets cannot contain null.", nameof(projections)))
            .OrderBy(projection => projection.LogicalSource.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (orderedProjections.Select(projection => projection.LogicalSource.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != orderedProjections.Length)
        {
            throw new ArgumentException("Projection sets require unique logical source bases.", nameof(projections));
        }

        var projectedSources = orderedProjections
            .Where(projection => projection.Source is not null)
            .Select(projection => projection.Source
                ?? throw new InvalidOperationException("A projected source must retain its Route source."))
            .OrderBy(source => source.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (projectedSources.Select(source => source.CanonicalPath)
            .Distinct(StringComparer.Ordinal)
            .Count() != projectedSources.Length)
        {
            throw new ArgumentException("Projected Route sources require unique canonical paths.", nameof(projections));
        }

        var orderedOverwriteFacts = overwriteFacts
            .Select(fact => fact ?? throw new ArgumentException("Projection overwrite facts cannot contain null.", nameof(overwriteFacts)))
            .OrderBy(fact => fact.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (orderedOverwriteFacts.Select(fact => fact.CanonicalPath)
            .Distinct(StringComparer.Ordinal)
            .Count() != orderedOverwriteFacts.Length)
        {
            throw new ArgumentException("Projection overwrite facts require unique canonical paths.", nameof(overwriteFacts));
        }

        ValidateOverwriteFacts(projectedSources, orderedOverwriteFacts);

        Projections = new ReadOnlyCollection<RouteSourceProjection>(orderedProjections);
        Sources = new ReadOnlyCollection<RouteSource>(projectedSources);
        IdentityCollisions = new ReadOnlyCollection<RouteSourceIdentityCollision>(
            projectedSources
                .GroupBy(source => source.Id, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => new RouteSourceIdentityCollision(
                    group.Key,
                    group.Select(source => source.CanonicalPath)))
                .OrderBy(collision => collision.Id, StringComparer.Ordinal)
                .ToArray());
        OverwriteFacts = new ReadOnlyCollection<RouteOverwriteFact>(orderedOverwriteFacts);
        _sourcesById = new ReadOnlyDictionary<string, IReadOnlyList<RouteSource>>(
            projectedSources
                .GroupBy(source => source.Id, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<RouteSource>)new ReadOnlyCollection<RouteSource>(group.ToArray()),
                    StringComparer.Ordinal));
        var sourcesByPath = new Dictionary<string, RouteSource>(StringComparer.Ordinal);
        foreach (var source in projectedSources)
        {
            AddPath(sourcesByPath, source.CanonicalPath, source);
            if (source.OverwritePath is not null)
            {
                AddPath(sourcesByPath, source.OverwritePath, source);
            }
        }

        _sourcesByPath = new ReadOnlyDictionary<string, RouteSource>(sourcesByPath);
        _overwriteFactsByPath = new ReadOnlyDictionary<string, RouteOverwriteFact>(
            orderedOverwriteFacts.ToDictionary(fact => fact.CanonicalPath, StringComparer.Ordinal));
    }

    internal IReadOnlyList<RouteSourceProjection> Projections { get; }

    internal IReadOnlyList<RouteSource> Sources { get; }

    internal IReadOnlyList<RouteSourceIdentityCollision> IdentityCollisions { get; }

    internal IReadOnlyList<RouteOverwriteFact> OverwriteFacts { get; }

    internal IReadOnlyList<RouteSource> FindAllById(string id)
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

    private static void ValidateOverwriteFacts(
        IReadOnlyList<RouteSource> sources,
        IReadOnlyList<RouteOverwriteFact> facts)
    {
        var sourcesByBasePath = sources.ToDictionary(source => source.CanonicalPath, StringComparer.Ordinal);
        var projectedPaths = new HashSet<string>(sourcesByBasePath.Keys, StringComparer.Ordinal);
        foreach (var source in sources.Where(source => source.OverwritePath is not null))
        {
            var overwritePath = source.OverwritePath
                ?? throw new InvalidOperationException(
                    "A source with an overwrite must retain its canonical overwrite path.");
            if (!projectedPaths.Add(overwritePath))
            {
                throw new ArgumentException("Projected Route base and overwrite paths must be unique.", nameof(sources));
            }
        }

        foreach (var fact in facts)
        {
            if (fact.State != RouteOverwriteState.Paired)
            {
                if (projectedPaths.Contains(fact.CanonicalPath))
                {
                    throw new ArgumentException("An orphan or ambiguous overwrite cannot collide with a projected source path.", nameof(facts));
                }

                continue;
            }

            var basePath = fact.CandidateBasePaths.Single();
            if (!sourcesByBasePath.TryGetValue(basePath, out var source)
                || source.Overwrite is null
                || !string.Equals(source.Overwrite.CanonicalLogicalPath, fact.CanonicalPath, StringComparison.Ordinal)
                || !string.Equals(source.Overwrite.PhysicalPath, fact.PhysicalPath, StringComparison.Ordinal))
            {
                throw new ArgumentException("Every paired overwrite fact must match one projected source overwrite.", nameof(facts));
            }
        }

        foreach (var source in sources.Where(source => source.Overwrite is not null))
        {
            var matches = facts.Count(fact => fact.State == RouteOverwriteState.Paired
                && string.Equals(fact.CandidateBasePaths.Single(), source.CanonicalPath, StringComparison.Ordinal));
            if (matches != 1)
            {
                throw new ArgumentException("Every projected source overwrite requires exactly one paired fact.", nameof(facts));
            }
        }
    }

    private static void AddPath(
        IDictionary<string, RouteSource> sourcesByPath,
        string path,
        RouteSource source)
    {
        if (!sourcesByPath.TryAdd(path, source))
        {
            throw new ArgumentException("Projected Route source paths must be unique.", nameof(source));
        }
    }
}
