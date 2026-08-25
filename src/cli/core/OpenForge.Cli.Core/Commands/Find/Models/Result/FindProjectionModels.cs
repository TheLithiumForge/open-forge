using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Find.Models.Result;

internal enum FindProjectionState
{
    Available,
    Missing,
    Unavailable,
    Ambiguous,
}

internal enum FindRouteState
{
    Routed,
    Unrouted,
}

internal sealed record FindMetadataLayer
{
    internal FindMetadataLayer(SourceLayerKind kind, string path)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find metadata layer kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!SourceLogicalPath.IsCanonicalSource(path)
            || !path.EndsWith(".md", StringComparison.Ordinal)
            || (kind == SourceLayerKind.Overwrite) != path.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            throw new ArgumentException("The Find metadata layer path and kind must agree.", nameof(path));
        }
        Kind = kind;
        Path = path;
    }

    internal SourceLayerKind Kind { get; }

    internal string Path { get; }
}

internal sealed record FindMetadata
{
    internal FindMetadata(
        int position,
        string id,
        string path,
        FindRouteState routeState,
        string? route,
        IEnumerable<FindMetadataLayer> layers)
    {
        if (position < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, "A Find metadata position must be positive.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!SourceLogicalPath.IsCanonicalSource(path)
            || !path.EndsWith(".md", StringComparison.Ordinal)
            || path.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            throw new ArgumentException("Find metadata requires a canonical base source path.", nameof(path));
        }
        if (!string.Equals(SourceIdentity.DeriveId(path), id, StringComparison.Ordinal))
        {
            throw new ArgumentException("A Find metadata ID must be derived from its canonical base path.", nameof(id));
        }
        if (!Enum.IsDefined(routeState))
        {
            throw new ArgumentOutOfRangeException(nameof(routeState), routeState, "The Find route state is not defined.");
        }

        if (routeState == FindRouteState.Routed)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(route);
            if (!string.Equals(route, id, StringComparison.Ordinal))
            {
                throw new ArgumentException("A routed Find metadata route must equal its automatic source ID.", nameof(route));
            }
        }
        else if (route is not null)
        {
            throw new ArgumentException("An unrouted Find metadata record cannot carry a route.", nameof(route));
        }

        ArgumentNullException.ThrowIfNull(layers);
        var materializedLayers = layers.ToArray();
        if (materializedLayers.Any(layer => layer is null)
            || materializedLayers
                .Select(layer => layer.Kind)
                .Distinct()
                .Count() != materializedLayers.Length)
        {
            throw new ArgumentException("Find metadata layers must be non-null and unique by kind.", nameof(layers));
        }

        if (materializedLayers.Length == 0
            || materializedLayers[0].Kind != SourceLayerKind.Base
            || materializedLayers.Length == 2
                && materializedLayers[1].Kind != SourceLayerKind.Overwrite)
        {
            throw new ArgumentException("Find metadata layers must use base-then-overwrite order.", nameof(layers));
        }

        if (!string.Equals(materializedLayers[0].Path, path, StringComparison.Ordinal)
            || materializedLayers.Length == 2
                && !string.Equals(materializedLayers[1].Path, ReadOverwritePath(path), StringComparison.Ordinal))
        {
            throw new ArgumentException("Find metadata layers must belong to the logical base source.", nameof(layers));
        }

        Position = position;
        Id = id;
        Path = path;
        RouteState = routeState;
        Route = route;
        Layers = Array.AsReadOnly(materializedLayers);
    }

    internal int Position { get; }

    internal string Id { get; }

    internal string Path { get; }

    internal FindRouteState RouteState { get; }

    internal string? Route { get; }

    internal IReadOnlyList<FindMetadataLayer> Layers { get; }

    private static string ReadOverwritePath(string basePath)
        => $"{basePath[..^3]}.overwrite.md";
}

internal sealed record FindProjectedHeading
{
    internal FindProjectedHeading(
        string text,
        int level,
        MarkdownHeadingForm form,
        FindSourceLocation location,
        bool canonical)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (level is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, "A projected Find heading level must be between 1 and 6.");
        }

        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The projected Markdown heading form is not defined.");
        }

        if (canonical && form != MarkdownHeadingForm.Atx)
        {
            throw new ArgumentException("Only an ATX projected heading can be canonical.", nameof(canonical));
        }

        ArgumentNullException.ThrowIfNull(location);
        Text = text;
        Level = level;
        Form = form;
        Location = location;
        Canonical = canonical;
    }

    internal string Text { get; }

    internal int Level { get; }

    internal MarkdownHeadingForm Form { get; }

    internal FindSourceLocation Location { get; }

    internal bool Canonical { get; }
}

internal sealed record FindProjection
{
    internal FindProjection(
        FindContentPartKind part,
        string? name,
        SourceLayerKind? layer,
        string? path,
        FindProjectionState state,
        FindMetadata? metadata,
        string? text,
        IEnumerable<FindProjectedHeading> headings,
        FindSourceLocation? location)
    {
        if (!Enum.IsDefined(part))
        {
            throw new ArgumentOutOfRangeException(nameof(part), part, "The Find projection part is not defined.");
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Find projection state is not defined.");
        }

        if (part == FindContentPartKind.Section)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
        }
        else if (name is not null)
        {
            throw new ArgumentException("Only a section projection can carry a name.", nameof(name));
        }

        if (state is FindProjectionState.Missing or FindProjectionState.Ambiguous
            && part != FindContentPartKind.Section)
        {
            throw new ArgumentException("Only a section projection can be missing or ambiguous.", nameof(state));
        }

        if (part == FindContentPartKind.Metadata)
        {
            if (layer is not null || path is not null || text is not null || location is not null)
            {
                throw new ArgumentException("Metadata projections cannot carry a physical layer, text, or location.");
            }
        }
        else if (layer is null || string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A physical Find projection requires layer and path identity.");
        }

        if (path is not null
            && (!SourceLogicalPath.IsCanonicalSource(path)
                || !path.EndsWith(".md", StringComparison.Ordinal)
                || layer is { } pathLayer
                    && (pathLayer == SourceLayerKind.Overwrite) != path.EndsWith(".overwrite.md", StringComparison.Ordinal)))
        {
            throw new ArgumentException("The Find projection path and layer must agree.", nameof(path));
        }

        ArgumentNullException.ThrowIfNull(headings);
        var materializedHeadings = headings.ToArray();
        if (materializedHeadings.Any(heading => heading is null))
        {
            throw new ArgumentException("Find projected headings cannot contain null values.", nameof(headings));
        }

        for (var index = 1; index < materializedHeadings.Length; index++)
        {
            if (materializedHeadings[index].Location.ByteOffset < materializedHeadings[index - 1].Location.ByteOffset)
            {
                throw new ArgumentException("Find projected headings must retain document order.", nameof(headings));
            }
        }

        if (part != FindContentPartKind.Headings && materializedHeadings.Length != 0)
        {
            throw new ArgumentException("Only heading projections can carry projected headings.", nameof(headings));
        }

        if (part != FindContentPartKind.Metadata && metadata is not null)
        {
            throw new ArgumentException("Only metadata projections can carry metadata payload.", nameof(metadata));
        }

        if (part is not (FindContentPartKind.Frontmatter or FindContentPartKind.Body or FindContentPartKind.Section)
            && text is not null)
        {
            throw new ArgumentException("Only authored text projections can carry text payload.", nameof(text));
        }

        if (part is not (FindContentPartKind.Frontmatter or FindContentPartKind.Body or FindContentPartKind.Section)
            && location is not null)
        {
            throw new ArgumentException("Only authored text projections can carry a location.", nameof(location));
        }

        var hasPayload = metadata is not null || text is not null || materializedHeadings.Length != 0 || location is not null;
        if (state != FindProjectionState.Available && hasPayload)
        {
            throw new ArgumentException("Unavailable, missing, and ambiguous projections cannot carry payload facts.");
        }

        if (state == FindProjectionState.Available)
        {
            if (part == FindContentPartKind.Metadata && metadata is null
                || part is FindContentPartKind.Frontmatter or FindContentPartKind.Body or FindContentPartKind.Section
                    && (text is null || location is null))
            {
                throw new ArgumentException("An available Find projection requires its complete payload.");
            }

            if (part == FindContentPartKind.Headings && location is not null)
            {
                throw new ArgumentException("A heading-outline projection has no aggregate location.", nameof(location));
            }
        }

        Part = part;
        Name = name;
        Layer = layer;
        Path = path;
        State = state;
        Metadata = metadata;
        Text = text;
        Headings = Array.AsReadOnly(materializedHeadings);
        Location = location;
    }

    internal FindContentPartKind Part { get; }

    internal string? Name { get; }

    internal SourceLayerKind? Layer { get; }

    internal string? Path { get; }

    internal FindProjectionState State { get; }

    internal FindMetadata? Metadata { get; }

    internal string? Text { get; }

    internal IReadOnlyList<FindProjectedHeading> Headings { get; }

    internal FindSourceLocation? Location { get; }
}
