using System.Text;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Documents;

public sealed class FindFrontmatterReaderRedTests
{
    [Fact(DisplayName = "Find frontmatter tag sequences preserve authored order and scalar marks")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void BlockAndFlowTagSequencesRetainSourceOrderAndMarks()
    {
        AssertTagSequence(
            "---\n"
                + "open-forge:\n"
                + "  description: Example\n"
                + "  tags:\n"
                + "    - First\n"
                + "    - Second\n"
                + "---\n"
                + "Body\n",
            [("First", 5, 7), ("Second", 6, 7)]);
        AssertTagSequence(
            "---\n"
                + "open-forge:\n"
                + "  description: Example\n"
                + "  tags: [Second, First]\n"
                + "---\n"
                + "Body\n",
            [("Second", 4, 10), ("First", 4, 18)]);
    }

    [Fact(DisplayName = "Find frontmatter preserves valid quoted Unicode scalar spans")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void QuotedEscapedUnicodeTagsPreserveScalarSpans()
    {
        const string source = "---\n"
            + "open-forge:\n"
            + "  description: Example\n"
            + "  tags: [\"Caf\\u00E9\", \"工作\"]\n"
            + "---\n";

        var result = Read(source);
        var firstToken = "\"Caf\\u00E9\"";
        var secondToken = "\"工作\"";
        var firstStart = source.IndexOf(firstToken, StringComparison.Ordinal);
        var secondStart = source.IndexOf(secondToken, StringComparison.Ordinal);

        Assert.Equal(FindFrontmatterAvailability.Complete, result.Availability);
        Assert.Equal(["Café", "工作"], result.Tags.Select(tag => tag.Authored));
        Assert.Equal(
            ExpectedLocation(source, firstStart, firstToken.Length, 4, 10),
            result.Tags[0].Location);
        Assert.Equal(
            ExpectedLocation(source, secondStart, secondToken.Length, 4, 23),
            result.Tags[1].Location);
    }

    [Fact(DisplayName = "Find ignores Rune and reads Open Forge when both roots are present")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void RuneIsOpaqueAndOpenForgeRemainsSoleMetadataAuthority()
    {
        AssertCompleteWithoutTags(
            "---\nrune:\n  description: Legacy\n  tags: [Legacy]\n---\n",
            null,
            SourceDocumentForm.Markdown,
            ".agents/example.md");
        AssertTagSequence(
            "---\n"
                + "open-forge:\n"
                + "  description: Example\n"
                + "  tags: [First, Second]\n"
                + "rune:\n"
                + "  description: &legacy Legacy\n"
                + "  tags: [*legacy]\n"
                + "---\n",
            [("First", 4, 10), ("Second", 4, 17)]);
    }

    [Fact(DisplayName = "Find frontmatter keeps missing, empty, commented, unknown, and skill metadata complete")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void MissingEmptyCommentsAndUnknownFieldsRemainComplete()
    {
        AssertCompleteWithoutTags("# Body\n", null, SourceDocumentForm.Markdown, ".agents/example.md");
        AssertCompleteWithoutTags(
            "---\n"
                + "unknown: [Ignored]\n"
                + "---\n"
                + "Body\n",
            null,
            SourceDocumentForm.Markdown,
            ".agents/example.md");
        AssertCompleteWithoutTags(
            "---\n"
                + "# comment\n"
                + "unknown: [Ignored]\n"
                + "open-forge:\n"
                + "  # nested comment\n"
                + "  tags: []\n"
                + "---\n"
                + "Body\n",
            null,
            SourceDocumentForm.Markdown,
            ".agents/example.md");
        AssertCompleteWithoutTags(
            "---\n"
                + "open-forge: null\n"
                + "---\n"
                + "Body\n",
            null,
            SourceDocumentForm.Markdown,
            ".agents/example.md");
        AssertCompleteWithoutTags(
            "---\n"
                + "unknown:\n"
                + "  tags: [WrongScope]\n"
                + "open-forge:\n"
                + "  description: Known\n"
                + "  responsibility: Ignored\n"
                + "---\n"
                + "Body\n",
            null,
            SourceDocumentForm.Markdown,
            ".agents/example.md");
        AssertCompleteWithoutTags(
            "---\n"
                + "name: Example\n"
                + "description: Skill description\n"
                + "tags: [NotAFindTag]\n"
                + "---\n"
                + "Body\n",
            "Skill description",
            SourceDocumentForm.Skill,
            ".agents/skills/example/SKILL.md");
    }

    [Fact(DisplayName = "Find frontmatter makes malformed or structurally unavailable tag metadata unavailable")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void MalformedWrongDuplicateAndAliasShapesBecomeUnavailableWithoutGuessing()
    {
        AssertUnavailable(
            "---\n"
                + "open-forge:\n"
                + "  tags: [One\n"
                + "---\n"
                + "Body\n");
        AssertUnavailable(
            "---\n"
                + "open-forge:\n"
                + "  description: Example\n"
                + "  tags: { value: One }\n"
                + "---\n"
                + "Body\n");
        AssertUnavailable(
            "---\n"
                + "open-forge:\n"
                + "  description: Example\n"
                + "  tags: ['', ' ', 1Invalid, Invalid--Tag]\n"
                + "---\n"
                + "Body\n");
        AssertUnavailable(
            "---\n"
                + "open-forge:\n"
                + "  tags: [One]\n"
                + "open-forge:\n"
                + "  tags: [Two]\n"
                + "---\n"
                + "Body\n");
        AssertUnavailable(
            "---\n"
                + "values: &values [One]\n"
                + "open-forge:\n"
                + "  tags: *values\n"
                + "---\n"
                + "Body\n");
    }

    private static void AssertTagSequence(
        string source,
        IReadOnlyList<(string Value, int Line, int Column)> expected)
    {
        var result = Read(source);

        Assert.Equal(FindFrontmatterAvailability.Complete, result.Availability);
        Assert.Equal("Example", result.Description);
        Assert.Equal(expected.Select(value => value.Value), result.Tags.Select(tag => tag.Authored));
        Assert.Equal(expected.Count, result.Tags.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            var start = source.IndexOf(expected[index].Value, StringComparison.Ordinal);
            Assert.True(start >= 0);
            Assert.Equal(
                ExpectedLocation(source, start, expected[index].Value.Length, expected[index].Line, expected[index].Column),
                result.Tags[index].Location);
        }
    }

    private static void AssertCompleteWithoutTags(
        string source,
        string? expectedDescription,
        SourceDocumentForm form,
        string path)
    {
        var result = Read(source, form, path);

        Assert.Equal(FindFrontmatterAvailability.Complete, result.Availability);
        Assert.Equal(expectedDescription, result.Description);
        Assert.Empty(result.Tags);
    }

    private static void AssertUnavailable(string source)
    {
        var result = Read(source);

        Assert.Equal(FindFrontmatterAvailability.Unavailable, result.Availability);
        Assert.Null(result.Description);
        Assert.Empty(result.Tags);
    }

    private static FindFrontmatterFacts Read(
        string source,
        SourceDocumentForm form = SourceDocumentForm.Markdown,
        string path = ".agents/example.md")
    {
        return new FindFrontmatterReader().Read(
            new FindFrontmatterInput(
                new SourceLayer(
                    path,
                    Path.GetFullPath(Path.Combine(Path.GetTempPath(), "find-frontmatter-red", path.Replace('/', Path.DirectorySeparatorChar))),
                    form,
                    SourceLayerKind.Base),
                CompleteOrMissingDocument(source)));
    }

    private static MarkdownDocumentFacts CompleteOrMissingDocument(string source)
    {
        if (!source.StartsWith("---", StringComparison.Ordinal))
        {
            return new MarkdownDocumentFacts(
                source,
                new MarkdownFrontmatterBoundary(MarkdownFrontmatterState.Missing, null, null, 0),
                new MarkdownTextSpan(0, source.Length),
                [],
                [],
                [],
                [],
                [],
                MarkdownGeneratedRegionFact.Absent());
        }

        var openingLineEnd = source.IndexOf('\n') + 1;
        var closingMarker = source.IndexOf("\n---", openingLineEnd, StringComparison.Ordinal);
        var closingStart = closingMarker + 1;
        var closingEnd = closingStart + 3;
        var bodyStart = closingEnd + (source[closingEnd..].StartsWith("\r\n", StringComparison.Ordinal) ? 2 : 1);
        return new MarkdownDocumentFacts(
            source,
            new MarkdownFrontmatterBoundary(
                MarkdownFrontmatterState.Complete,
                new MarkdownTextSpan(0, closingEnd),
                new MarkdownTextSpan(openingLineEnd, closingStart - openingLineEnd),
                bodyStart),
            new MarkdownTextSpan(bodyStart, source.Length - bodyStart),
            [],
            [],
            [],
            [],
            [],
            MarkdownGeneratedRegionFact.Absent());
    }

    private static SourceLocation ExpectedLocation(
        string source,
        int start,
        int length,
        int line,
        int column)
    {
        return new SourceLocation(
            line,
            column,
            Encoding.UTF8.GetByteCount(source[..start]),
            Encoding.UTF8.GetByteCount(source.Substring(start, length)));
    }
}
