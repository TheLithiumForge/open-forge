namespace OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

internal enum YamlDocumentState
{
    Complete,
    Unavailable,
}

internal sealed record YamlDocumentFacts
{
    internal YamlDocumentFacts(string source, YamlDocumentState state, YamlNode? root)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The YAML document state is not defined.");
        }

        if (state == YamlDocumentState.Unavailable && root is not null)
        {
            throw new ArgumentException("Unavailable YAML cannot expose a root node.", nameof(root));
        }

        if (root is not null)
        {
            ValidateNode(root, source.Length, null, nameof(root));
        }

        Source = source;
        State = state;
        Root = root;
    }

    internal string Source { get; }

    internal YamlDocumentState State { get; }

    internal YamlNode? Root { get; }

    internal bool HasAliases => Root?.ContainsAlias == true;

    internal bool HasUnsupportedMappings => Root?.ContainsUnsupportedMapping == true;

    private static void ValidateNode(
        YamlNode node,
        int sourceLength,
        YamlTextSpan? parentSpan,
        string parameterName)
    {
        ValidateSpan(node.Span, sourceLength, parentSpan, parameterName);
        if (node.Scalar is { } scalar)
        {
            ValidateSpan(scalar.Span, sourceLength, node.Span, parameterName);
        }

        if (node.Sequence is { } sequence)
        {
            foreach (var item in sequence)
            {
                ValidateNode(item, sourceLength, node.Span, parameterName);
            }
        }

        if (node.Mapping is { } mapping)
        {
            foreach (var entry in mapping)
            {
                ValidateNode(entry.Key, sourceLength, node.Span, parameterName);
                ValidateNode(entry.Value, sourceLength, node.Span, parameterName);
            }
        }
    }

    private static void ValidateSpan(
        YamlTextSpan span,
        int sourceLength,
        YamlTextSpan? parentSpan,
        string parameterName)
    {
        if (span.End > sourceLength)
        {
            throw new ArgumentException("A YAML node span must be contained by its source.", parameterName);
        }

        if (parentSpan is not null
            && (span.Start < parentSpan.Start || span.End > parentSpan.End))
        {
            throw new ArgumentException("A YAML child span must be contained by its parent.", parameterName);
        }
    }
}
