using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Framework.Sources.Metadata;

internal sealed class SourceAuthoredMetadataParser
{
    private readonly SourceOpenForgeMetadataParser _openForgeParser = new();
    private readonly YamlDocumentParser _yamlParser = new();

    internal SourceAuthoredMetadataFacts Parse(
        MarkdownDocumentFacts document,
        SourceDocumentForm form)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The source document form is not defined.");
        }

        return form switch
        {
            SourceDocumentForm.Loader => SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.NotApplicable),
            SourceDocumentForm.Skill => ParseSkill(document),
            SourceDocumentForm.Markdown
                or SourceDocumentForm.CanonicalEntrypoint
                or SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint => ProjectOpenForge(_openForgeParser.Parse(document)),
            SourceDocumentForm.OverwriteCompanion => throw new ArgumentOutOfRangeException(
                nameof(form),
                form,
                "An overwrite companion is not a logical base form."),
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The source document form is not supported."),
        };
    }

    internal static SourceAuthoredMetadataFacts ProjectOpenForge(SourceOpenForgeMetadataFacts facts)
    {
        return facts.State switch
        {
            SourceOpenForgeMetadataState.Complete => SourceAuthoredMetadataFacts.Complete(
                facts.Description
                    ?? throw new InvalidOperationException("Complete Open Forge metadata requires a description."),
                facts.Tags),
            SourceOpenForgeMetadataState.Missing => SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing),
            SourceOpenForgeMetadataState.Malformed => SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Malformed),
            _ => throw new ArgumentOutOfRangeException(nameof(facts), facts.State, "The Open Forge metadata state is not defined."),
        };
    }

    private SourceAuthoredMetadataFacts ParseSkill(MarkdownDocumentFacts document)
    {
        if (document.Frontmatter.State == MarkdownFrontmatterState.Missing)
        {
            return SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing);
        }

        if (document.Frontmatter.State != MarkdownFrontmatterState.Complete
            || document.Frontmatter.YamlSpan is not { } yamlSpan)
        {
            return SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Malformed);
        }

        var yaml = document.Source[yamlSpan.Start..yamlSpan.End];
        var syntax = _yamlParser.Parse(yaml);
        if (syntax.State != YamlDocumentState.Complete)
        {
            return SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Malformed);
        }

        if (syntax.Root is null || IsNullScalar(syntax.Root, yaml))
        {
            return SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing);
        }

        if (syntax.Root is not { Kind: YamlNodeKind.Mapping } root
            || root.ContainsUnsupportedMapping
            || !TryCreateAnchorIndex(root, yaml, out var anchors)
            || !TryReadSkillValues(root, yaml, anchors, out var name, out var description))
        {
            return SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Malformed);
        }

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
        {
            return SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing);
        }

        return SourceAuthoredMetadataFacts.Complete(description, []);
    }

    private static bool TryReadSkillValues(
        YamlNode root,
        string yaml,
        IReadOnlyDictionary<string, YamlNode> anchors,
        out string? name,
        out string? description)
    {
        name = null;
        description = null;
        if (root.Mapping is not { } entries)
        {
            return false;
        }

        foreach (var entry in entries)
        {
            if (!TryResolveScalar(entry.Key, yaml, anchors, out var key)
                || !TryResolveScalar(entry.Value, yaml, anchors, out var value))
            {
                return false;
            }

            switch (key)
            {
                case "name" when name is null:
                    name = value;
                    break;
                case "description" when description is null:
                    description = value;
                    break;
                default:
                    return false;
            }
        }

        return true;
    }

    private static bool TryCreateAnchorIndex(
        YamlNode root,
        string yaml,
        out IReadOnlyDictionary<string, YamlNode> anchors)
    {
        var values = new Dictionary<string, YamlNode>(StringComparer.Ordinal);
        anchors = values;
        foreach (var node in EnumerateNodes(root))
        {
            if (node.Scalar is null
                || TryReadAnchorName(yaml, node.Span) is not { } anchor)
            {
                continue;
            }

            if (!values.TryAdd(anchor, node))
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<YamlNode> EnumerateNodes(YamlNode node)
    {
        yield return node;
        if (node.Sequence is { } sequence)
        {
            foreach (var item in sequence)
            {
                foreach (var descendant in EnumerateNodes(item))
                {
                    yield return descendant;
                }
            }
        }

        if (node.Mapping is not { } mapping)
        {
            yield break;
        }

        foreach (var entry in mapping)
        {
            foreach (var descendant in EnumerateNodes(entry.Key))
            {
                yield return descendant;
            }

            foreach (var descendant in EnumerateNodes(entry.Value))
            {
                yield return descendant;
            }
        }
    }

    private static bool TryResolveScalar(
        YamlNode node,
        string yaml,
        IReadOnlyDictionary<string, YamlNode> anchors,
        out string? value)
    {
        if (node.Scalar is { } scalar)
        {
            value = IsNullScalar(node, yaml) ? null : scalar.Value;
            return true;
        }

        value = null;
        if (node.Kind != YamlNodeKind.Alias
            || ReadAliasName(yaml, node.Span) is not { } alias
            || !anchors.TryGetValue(alias, out var target)
            || target.Span.Start >= node.Span.Start
            || target.Scalar is not { } targetScalar)
        {
            return false;
        }

        value = IsNullScalar(target, yaml) ? null : targetScalar.Value;
        return true;
    }

    private static bool IsNullScalar(YamlNode node, string yaml)
    {
        if (node.Scalar is null)
        {
            return false;
        }

        var source = yaml.AsSpan(node.Span.Start, node.Span.Length).Trim();
        var hasExplicitTag = false;
        var hasNullTag = false;
        while (!source.IsEmpty && source[0] is '&' or '!')
        {
            var propertyLength = ReadNodePropertyLength(source);
            if (propertyLength == 0)
            {
                return false;
            }

            if (source[0] == '!')
            {
                hasExplicitTag = true;
                var tag = source[..propertyLength];
                hasNullTag = tag.SequenceEqual("!!null")
                    || tag.SequenceEqual("!<tag:yaml.org,2002:null>");
            }

            source = source[propertyLength..].TrimStart();
        }

        if (hasExplicitTag)
        {
            return hasNullTag;
        }

        return source.SequenceEqual("~")
            || source.Equals("null", StringComparison.OrdinalIgnoreCase);
    }

    private static int ReadNodePropertyLength(ReadOnlySpan<char> source)
    {
        var index = 1;
        if (source.StartsWith("!<", StringComparison.Ordinal))
        {
            var end = source.IndexOf('>');
            return end < 0 ? 0 : end + 1;
        }

        while (index < source.Length && !IsAnchorTokenBoundary(source[index]))
        {
            index++;
        }

        return index;
    }

    private static string? TryReadAnchorName(string yaml, YamlTextSpan span)
    {
        var source = yaml.AsSpan(span.Start, span.Length);
        var index = 0;
        while (index < source.Length)
        {
            while (index < source.Length && char.IsWhiteSpace(source[index]))
            {
                index++;
            }

            if (index >= source.Length || source[index] is not ('!' or '&'))
            {
                return null;
            }

            var indicator = source[index++];
            var start = index;
            while (index < source.Length && !IsAnchorTokenBoundary(source[index]))
            {
                index++;
            }

            if (indicator == '&')
            {
                return index == start ? null : source[start..index].ToString();
            }
        }

        return null;
    }

    private static string? ReadAliasName(string yaml, YamlTextSpan span)
    {
        var source = yaml.AsSpan(span.Start, span.Length);
        if (source.Length < 2 || source[0] != '*')
        {
            return null;
        }

        var name = source[1..];
        if (name.IsEmpty)
        {
            return null;
        }

        foreach (var value in name)
        {
            if (IsAnchorTokenBoundary(value))
            {
                return null;
            }
        }

        return name.ToString();
    }

    private static bool IsAnchorTokenBoundary(char value)
        => char.IsWhiteSpace(value) || value is ',' or '[' or ']' or '{' or '}';
}
