using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal enum RouteListRowKind
{
    Entrypoint,
    RoutedLeaf,
}

internal enum RouteListSelectionProvenance
{
    LoaderRoot,
    ExplicitRoot,
    DetachedRoot,
    Descendant,
}

internal enum RouteListSourceProvenance
{
    AuthoredEntrypoint,
    AuthoredLeaf,
    RoutedNative,
}

internal sealed record RouteListProvenance
{
    internal RouteListProvenance(
        RouteListSelectionProvenance selection,
        RouteListSourceProvenance source,
        bool hasOverwrite)
    {
        if (!Enum.IsDefined(selection))
        {
            throw new ArgumentOutOfRangeException(nameof(selection), selection, "The selection provenance is not defined.");
        }

        if (!Enum.IsDefined(source))
        {
            throw new ArgumentOutOfRangeException(nameof(source), source, "The source provenance is not defined.");
        }

        Selection = selection;
        Source = source;
        HasOverwrite = hasOverwrite;
    }

    internal RouteListSelectionProvenance Selection { get; }

    internal RouteListSourceProvenance Source { get; }

    internal bool HasOverwrite { get; }
}

internal sealed class RouteListRow
{
    private RouteListRow(
        string id,
        string path,
        string? parentId,
        string? parentPath,
        int? absoluteDepth,
        int relativeDepth,
        RouteListRowKind kind,
        string description,
        IReadOnlyList<string> tags,
        int? directChildCount,
        RouteListProvenance provenance)
    {
        Id = id;
        Path = path;
        ParentId = parentId;
        ParentPath = parentPath;
        AbsoluteDepth = absoluteDepth;
        RelativeDepth = relativeDepth;
        Kind = kind;
        Description = description;
        Tags = tags;
        DirectChildCount = directChildCount;
        Provenance = provenance;
    }

    internal string Id { get; }

    internal string Path { get; }

    internal string? ParentId { get; }

    internal string? ParentPath { get; }

    internal int? AbsoluteDepth { get; }

    internal int RelativeDepth { get; }

    internal RouteListRowKind Kind { get; }

    internal string Description { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal int? DirectChildCount { get; }

    internal RouteListProvenance Provenance { get; }

    internal static RouteListRow Entrypoint(
        string id,
        string path,
        string? parentId,
        string? parentPath,
        int? absoluteDepth,
        int relativeDepth,
        string description,
        IEnumerable<string> tags,
        int? directChildCount,
        RouteListProvenance provenance)
    {
        if (directChildCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(directChildCount),
                directChildCount,
                "Direct child count cannot be negative.");
        }

        return Create(
            id,
            path,
            parentId,
            parentPath,
            absoluteDepth,
            relativeDepth,
            RouteListRowKind.Entrypoint,
            description,
            tags,
            directChildCount,
            provenance);
    }

    internal static RouteListRow RoutedLeaf(
        string id,
        string path,
        string? parentId,
        string? parentPath,
        int? absoluteDepth,
        int relativeDepth,
        string description,
        IEnumerable<string> tags,
        RouteListProvenance provenance)
    {
        return Create(
            id,
            path,
            parentId,
            parentPath,
            absoluteDepth,
            relativeDepth,
            RouteListRowKind.RoutedLeaf,
            description,
            tags,
            null,
            provenance);
    }

    private static RouteListRow Create(
        string id,
        string path,
        string? parentId,
        string? parentPath,
        int? absoluteDepth,
        int relativeDepth,
        RouteListRowKind kind,
        string description,
        IEnumerable<string> tags,
        int? directChildCount,
        RouteListProvenance provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if ((parentId is null) != (parentPath is null))
        {
            throw new ArgumentException("Parent ID and parent path must both be present or both be absent.");
        }

        if (parentId is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(parentId);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentPath);
        }

        if (absoluteDepth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(absoluteDepth), absoluteDepth, "Absolute depth cannot be negative.");
        }

        if (relativeDepth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(relativeDepth), relativeDepth, "Relative depth cannot be negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(tags);
        var materializedTags = tags.ToArray();
        if (materializedTags.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Route-list tags cannot contain an empty value.", nameof(tags));
        }

        ArgumentNullException.ThrowIfNull(provenance);
        ValidateProvenance(
            kind,
            parentId,
            absoluteDepth,
            relativeDepth,
            provenance);
        return new RouteListRow(
            id,
            path,
            parentId,
            parentPath,
            absoluteDepth,
            relativeDepth,
            kind,
            description,
            new ReadOnlyCollection<string>(materializedTags),
            directChildCount,
            provenance);
    }

    private static void ValidateProvenance(
        RouteListRowKind kind,
        string? parentId,
        int? absoluteDepth,
        int relativeDepth,
        RouteListProvenance provenance)
    {
        if (kind == RouteListRowKind.Entrypoint
            && provenance.Source != RouteListSourceProvenance.AuthoredEntrypoint)
        {
            throw new ArgumentException("An entrypoint row requires authored-entrypoint provenance.", nameof(provenance));
        }

        if (kind == RouteListRowKind.RoutedLeaf
            && provenance.Source == RouteListSourceProvenance.AuthoredEntrypoint)
        {
            throw new ArgumentException("A routed leaf cannot have entrypoint source provenance.", nameof(provenance));
        }

        switch (provenance.Selection)
        {
            case RouteListSelectionProvenance.LoaderRoot:
                if (kind != RouteListRowKind.Entrypoint
                    || relativeDepth != 0
                    || parentId is null && absoluteDepth != 0
                    || parentId is not null && absoluteDepth == 0)
                {
                    throw new ArgumentException(
                        "A Loader-selected root has relative depth zero and preserves established authored ancestry.",
                        nameof(provenance));
                }

                break;
            case RouteListSelectionProvenance.ExplicitRoot:
                if (relativeDepth != 0
                    || absoluteDepth is null
                    || parentId is null && (kind != RouteListRowKind.Entrypoint || absoluteDepth != 0)
                    || parentId is not null && absoluteDepth <= 0)
                {
                    throw new ArgumentException(
                        "An explicitly selected root has relative depth zero and preserves valid absolute ancestry.",
                        nameof(provenance));
                }

                break;
            case RouteListSelectionProvenance.DetachedRoot:
                if (absoluteDepth is not null
                    || relativeDepth != 0
                    || kind == RouteListRowKind.RoutedLeaf && parentId is null)
                {
                    throw new ArgumentException(
                        "A detached selected root has no Loader-rooted absolute depth, preserves any local parent, and has relative depth zero.",
                        nameof(provenance));
                }

                break;
            case RouteListSelectionProvenance.Descendant:
                if (parentId is null || relativeDepth == 0 || absoluteDepth == 0)
                {
                    throw new ArgumentException(
                        "A descendant requires a parent, positive relative depth, and positive or detached absolute depth.",
                        nameof(provenance));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(provenance),
                    provenance.Selection,
                    "The row selection provenance is not defined.");
        }
    }
}
