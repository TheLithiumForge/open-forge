namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal sealed class MarkdownDocumentMaterialization
{
    private MarkdownDocumentMaterialization(
        MarkdownHeadingFact[] headings,
        MarkdownSectionFact[] sections,
        MarkdownInlineFacts inlineFacts)
    {
        Headings = headings;
        Sections = sections;
        VisibleText = inlineFacts.VisibleText.ToArray();
        OpaqueSpans = inlineFacts.OpaqueSpans.ToArray();
        Links = inlineFacts.Links.ToArray();
        Images = inlineFacts.Images.ToArray();
    }

    internal MarkdownHeadingFact[] Headings { get; }

    internal MarkdownSectionFact[] Sections { get; }

    internal MarkdownVisibleTextFact[] VisibleText { get; }

    internal MarkdownOpaqueSpan[] OpaqueSpans { get; }

    internal MarkdownLinkFact[] Links { get; }

    internal MarkdownLinkFact[] Images { get; }

    internal static MarkdownDocumentMaterialization Create(
        MarkdownDocumentStructure structure,
        MarkdownInlineFacts inlineFacts)
    {
        var materialized = new MarkdownDocumentMaterialization(
            structure.Headings.ToArray(),
            structure.Sections.ToArray(),
            inlineFacts);
        if (materialized.Headings.Any(heading => heading is null)
            || materialized.Sections.Any(section => section is null)
            || materialized.VisibleText.Any(fact => fact is null)
            || materialized.OpaqueSpans.Any(span => span is null)
            || materialized.Links.Any(link => link is null)
            || materialized.Images.Any(image => image is null))
        {
            throw new ArgumentException("Markdown facts cannot contain null members.");
        }

        if (materialized.Links.Any(link => link.IsImage)
            || materialized.Images.Any(image => !image.IsImage))
        {
            throw new ArgumentException(
                "Markdown link and image facts must retain their exact resource kind.",
                nameof(inlineFacts));
        }

        return materialized;
    }
}
