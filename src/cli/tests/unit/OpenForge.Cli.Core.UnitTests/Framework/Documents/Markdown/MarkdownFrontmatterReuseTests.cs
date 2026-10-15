using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;
using OpenForge.Cli.Core.Framework.Documents.Yaml;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown;

[Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class MarkdownFrontmatterReuseTests
{
    [Fact(DisplayName = "Metadata scopes and completion parse a retained document's YAML once")]
    public void MetadataConsumersShareOneYamlParse()
    {
        const string source = "---\nopen-forge:\n  description: Note\n  tags: [Guidance]\n---\n# Note\n";
        var yamlReads = 0;
        var document = new MarkdownDocumentParser(text =>
        {
            yamlReads++;
            return new YamlDocumentParser().Parse(text);
        }).Parse(source);
        Assert.Equal(0, yamlReads);
        var metadata = new FrameworkDocumentMetadataParser();

        Assert.Equal(FrameworkDocumentMetadataState.Complete, metadata.Parse(document, FrameworkMetadataReadScope.RoutedSource).State);
        Assert.Equal(FrameworkDocumentMetadataState.Complete, metadata.Parse(document, FrameworkMetadataReadScope.ScopedOnly).State);
        var markdownReads = 0;
        var completion = new FrameworkMetadataCompleter(text =>
        {
            markdownReads++;
            return new MarkdownDocumentParser().Parse(text);
        }).Complete(new()
        {
            Bytes = [.. Encoding.UTF8.GetBytes(source)],
            CanonicalPath = ".agents/guidance/note.md",
            Form = FrontmatterForm.Scoped,
            DefaultTags = ["Guidance"],
            Document = document,
        });

        Assert.Equal(1, yamlReads);
        Assert.Equal(0, markdownReads);
        Assert.Same(document, completion.Document);
        Assert.Equal(Encoding.UTF8.GetBytes(source), completion.Bytes.ToArray());
    }

    [Fact(DisplayName = "Completion reparses fresh bytes when the retained document has changed")]
    public void ChangedBytesDiscardRetainedDocument()
    {
        const string original = "# Original\n";
        const string current = "# Current\n";
        var reads = 0;
        var completion = new FrameworkMetadataCompleter(text =>
        {
            reads++;
            return new MarkdownDocumentParser().Parse(text);
        }).Complete(new()
        {
            Bytes = [.. Encoding.UTF8.GetBytes(current)],
            CanonicalPath = ".agents/guidance/note.md",
            Form = FrontmatterForm.Scoped,
            DefaultTags = ["Guidance"],
            Document = new MarkdownDocumentParser().Parse(original),
        });

        Assert.Equal(2, reads);
        Assert.Equal("Current", completion.Description);
        Assert.EndsWith(current, Encoding.UTF8.GetString(completion.Bytes.AsSpan()), StringComparison.Ordinal);
    }
}
