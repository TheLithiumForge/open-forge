using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown;

public sealed class MarkdownFrontmatterParserRedTests
{
    [Theory(DisplayName = "Shared Markdown frontmatter parsing preserves exact LF and CRLF boundaries"),
        InlineData("---\nkey: value\n---\n# Body\n", 18, 4, 11, 19),
        InlineData("---\r\nkey: value\r\n---\r\n# Body\r\n", 20, 5, 12, 22)]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void CompleteBoundariesPreserveExactSourceSpans(
        string source,
        int blockLength,
        int yamlStart,
        int yamlLength,
        int bodyStart)
    {
        var boundary = new MarkdownFrontmatterParser().Parse(source);

        Assert.Equal(MarkdownFrontmatterState.Complete, boundary.State);
        Assert.Equal(new MarkdownTextSpan(0, blockLength), boundary.BlockSpan);
        Assert.Equal(new MarkdownTextSpan(yamlStart, yamlLength), boundary.YamlSpan);
        Assert.Equal(bodyStart, boundary.BodyStart);
        Assert.Equal(boundary, new MarkdownDocumentParser().Parse(source).Frontmatter);
    }

    [Fact(DisplayName = "Shared Markdown frontmatter parsing distinguishes missing and unterminated boundaries")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void MissingAndUnterminatedBoundariesRemainDistinct()
    {
        const string missingSource = "# Body\n";
        var missing = new MarkdownFrontmatterParser().Parse(missingSource);
        Assert.Equal(
            new MarkdownFrontmatterBoundary(MarkdownFrontmatterState.Missing, null, null, 0),
            missing);
        Assert.Equal(missing, new MarkdownDocumentParser().Parse(missingSource).Frontmatter);

        const string unavailableSource = "---\nkey: value\n# Body\n";
        var unavailable = new MarkdownFrontmatterParser().Parse(unavailableSource);
        Assert.Equal(
            new MarkdownFrontmatterBoundary(MarkdownFrontmatterState.Unavailable, null, null, null),
            unavailable);
        Assert.Equal(unavailable, new MarkdownDocumentParser().Parse(unavailableSource).Frontmatter);

        const string malformedYamlSource = "---\nopen-forge: [\n---\n# Body\n";
        var malformedYamlBoundary = new MarkdownFrontmatterParser().Parse(malformedYamlSource);
        Assert.Equal(
            new MarkdownFrontmatterBoundary(
                MarkdownFrontmatterState.Complete,
                new MarkdownTextSpan(0, 21),
                new MarkdownTextSpan(4, 14),
                22),
            malformedYamlBoundary);
        Assert.Equal(
            malformedYamlBoundary,
            new MarkdownDocumentParser().Parse(malformedYamlSource).Frontmatter);
    }
}
