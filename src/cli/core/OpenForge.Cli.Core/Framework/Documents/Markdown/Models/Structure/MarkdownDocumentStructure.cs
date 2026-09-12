namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

internal sealed record MarkdownDocumentStructure
{
    internal MarkdownDocumentStructure(
        MarkdownFrontmatterBoundary frontmatter,
        MarkdownTextSpan? bodySpan,
        IEnumerable<MarkdownHeadingFact> headings,
        IEnumerable<MarkdownSectionFact> sections,
        MarkdownGeneratedRegionFact generatedRegion)
    {
        ArgumentNullException.ThrowIfNull(frontmatter);
        ArgumentNullException.ThrowIfNull(headings);
        ArgumentNullException.ThrowIfNull(sections);
        ArgumentNullException.ThrowIfNull(generatedRegion);
        Frontmatter = frontmatter;
        BodySpan = bodySpan;
        Headings = Array.AsReadOnly(headings.ToArray());
        Sections = Array.AsReadOnly(sections.ToArray());
        GeneratedRegion = generatedRegion;
    }

    internal MarkdownFrontmatterBoundary Frontmatter { get; }

    internal MarkdownTextSpan? BodySpan { get; }

    internal IReadOnlyList<MarkdownHeadingFact> Headings { get; }

    internal IReadOnlyList<MarkdownSectionFact> Sections { get; }

    internal MarkdownGeneratedRegionFact GeneratedRegion { get; }
}
