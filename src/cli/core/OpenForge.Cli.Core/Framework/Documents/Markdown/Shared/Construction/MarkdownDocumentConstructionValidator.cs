using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.Construction;

internal static class MarkdownDocumentConstructionValidator
{
    internal static void Validate(
        MarkdownDocumentStructure structure,
        MarkdownInlineFacts inlineFacts)
    {
        if (structure.Headings.Any(heading => heading is null)
            || structure.Sections.Any(section => section is null)
            || inlineFacts.VisibleText.Any(fact => fact is null)
            || inlineFacts.OpaqueSpans.Any(span => span is null)
            || inlineFacts.Links.Any(link => link is null)
            || inlineFacts.Images.Any(image => image is null))
        {
            throw new ArgumentException("Markdown facts cannot contain null members.");
        }

        if (inlineFacts.Links.Any(link => link.IsImage)
            || inlineFacts.Images.Any(image => !image.IsImage))
        {
            throw new ArgumentException(
                "Markdown link and image facts must retain their exact resource kind.",
                nameof(inlineFacts));
        }
    }
}
