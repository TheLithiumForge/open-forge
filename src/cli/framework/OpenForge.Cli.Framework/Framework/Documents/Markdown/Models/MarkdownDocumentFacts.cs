using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.Construction;

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

        MarkdownDocumentConstructionValidator.Validate(structure, inlineFacts);
        MarkdownDocumentStructureValidator.Validate(source, structure);
        MarkdownDocumentInlineValidator.Validate(source, structure.BodySpan, inlineFacts);

        Source = source;
        Frontmatter = structure.Frontmatter;
        BodySpan = structure.BodySpan;
        Headings = structure.Headings;
        Sections = structure.Sections;
        VisibleText = inlineFacts.VisibleText;
        OpaqueSpans = inlineFacts.OpaqueSpans;
        Links = inlineFacts.Links;
        Images = inlineFacts.Images;
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
