using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown;

public sealed class FrameworkMarkdownDocumentWriterTests
{
    [Theory(DisplayName = "Complete and optional Markdown writers preserve exact body bytes in either metadata form")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Boundary", "Output"), Trait("Feature", "framework-markdown-document"), Trait("Evidence", "Unit")]
    public void BothFormsPreserveExactBodyBytes(bool root)
    {
        var form = root ? FrontmatterForm.Root : FrontmatterForm.Scoped;
        const string body = "\r\n# Body\r\n\nUnicode: café 工作\0\uFEFF";
        var writer = new FrameworkMarkdownDocumentWriter();
        var complete = writer.Write(new FrameworkDocumentMetadata("Description", ["Docs"], null), body, form);
        var optional = writer.WriteOptional(new FrameworkDocumentMetadataEmission("Description", [], null), body, form);

        foreach (var bytes in new[] { complete, optional })
        {
            var source = Encoding.UTF8.GetString(bytes.AsSpan());
            var document = new MarkdownDocumentParser().Parse(source);
            var bodySpan = Assert.IsType<MarkdownTextSpan>(document.BodySpan);
            var bodyStart = Encoding.UTF8.GetByteCount(source.AsSpan(0, bodySpan.Start));
            Assert.Equal(Encoding.UTF8.GetBytes(body), bytes.AsSpan()[bodyStart..].ToArray());
            Assert.Equal((byte)'-', bytes[0]);
            var facts = new FrameworkDocumentMetadataParser().Parse(document, FrameworkMetadataReadScope.RoutedSource);
            Assert.Equal(form, facts.Syntax.AuthoredForm);
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Optional Markdown writer omits tags and preserves exact UTF8 body bytes")]
    [Trait("Feature", "framework-markdown-document"), Trait("Evidence", "Unit")]
    public void OptionalWriterPreservesBodyWithoutInventingMetadata()
    {
        const string body = "\r\n# Body\r\n\nUnicode: café\0";
        const string expected = "---\nopen-forge:\n  description: Description\n---\n" + body;
        var bytes = new FrameworkMarkdownDocumentWriter().WriteOptional(
            new FrameworkDocumentMetadataEmission("Description", [], null), body, FrontmatterForm.Scoped);

        Assert.Equal(Encoding.UTF8.GetBytes(expected), bytes.ToArray());
        Assert.Equal((byte)'-', bytes[0]);
        var document = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(bytes.AsSpan()));
        var facts = new FrameworkDocumentMetadataParser().Parse(document, FrameworkMetadataReadScope.RoutedSource);
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

        var bytes = new FrameworkMarkdownDocumentWriter().Write(metadata, body, FrontmatterForm.Scoped);

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
                .Write(metadata, body, FrontmatterForm.Scoped)
                .AsSpan());
        var document = new MarkdownDocumentParser().Parse(source);
        var facts = new FrameworkDocumentMetadataParser().Parse(document, FrameworkMetadataReadScope.RoutedSource);

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
                .Write(metadata, string.Empty, FrontmatterForm.Scoped)
                .AsSpan());

        Assert.Equal(
            "---\nopen-forge:\n  description: Description\n  tags: [CurrentTruth]\n---\n",
            output);
        Assert.EndsWith("---\n", output, StringComparison.Ordinal);
    }
}
