using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Context.Models.Selection;

internal enum ContextContentPartKind
{
    Metadata,
    Paths,
    Frontmatter,
    Headings,
    Body,
    Section,
}

internal sealed record ContextContentPart
{
    internal ContextContentPart(ContextContentPartKind kind, string? name, string canonicalValue)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Context content part kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalValue);
        if ((kind == ContextContentPartKind.Section) != (name is not null))
        {
            throw new ArgumentException("Only a Context section part carries a name.", nameof(name));
        }

        if (kind == ContextContentPartKind.Section
            && (string.IsNullOrEmpty(name)
                || !string.Equals(canonicalValue, ContextDefinitions.SectionPrefix + name, StringComparison.Ordinal)))
        {
            throw new ArgumentException("A Context section part must retain its exact canonical section name.", nameof(canonicalValue));
        }

        Kind = kind;
        Name = name;
        CanonicalValue = canonicalValue;
    }

    internal ContextContentPartKind Kind { get; }

    internal string? Name { get; }

    internal string CanonicalValue { get; }
}

internal sealed record ContextContentSelection
{
    internal ContextContentSelection(
        IEnumerable<ContextContentPart> supplied,
        IEnumerable<ContextContentPart> effective)
    {
        Supplied = Snapshot(supplied, nameof(supplied));
        Effective = Snapshot(effective, nameof(effective));
    }

    internal IReadOnlyList<ContextContentPart> Supplied { get; }

    internal IReadOnlyList<ContextContentPart> Effective { get; }

    private static IReadOnlyList<ContextContentPart> Snapshot(
        IEnumerable<ContextContentPart> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values
            .Select(value => value ?? throw new ArgumentException("Context content selections cannot contain null members.", parameterName))
            .ToArray();
        return new ReadOnlyCollection<ContextContentPart>(materialized);
    }
}

internal enum ContextLinkExpansionMode
{
    None,
    Bounded,
    All,
}

internal sealed record ContextLinkExpansion
{
    internal ContextLinkExpansion(ContextLinkExpansionMode mode, int? depth)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Context link expansion mode is not defined.");
        }

        if ((mode == ContextLinkExpansionMode.Bounded) != (depth is > 0))
        {
            throw new ArgumentException("Only bounded Context link expansion carries a positive depth.", nameof(depth));
        }

        Mode = mode;
        Depth = depth;
    }

    internal ContextLinkExpansionMode Mode { get; }

    internal int? Depth { get; }

    internal static ContextLinkExpansion None { get; } = new(mode: ContextLinkExpansionMode.None, depth: null);

    internal static ContextLinkExpansion All { get; } = new(mode: ContextLinkExpansionMode.All, depth: null);

    internal static ContextLinkExpansion Bounded(int depth) => new(mode: ContextLinkExpansionMode.Bounded, depth: depth);
}
