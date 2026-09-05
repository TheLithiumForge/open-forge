namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal sealed record MarkdownDocumentFacts
{
    internal MarkdownDocumentFacts(
        string source,
        MarkdownDocumentStructure structure,
        MarkdownInlineFacts inlineFacts)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(structure);
        ArgumentNullException.ThrowIfNull(inlineFacts);

        var materialized = MarkdownDocumentMaterialization.Create(structure, inlineFacts);
        MarkdownDocumentStructureValidator.Validate(source, structure, materialized);
        MarkdownDocumentInlineValidator.Validate(source, structure.BodySpan, materialized);

        Source = source;
        Frontmatter = structure.Frontmatter;
        BodySpan = structure.BodySpan;
        Headings = Array.AsReadOnly(materialized.Headings);
        Sections = Array.AsReadOnly(materialized.Sections);
        VisibleText = Array.AsReadOnly(materialized.VisibleText);
        OpaqueSpans = Array.AsReadOnly(materialized.OpaqueSpans);
        Links = Array.AsReadOnly(materialized.Links);
        Images = Array.AsReadOnly(materialized.Images);
        GeneratedRegion = structure.GeneratedRegion;
    }

    internal string Source { get; }

    internal MarkdownFrontmatterBoundary Frontmatter { get; }

    internal MarkdownTextSpan? BodySpan { get; }

    internal IReadOnlyList<MarkdownHeadingFact> Headings { get; }

    internal IReadOnlyList<MarkdownSectionFact> Sections { get; }

    internal IReadOnlyList<MarkdownVisibleTextFact> VisibleText { get; }

    internal IReadOnlyList<MarkdownOpaqueSpan> OpaqueSpans { get; }

    internal IReadOnlyList<MarkdownLinkFact> Links { get; }

    internal IReadOnlyList<MarkdownLinkFact> Images { get; }

    internal MarkdownGeneratedRegionFact GeneratedRegion { get; }

}
