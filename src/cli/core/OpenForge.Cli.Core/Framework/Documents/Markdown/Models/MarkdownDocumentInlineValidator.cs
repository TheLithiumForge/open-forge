namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal static class MarkdownDocumentInlineValidator
{
    internal static void Validate(
        string source,
        MarkdownTextSpan? bodySpan,
        MarkdownDocumentMaterialization facts)
    {
        foreach (var fact in facts.VisibleText)
        {
            MarkdownDocumentStructureValidator.ValidateBodyFact(
                source,
                fact.Span,
                bodySpan,
                nameof(facts));
        }

        foreach (var opaqueSpan in facts.OpaqueSpans)
        {
            MarkdownDocumentStructureValidator.ValidateBodyFact(
                source,
                opaqueSpan.Span,
                bodySpan,
                nameof(facts));
        }

        foreach (var link in facts.Links.Concat(facts.Images))
        {
            MarkdownDocumentStructureValidator.ValidateBodyFact(
                source,
                link.Span,
                bodySpan,
                nameof(facts));
            MarkdownDocumentStructureValidator.ValidateSpan(
                source,
                link.DestinationSpan,
                nameof(facts));
            if (link.DestinationSpan is { } destination)
            {
                MarkdownDocumentStructureValidator.ValidateBodySpan(
                    destination,
                    bodySpan,
                    nameof(facts));
            }
        }

        ValidateOrder(facts.VisibleText.Select(fact => fact.Span), "visible text");
        ValidateOrder(facts.OpaqueSpans.Select(fact => fact.Span), "opaque spans");
        ValidateOrder(facts.Links.Select(fact => fact.Span), "links");
        ValidateOrder(facts.Images.Select(fact => fact.Span), "images");
    }

    private static void ValidateOrder(IEnumerable<MarkdownTextSpan> spans, string kind)
    {
        MarkdownTextSpan? previous = null;
        foreach (var span in spans)
        {
            if (previous is not null && span.Start < previous.Start)
            {
                throw new ArgumentException($"Markdown {kind} must retain source order.", nameof(spans));
            }

            previous = span;
        }
    }
}
