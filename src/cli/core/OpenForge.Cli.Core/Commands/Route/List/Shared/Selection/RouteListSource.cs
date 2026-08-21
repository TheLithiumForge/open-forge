using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal enum RouteListSourceKind
{
    Loader,
    Entrypoint,
    RoutedLeaf,
    RoutedNative,
    Unrouted,
}

internal sealed class RouteListSource
{
    internal RouteListSource(
        string id,
        string canonicalPath,
        string physicalPath,
        RouteListSourceKind kind,
        string? overwritePath = null,
        bool isRouteAmbiguous = false)
    {
        if (!RouteListSourceIdentity.IsValidId(id))
        {
            throw new ArgumentException("The source ID is not valid.", nameof(id));
        }

        if (!RouteListSourceReferenceParser.IsValidCanonicalPath(canonicalPath))
        {
            throw new ArgumentException("The source canonical path is not valid.", nameof(canonicalPath));
        }

        if (!string.Equals(id, RouteListSourceIdentity.DeriveId(canonicalPath), StringComparison.Ordinal))
        {
            throw new ArgumentException("The source ID must match its canonical path identity.", nameof(id));
        }

        if (canonicalPath.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            throw new ArgumentException("An overwrite companion cannot be a logical source base.", nameof(canonicalPath));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The source kind is not defined.");
        }

        var isLoader = string.Equals(canonicalPath, ".agents/loader.md", StringComparison.Ordinal);
        if ((kind == RouteListSourceKind.Loader) != isLoader)
        {
            throw new ArgumentException("Loader source kind is valid only for the exact Loader path.", nameof(kind));
        }

        var isEntrypoint = RouteListSourceIdentity.IsRecognizedEntrypointPath(canonicalPath);
        if ((kind == RouteListSourceKind.Entrypoint) != isEntrypoint)
        {
            throw new ArgumentException("Entrypoint source kind must match a recognized entrypoint path.", nameof(kind));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        var normalizedPhysicalPath = Path.GetFullPath(physicalPath);
        if (!Path.IsPathRooted(physicalPath)
            || !string.Equals(normalizedPhysicalPath, physicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("The source physical path must be absolute and normalized.", nameof(physicalPath));
        }

        if (overwritePath is not null
            && !RouteListSourceReferenceParser.IsValidCanonicalPath(overwritePath))
        {
            throw new ArgumentException("The source overwrite path is not valid.", nameof(overwritePath));
        }

        if (overwritePath is not null
            && !string.Equals(overwritePath, ReadAdjacentOverwritePath(canonicalPath), StringComparison.Ordinal))
        {
            throw new ArgumentException("A source overwrite path must be adjacent to its Markdown base.", nameof(overwritePath));
        }

        Id = id;
        CanonicalPath = canonicalPath;
        PhysicalPath = normalizedPhysicalPath;
        Kind = kind;
        OverwritePath = overwritePath;
        IsRouteAmbiguous = isRouteAmbiguous;
    }

    private static string? ReadAdjacentOverwritePath(string canonicalPath)
    {
        return canonicalPath.EndsWith(".md", StringComparison.Ordinal)
            ? canonicalPath[..^".md".Length] + ".overwrite.md"
            : null;
    }

    internal string Id { get; }

    internal string CanonicalPath { get; }

    internal string PhysicalPath { get; }

    internal RouteListSourceKind Kind { get; }

    internal string? OverwritePath { get; }

    internal bool IsRouteAmbiguous { get; }
}

internal sealed class RouteListSourceCatalogue
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<RouteListSource>> _sourcesById;
    private readonly IReadOnlyDictionary<string, RouteListSource> _sourcesByPath;

    internal RouteListSourceCatalogue(IEnumerable<RouteListSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        var materialized = sources.ToArray();
        if (materialized.Any(source => source is null))
        {
            throw new ArgumentException("A source catalogue cannot contain null.", nameof(sources));
        }

        var byPath = new Dictionary<string, RouteListSource>(StringComparer.Ordinal);
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
                group => (IReadOnlyList<RouteListSource>)new ReadOnlyCollection<RouteListSource>(
                    group.OrderBy(source => source.CanonicalPath, StringComparer.Ordinal).ToArray()),
                StringComparer.Ordinal);

        _sourcesById = new ReadOnlyDictionary<string, IReadOnlyList<RouteListSource>>(byId);
        _sourcesByPath = new ReadOnlyDictionary<string, RouteListSource>(byPath);
    }

    internal IReadOnlyList<RouteListSource> FindById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _sourcesById.TryGetValue(id, out var sources)
            ? sources
            : Array.Empty<RouteListSource>();
    }

    internal RouteListSource? FindByPath(string canonicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        return _sourcesByPath.TryGetValue(canonicalPath, out var source)
            ? source
            : null;
    }

    private static void AddPath(
        IDictionary<string, RouteListSource> byPath,
        string path,
        RouteListSource source)
    {
        if (!byPath.TryAdd(path, source))
        {
            throw new ArgumentException("Source canonical and overwrite paths must be unique.", nameof(source));
        }
    }
}
