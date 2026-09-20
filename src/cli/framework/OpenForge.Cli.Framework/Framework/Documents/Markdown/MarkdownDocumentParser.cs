using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal sealed class MarkdownDocumentParser
{
    private readonly MarkdownFrontmatterParser _frontmatterParser = new();

    internal MarkdownDocumentFacts Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var frontmatter = _frontmatterParser.Parse(source);
        if (frontmatter.State == MarkdownFrontmatterState.Unavailable)
        {
            return new MarkdownDocumentFacts(
                source,
                new MarkdownDocumentStructure(
                    frontmatter,
                    null,
                    [],
                    [],
                    MarkdownGeneratedRegionFact.Unavailable("The Markdown body boundary is unavailable.")),
                new MarkdownInlineFacts([], [], [], []));
        }

        var bodyStart = frontmatter.BodyStart ?? throw new InvalidOperationException(
            "A Markdown document with an established frontmatter state must establish its body start.");
        var bodySpan = new MarkdownTextSpan(bodyStart, source.Length - bodyStart);
        var parseStart = bodyStart;
        if (parseStart == 0 && source.Length > 0 && source[0] == '\uFEFF')
        {
            parseStart++;
        }

        var document = Markdig.Markdown.Parse(source[parseStart..], MarkdownPipelineFactory.Get());
        var headings = new List<MarkdownHeadingFact>();
        var inlineFacts = new MarkdownInlineFactCollections();

        foreach (var block in EnumerateBlocks(document))
        {
            if (block is HeadingBlock heading)
            {
                headings.Add(CreateHeadingFact(heading, parseStart));
            }

            if (block is CodeBlock or HtmlBlock)
            {
                MarkdownInlineFactCollector.AddOpaqueSpan(
                    inlineFacts.OpaqueSpans,
                    block.Span,
                    parseStart,
                    block is CodeBlock);
                continue;
            }

            if (block is LeafBlock { Inline: { } inline })
            {
                MarkdownInlineFactCollector.Collect(
                    inline,
                    parseStart,
                    inlineFacts);
            }
        }

        var sections = CreateSections(headings, bodySpan);
        var generatedRegion = MarkdownEntriesSectionReader.Parse(source, bodySpan, headings, document, parseStart);

        return new MarkdownDocumentFacts(
            source,
            new MarkdownDocumentStructure(
                frontmatter,
                bodySpan,
                headings,
                sections,
                generatedRegion),
            new MarkdownInlineFacts(
                inlineFacts.VisibleText,
                inlineFacts.OpaqueSpans,
                inlineFacts.Links,
                inlineFacts.Images));
    }

    private static MarkdownHeadingFact CreateHeadingFact(HeadingBlock heading, int bodyStart)
    {
        var span = MarkdownInlineFactCollector.ToDocumentSpan(heading.Span, bodyStart)
            ?? throw new InvalidOperationException("Markdig returned a heading without a source span.");
        return new MarkdownHeadingFact(
            MarkdownInlineTextReader.ReadHeadingText(heading.Inline),
            heading.Level,
            heading.IsSetext ? MarkdownHeadingForm.Setext : MarkdownHeadingForm.Atx,
            !heading.IsSetext,
            ReadFragmentIdentifier(heading),
            span,
            isTopLevel: heading.Parent is MarkdownDocument);
    }

    private static string? ReadFragmentIdentifier(HeadingBlock heading)
    {
        if (heading.IsSetext || !MarkdownInlineTextReader.HasSupportedHeadingText(heading.Inline))
        {
            return null;
        }

        var identifier = heading.TryGetAttributes()?.Id;
        return string.IsNullOrWhiteSpace(identifier) ? null : identifier;
    }

    private static IReadOnlyList<MarkdownSectionFact> CreateSections(
        IReadOnlyList<MarkdownHeadingFact> headings,
        MarkdownTextSpan bodySpan)
    {
        var sections = new MarkdownSectionFact[headings.Count];
        for (var index = 0; index < headings.Count; index++)
        {
            var end = bodySpan.End;
            for (var next = index + 1; next < headings.Count; next++)
            {
                if (headings[next].Level <= headings[index].Level)
                {
                    end = headings[next].Span.Start;
                    break;
                }
            }

            sections[index] = new MarkdownSectionFact(
                headings[index],
                new MarkdownTextSpan(headings[index].Span.Start, end - headings[index].Span.Start));
        }
        return sections;
    }

    private static IEnumerable<Block> EnumerateBlocks(ContainerBlock container)
    {
        foreach (var block in container)
        {
            yield return block;
            if (block is ContainerBlock child)
            {
                foreach (var descendant in EnumerateBlocks(child))
                {
                    yield return descendant;
                }
            }
        }
    }
}
