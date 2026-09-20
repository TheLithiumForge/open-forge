using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown;

public sealed class FrameworkMarkdownDocumentWriterTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Optional Markdown writer omits tags and preserves exact UTF8 body bytes")]
    [Trait("Feature", "framework-markdown-document"), Trait("Evidence", "Unit")]
    public void OptionalWriterPreservesBodyWithoutInventingMetadata()
    {
        const string body = "\r\n# Body\r\n\nUnicode: café\0";
        const string expected = "---\nopen-forge:\n  description: Description\n---\n" + body;
        var bytes = new FrameworkMarkdownDocumentWriter().WriteOptional(
            new FrameworkDocumentMetadataEmission("Description", [], null), body);

        Assert.Equal(Encoding.UTF8.GetBytes(expected), bytes.ToArray());
        Assert.Equal((byte)'-', bytes[0]);
        var document = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(bytes.AsSpan()));
        var facts = new FrameworkDocumentMetadataParser().Parse(document);
        Assert.Equal(FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Equal("Description", facts.ObservedDescription);
        Assert.Empty(facts.ObservedTags);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework Markdown writer emits canonical frontmatter and preserves the exact body bytes"), Trait("Feature", "framework-markdown-document"), Trait("Evidence", "Unit")]
    public void WritesCanonicalFrontmatterAndExactBody()
    {
        var metadata = new FrameworkDocumentMetadata(
            description: "Description",
            tags: ["First", "Second"],
            responsibility: "Responsibility");
        const string body = "\r\n# Body\r\n\nUnicode: café\0";
        var expected = $"---\nopen-forge:\n  description: Description\n  tags: [First, Second]\n  responsibility: Responsibility\n---\n{body}";

        var bytes = new FrameworkMarkdownDocumentWriter().Write(metadata, body);

        Assert.Equal(expected, Encoding.UTF8.GetString(bytes.AsSpan()));
        Assert.Equal((byte)'-', bytes[0]);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), bytes.ToArray());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework Markdown writer output round-trips through the Markdown and metadata parsers"), Trait("Feature", "framework-markdown-document"), Trait("Evidence", "Unit")]
    public void WrittenDocumentRoundTripsThroughCanonicalParsers()
    {
        var metadata = new FrameworkDocumentMetadata(
            description: "A Unicode description: café",
            tags: ["Évidence2", "工作-2"],
            responsibility: "Owns the routed document.");
        const string body = "\n# Body\n\nPreserve this body exactly.\n";

        var source = Encoding.UTF8.GetString(
            new FrameworkMarkdownDocumentWriter()
                .Write(metadata, body)
                .AsSpan());
        var document = new MarkdownDocumentParser().Parse(source);
        var facts = new FrameworkDocumentMetadataParser().Parse(document);

        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        var actual = Assert.IsType<FrameworkDocumentMetadata>(facts.Metadata);
        Assert.Equal(metadata.Description, actual.Description);
        Assert.Equal(metadata.Tags, actual.Tags);
        Assert.Equal(metadata.Responsibility, actual.Responsibility);
        var bodySpan = Assert.IsType<MarkdownTextSpan>(document.BodySpan);
        Assert.Equal(body, source.Substring(bodySpan.Start, bodySpan.Length));
        Assert.Contains("tags: [Évidence2, 工作-2]", source, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework Markdown writer closes metadata-only documents with one canonical delimiter newline"), Trait("Feature", "framework-markdown-document"), Trait("Evidence", "Unit")]
    public void MetadataOnlyOutputEndsAtClosingDelimiterNewline()
    {
        var metadata = new FrameworkDocumentMetadata(
            description: "Description",
            tags: ["CurrentTruth"],
            responsibility: null);

        var output = Encoding.UTF8.GetString(
            new FrameworkMarkdownDocumentWriter()
                .Write(metadata, string.Empty)
                .AsSpan());

        Assert.Equal(
            "---\nopen-forge:\n  description: Description\n  tags: [CurrentTruth]\n---\n",
            output);
        Assert.EndsWith("---\n", output, StringComparison.Ordinal);
    }
}
