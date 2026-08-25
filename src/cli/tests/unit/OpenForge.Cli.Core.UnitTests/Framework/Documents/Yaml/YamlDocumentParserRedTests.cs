using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Yaml;

public sealed class YamlDocumentParserRedTests
{
    [Fact(DisplayName = "Shared YAML parsing preserves ordered shapes, decoded scalars, and exact source spans")]
    [Trait("Feature", "yaml-documents"), Trait("Evidence", "Unit")]
    public void ShapesAndScalarSpansPreserveExactSourceFacts()
    {
        const string source =
            "open-forge:\n"
                + "  tags: [Plain, 'Évidence', \"工作\"]\n"
                + "emoji: \"😀\"\n"
                + "inline: {first: One, second: Two}\n"
                + "nested:\n"
                + "  - key: value\n";

        var facts = new YamlDocumentParser().Parse(source);

        Assert.Equal(source, facts.Source);
        Assert.Equal(YamlDocumentState.Complete, facts.State);
        Assert.False(facts.HasAliases);
        Assert.False(facts.HasUnsupportedMappings);
        var root = Assert.IsType<YamlNode>(facts.Root);
        Assert.Equal(YamlNodeKind.Mapping, root.Kind);
        Assert.Equal(new YamlTextSpan(0, source.Length), root.Span);
        var rootEntries = Assert.IsAssignableFrom<IReadOnlyList<YamlMappingEntry>>(root.Mapping);
        Assert.Equal(
            ["open-forge", "emoji", "inline", "nested"],
            rootEntries.Select(entry => entry.Key.Scalar?.Value));

        var openForge = MappingValue(root, "open-forge");
        var tags = MappingValue(openForge, "tags");
        Assert.Equal(YamlNodeKind.Sequence, tags.Kind);
        Assert.Equal("[Plain, 'Évidence', \"工作\"]", Slice(source, tags.Span));

        var values = Assert.IsAssignableFrom<IReadOnlyList<YamlNode>>(tags.Sequence);
        Assert.Equal(["Plain", "Évidence", "工作"], values.Select(value => value.Scalar?.Value));
        Assert.Equal(["Plain", "'Évidence'", "\"工作\""], values.Select(value => Slice(source, value.Span)));

        var emoji = MappingValue(root, "emoji");
        Assert.Equal("😀", emoji.Scalar?.Value);
        Assert.Equal(new YamlTextSpan(source.IndexOf("\"😀\"", StringComparison.Ordinal), 4), emoji.Span);
        Assert.Equal("\"😀\"", Slice(source, emoji.Span));

        var inline = MappingValue(root, "inline");
        Assert.Equal(YamlNodeKind.Mapping, inline.Kind);
        Assert.Equal("{first: One, second: Two}", Slice(source, inline.Span));
        Assert.Equal(
            ["first", "second"],
            Assert.IsAssignableFrom<IReadOnlyList<YamlMappingEntry>>(inline.Mapping)
                .Select(entry => entry.Key.Scalar?.Value));

        var nestedEntry = rootEntries[3];
        Assert.Equal(source.IndexOf("nested", StringComparison.Ordinal), nestedEntry.Key.Span.Start);
        Assert.Equal("nested", Slice(source, nestedEntry.Key.Span));
        var nested = MappingValue(root, "nested");
        Assert.Equal("- key: value\n", Slice(source, nested.Span));
        var item = Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<YamlNode>>(nested.Sequence));
        Assert.Equal("value", MappingValue(item, "key").Scalar?.Value);
    }

    [Fact(DisplayName = "Shared YAML parsing reports aliases, duplicate keys, and non-scalar mapping keys as neutral facts")]
    [Trait("Feature", "yaml-documents"), Trait("Evidence", "Unit")]
    public void AliasesAndUnsupportedMappingsRemainVisibleWithoutPolicy()
    {
        const string duplicateSource =
            "anchor: &shared [One]\n"
                + "alias: *shared\n"
                + "duplicate: One\n"
                + "duplicate: Two\n";

        var facts = new YamlDocumentParser().Parse(duplicateSource);

        Assert.Equal(YamlDocumentState.Complete, facts.State);
        Assert.True(facts.HasAliases);
        Assert.True(facts.HasUnsupportedMappings);
        var root = Assert.IsType<YamlNode>(facts.Root);
        var alias = MappingValue(root, "alias");
        Assert.Equal(YamlNodeKind.Alias, alias.Kind);
        Assert.Equal("*shared", Slice(duplicateSource, alias.Span));
        var duplicateEntries = Assert.IsAssignableFrom<IReadOnlyList<YamlMappingEntry>>(root.Mapping)
            .Where(entry => entry.Key.Scalar?.Value == "duplicate")
            .ToArray();
        Assert.Equal(["One", "Two"], duplicateEntries.Select(entry => entry.Value.Scalar?.Value));
        Assert.All(duplicateEntries, entry => Assert.Equal("duplicate", Slice(duplicateSource, entry.Key.Span)));
        Assert.False(root.TryGetMappingValue("duplicate", out var duplicate));
        Assert.Null(duplicate);

        const string complexKeySource = "? [complex]\n: value\n";
        var complexKeyFacts = new YamlDocumentParser().Parse(complexKeySource);
        Assert.Equal(YamlDocumentState.Complete, complexKeyFacts.State);
        Assert.False(complexKeyFacts.HasAliases);
        Assert.True(complexKeyFacts.HasUnsupportedMappings);
        var complexRoot = Assert.IsType<YamlNode>(complexKeyFacts.Root);
        var complexEntry = Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<YamlMappingEntry>>(complexRoot.Mapping));
        Assert.Equal(YamlNodeKind.Sequence, complexEntry.Key.Kind);
        Assert.Equal("[complex]", Slice(complexKeySource, complexEntry.Key.Span));
        Assert.Equal("value", complexEntry.Value.Scalar?.Value);
    }

    [Fact(DisplayName = "Shared YAML parsing distinguishes empty documents from malformed and multiple documents")]
    [Trait("Feature", "yaml-documents"), Trait("Evidence", "Unit")]
    public void EmptyMalformedAndMultipleDocumentsHaveExactStates()
    {
        var empty = new YamlDocumentParser().Parse(string.Empty);
        Assert.Equal(YamlDocumentState.Complete, empty.State);
        Assert.Null(empty.Root);

        var commentOnly = new YamlDocumentParser().Parse("# comment\n");
        Assert.Equal(YamlDocumentState.Complete, commentOnly.State);
        Assert.Null(commentOnly.Root);

        foreach (var source in new[] { "open-forge: [\n", "first: one\n---\nsecond: two\n" })
        {
            var unavailable = new YamlDocumentParser().Parse(source);
            Assert.Equal(source, unavailable.Source);
            Assert.Equal(YamlDocumentState.Unavailable, unavailable.State);
            Assert.Null(unavailable.Root);
            Assert.False(unavailable.HasAliases);
            Assert.False(unavailable.HasUnsupportedMappings);
        }
    }

    [Fact(DisplayName = "Shared YAML facts own immutable children and reject null or out-of-source shapes")]
    [Trait("Feature", "yaml-documents"), Trait("Evidence", "Unit")]
    public void ModelSnapshotsAndSpanInvariantsAreEnforced()
    {
        var child = YamlNode.FromScalar(new YamlScalar("A", new YamlTextSpan(0, 1)));
        var children = new List<YamlNode> { child };
        var sequence = YamlNode.FromSequence(new YamlTextSpan(0, 1), children);
        children.Clear();

        var snapshot = Assert.IsAssignableFrom<IReadOnlyList<YamlNode>>(sequence.Sequence);
        Assert.Single(snapshot);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<YamlNode>)snapshot).Add(child));

        var nullChildren = new object?[] { null }.Cast<YamlNode>();
        Assert.Throws<ArgumentException>(() =>
            YamlNode.FromSequence(new YamlTextSpan(0, 0), nullChildren));

        var entries = new List<YamlMappingEntry> { new(child, child) };
        var mapping = YamlNode.FromMapping(new YamlTextSpan(0, 1), entries);
        entries.Clear();
        var mappingSnapshot = Assert.IsAssignableFrom<IReadOnlyList<YamlMappingEntry>>(mapping.Mapping);
        Assert.Single(mappingSnapshot);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<YamlMappingEntry>)mappingSnapshot).Add(new YamlMappingEntry(child, child)));

        var nullEntries = new object?[] { null }.Cast<YamlMappingEntry>();
        Assert.Throws<ArgumentException>(() =>
            YamlNode.FromMapping(new YamlTextSpan(0, 0), nullEntries));

        var outsideSource = YamlNode.FromScalar(new YamlScalar("A", new YamlTextSpan(1, 1)));
        Assert.Throws<ArgumentException>(() =>
            new YamlDocumentFacts("A", YamlDocumentState.Complete, outsideSource));

        var outsideParent = YamlNode.FromSequence(new YamlTextSpan(0, 1), [outsideSource]);
        Assert.Throws<ArgumentException>(() =>
            new YamlDocumentFacts("AA", YamlDocumentState.Complete, outsideParent));
    }

    private static YamlNode MappingValue(YamlNode mapping, string key)
    {
        Assert.Equal(YamlNodeKind.Mapping, mapping.Kind);
        Assert.True(mapping.TryGetMappingValue(key, out var value));
        return Assert.IsType<YamlNode>(value);
    }

    private static string Slice(string source, YamlTextSpan span)
        => source[span.Start..span.End];
}
