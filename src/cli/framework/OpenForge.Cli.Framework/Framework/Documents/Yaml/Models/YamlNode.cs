namespace OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

internal enum YamlNodeKind
{
    Scalar,
    Sequence,
    Mapping,
    Alias,
}

internal sealed record YamlScalar
{
    internal YamlScalar(string value, YamlTextSpan span)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(span);
        Value = value;
        Span = span;
    }

    internal string Value { get; }

    internal YamlTextSpan Span { get; }
}

internal sealed record YamlMappingEntry
{
    internal YamlMappingEntry(YamlNode key, YamlNode value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        Key = key;
        Value = value;
    }

    internal YamlNode Key { get; }

    internal YamlNode Value { get; }
}

internal sealed class YamlNode
{
    private YamlNode(
        YamlNodeKind kind,
        YamlTextSpan span,
        YamlScalar? scalar,
        IReadOnlyList<YamlNode>? sequence,
        IReadOnlyList<YamlMappingEntry>? mapping)
    {
        Kind = kind;
        Span = span;
        Scalar = scalar;
        Sequence = sequence;
        Mapping = mapping;
    }

    internal YamlNodeKind Kind { get; }

    internal YamlTextSpan Span { get; }

    internal YamlScalar? Scalar { get; }

    internal IReadOnlyList<YamlNode>? Sequence { get; }

    internal IReadOnlyList<YamlMappingEntry>? Mapping { get; }

    internal bool ContainsAlias
        => Kind == YamlNodeKind.Alias
            || Sequence?.Any(node => node.ContainsAlias) == true
            || Mapping?.Any(entry => entry.Key.ContainsAlias || entry.Value.ContainsAlias) == true;

    internal bool ContainsUnsupportedMapping
    {
        get
        {
            if (Sequence?.Any(node => node.ContainsUnsupportedMapping) == true)
            {
                return true;
            }

            if (Mapping is null)
            {
                return false;
            }

            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in Mapping)
            {
                if (entry.Key.Scalar is null
                    || !keys.Add(entry.Key.Scalar.Value)
                    || entry.Key.ContainsUnsupportedMapping
                    || entry.Value.ContainsUnsupportedMapping)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal static YamlNode FromScalar(YamlScalar scalar)
    {
        ArgumentNullException.ThrowIfNull(scalar);
        return new YamlNode(YamlNodeKind.Scalar, scalar.Span, scalar, null, null);
    }

    internal static YamlNode FromSequence(YamlTextSpan span, IEnumerable<YamlNode> sequence)
    {
        ArgumentNullException.ThrowIfNull(span);
        ArgumentNullException.ThrowIfNull(sequence);
        return new YamlNode(YamlNodeKind.Sequence, span, null, Snapshot(sequence, nameof(sequence)), null);
    }

    internal static YamlNode FromMapping(YamlTextSpan span, IEnumerable<YamlMappingEntry> mapping)
    {
        ArgumentNullException.ThrowIfNull(span);
        ArgumentNullException.ThrowIfNull(mapping);
        return new YamlNode(YamlNodeKind.Mapping, span, null, null, Snapshot(mapping, nameof(mapping)));
    }

    internal static YamlNode Alias(YamlTextSpan span)
    {
        ArgumentNullException.ThrowIfNull(span);
        return new YamlNode(YamlNodeKind.Alias, span, null, null, null);
    }

    internal bool TryGetMappingValue(string key, out YamlNode? value)
    {
        value = null;
        if (Mapping is null)
        {
            return false;
        }

        foreach (var entry in Mapping)
        {
            if (!string.Equals(entry.Key.Scalar?.Value, key, StringComparison.Ordinal))
            {
                continue;
            }

            if (value is not null)
            {
                value = null;
                return false;
            }

            value = entry.Value;
        }

        return value is not null;
    }

    private static IReadOnlyList<T> Snapshot<T>(IEnumerable<T> values, string parameterName)
        where T : class
    {
        var materialized = values.ToArray();
        if (materialized.Any(value => value is null))
        {
            throw new ArgumentException("A YAML child collection cannot contain null values.", parameterName);
        }

        return Array.AsReadOnly(materialized);
    }
}
