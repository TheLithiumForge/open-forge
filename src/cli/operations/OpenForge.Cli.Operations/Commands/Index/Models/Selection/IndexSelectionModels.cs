using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Index.Models.Selection;

internal enum IndexSelectionOrigin
{
    AutomaticLoader,
    ExplicitSources,
}

internal enum IndexSelectionScope
{
    NotEstablished,
    Rooted,
    Detached,
    Mixed,
}

internal enum IndexLogicalSourceScope
{
    Rooted,
    Detached,
}

internal sealed record IndexLogicalSource
{
    internal IndexLogicalSource(
        string id,
        string path,
        IndexLogicalSourceScope scope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (!SourceLogicalPath.IsCanonicalSource(path)
            || !string.Equals(SourceIdentity.DeriveId(path), id, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "An Index logical source requires its canonical source path and derived source ID.",
                nameof(path));
        }
        _ = scope switch
        {
            IndexLogicalSourceScope.Rooted or IndexLogicalSourceScope.Detached => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(scope),
                scope,
                "The Index logical source scope is not defined."),
        };

        Id = id;
        Path = path;
        Scope = scope;
    }

    internal string Id { get; }

    internal string Path { get; }

    internal IndexLogicalSourceScope Scope { get; }
}

internal sealed record IndexSelection
{
    internal IndexSelection(
        IndexSelectionOrigin origin,
        IndexSelectionScope scope,
        IEnumerable<IndexLogicalSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        var ordered = sources
            .Select(source => source ?? throw new ArgumentException(
                "Index selection sources cannot contain null members.",
                nameof(sources)))
            .OrderBy(source => source.Path, StringComparer.Ordinal)
            .ThenBy(source => source.Id, StringComparer.Ordinal)
            .ToArray();
        if (ordered.Select(source => source.Path).Distinct(StringComparer.Ordinal).Count() != ordered.Length)
        {
            throw new ArgumentException("Index selection sources require unique canonical paths.", nameof(sources));
        }

        ValidateScope(scope, ordered);
        ValidateOrigin(origin, scope, ordered);
        Origin = origin;
        Scope = scope;
        Sources = new ReadOnlyCollection<IndexLogicalSource>(ordered);
    }

    internal IndexSelectionOrigin Origin { get; }

    internal IndexSelectionScope Scope { get; }

    internal IReadOnlyList<IndexLogicalSource> Sources { get; }

    internal static IndexSelection NotEstablished(IndexSelectionOrigin origin)
        => new(origin, IndexSelectionScope.NotEstablished, []);

    private static void ValidateScope(
        IndexSelectionScope scope,
        IReadOnlyList<IndexLogicalSource> sources)
    {
        var rooted = sources.Count(source => source.Scope == IndexLogicalSourceScope.Rooted);
        var detached = sources.Count - rooted;
        var valid = scope switch
        {
            IndexSelectionScope.NotEstablished => sources.Count == 0,
            IndexSelectionScope.Rooted => rooted != 0 && detached == 0,
            IndexSelectionScope.Detached => detached != 0 && rooted == 0,
            IndexSelectionScope.Mixed => rooted != 0 && detached != 0,
            _ => throw new ArgumentOutOfRangeException(
                nameof(scope),
                scope,
                "The Index selection scope is not defined."),
        };
        if (!valid)
        {
            throw new ArgumentException("Index selection sources do not match the selection scope.", nameof(sources));
        }
    }

    private static void ValidateOrigin(
        IndexSelectionOrigin origin,
        IndexSelectionScope scope,
        IReadOnlyList<IndexLogicalSource> sources)
    {
        _ = origin switch
        {
            IndexSelectionOrigin.AutomaticLoader or IndexSelectionOrigin.ExplicitSources => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(origin),
                origin,
                "The Index selection origin is not defined."),
        };

        if (origin != IndexSelectionOrigin.AutomaticLoader || scope == IndexSelectionScope.NotEstablished)
        {
            return;
        }

        if (sources.Count != 1
            || sources[0].Path != SourceLogicalPath.LoaderPath
            || sources[0].Scope != IndexLogicalSourceScope.Rooted)
        {
            throw new ArgumentException(
                "An established automatic Index selection requires exactly the normalized rooted Loader.",
                nameof(sources));
        }
    }
}
