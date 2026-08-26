using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal sealed record MarkdownDocumentFacts
{
    internal MarkdownDocumentFacts(
        string source,
        MarkdownFrontmatterBoundary frontmatter,
        MarkdownTextSpan? bodySpan,
        IEnumerable<MarkdownHeadingFact> headings,
        IEnumerable<MarkdownSectionFact> sections,
        IEnumerable<MarkdownVisibleTextFact> visibleText,
        IEnumerable<MarkdownOpaqueSpan> opaqueSpans,
        IEnumerable<MarkdownLinkFact> links,
        MarkdownGeneratedRegionFact generatedRegion)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(frontmatter);
        ArgumentNullException.ThrowIfNull(headings);
        ArgumentNullException.ThrowIfNull(sections);
        ArgumentNullException.ThrowIfNull(visibleText);
        ArgumentNullException.ThrowIfNull(opaqueSpans);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(generatedRegion);

        var materializedHeadings = headings.ToArray();
        var materializedSections = sections.ToArray();
        var materializedVisibleText = visibleText.ToArray();
        var materializedOpaqueSpans = opaqueSpans.ToArray();
        var materializedLinks = links.ToArray();
        if (materializedHeadings.Any(heading => heading is null)
            || materializedSections.Any(section => section is null)
            || materializedVisibleText.Any(fact => fact is null)
            || materializedOpaqueSpans.Any(span => span is null)
            || materializedLinks.Any(link => link is null))
        {
            throw new ArgumentException("Markdown facts cannot contain null members.");
        }

        ValidateSpan(source, frontmatter.BlockSpan, nameof(frontmatter));
        ValidateSpan(source, frontmatter.YamlSpan, nameof(frontmatter));
        if (frontmatter.BodyStart is { } bodyStart
            && (bodyStart < 0 || bodyStart > source.Length))
        {
            throw new ArgumentException("The Markdown body boundary must be within the source.", nameof(frontmatter));
        }

        ValidateSpan(source, bodySpan, nameof(bodySpan));
        switch (frontmatter.State)
        {
            case MarkdownFrontmatterState.Missing:
                if (bodySpan is null || bodySpan.Start != 0 || bodySpan.End != source.Length)
                {
                    throw new ArgumentException("Missing frontmatter must establish the whole source as the body.", nameof(bodySpan));
                }

                break;
            case MarkdownFrontmatterState.Complete:
                if (frontmatter.BlockSpan?.Start != 0
                    || bodySpan is null
                    || bodySpan.Start != frontmatter.BodyStart
                    || bodySpan.End != source.Length)
                {
                    throw new ArgumentException("Complete frontmatter must establish exact frontmatter and body spans.", nameof(bodySpan));
                }

                break;
            case MarkdownFrontmatterState.Unavailable:
                if (bodySpan is not null)
                {
                    throw new ArgumentException("Unavailable frontmatter cannot establish a body span.", nameof(bodySpan));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(frontmatter), frontmatter.State, "The Markdown frontmatter state is not defined.");
        }

        foreach (var heading in materializedHeadings)
        {
            ValidateSpan(source, heading.Span, nameof(headings));
            ValidateBodySpan(heading.Span, bodySpan, nameof(headings));
        }

        foreach (var section in materializedSections)
        {
            ValidateSpan(source, section.Span, nameof(sections));
            ValidateBodySpan(section.Span, bodySpan, nameof(sections));
        }

        foreach (var fact in materializedVisibleText)
        {
            ValidateSpan(source, fact.Span, nameof(visibleText));
            ValidateBodySpan(fact.Span, bodySpan, nameof(visibleText));
        }

        foreach (var opaqueSpan in materializedOpaqueSpans)
        {
            ValidateSpan(source, opaqueSpan.Span, nameof(opaqueSpans));
            ValidateBodySpan(opaqueSpan.Span, bodySpan, nameof(opaqueSpans));
        }


        foreach (var link in materializedLinks)
        {
            ValidateSpan(source, link.Span, nameof(links));
            ValidateSpan(source, link.DestinationSpan, nameof(links));
            ValidateBodySpan(link.Span, bodySpan, nameof(links));
            if (link.DestinationSpan is { } destinationSpan)
            {
                ValidateBodySpan(destinationSpan, bodySpan, nameof(links));
            }
        }

        ValidateSpan(source, generatedRegion.RegionSpan, nameof(generatedRegion));
        ValidateSpan(source, generatedRegion.ContentSpan, nameof(generatedRegion));
        if (generatedRegion.RegionSpan is { } generatedRegionSpan)
        {
            ValidateBodySpan(generatedRegionSpan, bodySpan, nameof(generatedRegion));
        }

        ValidateHeadingOrder(materializedHeadings);
        ValidateSections(materializedHeadings, materializedSections, bodySpan);
        ValidateVisibleTextOrder(materializedVisibleText);
        ValidateOpaqueSpanOrder(materializedOpaqueSpans);
        ValidateLinkOrder(materializedLinks);

        Source = source;
        Frontmatter = frontmatter;
        BodySpan = bodySpan;
        Headings = Array.AsReadOnly(materializedHeadings);
        Sections = Array.AsReadOnly(materializedSections);
        VisibleText = Array.AsReadOnly(materializedVisibleText);
        OpaqueSpans = Array.AsReadOnly(materializedOpaqueSpans);
        Links = Array.AsReadOnly(materializedLinks);
        GeneratedRegion = generatedRegion;
    }

    internal string Source { get; }

    internal MarkdownFrontmatterBoundary Frontmatter { get; }

    internal MarkdownTextSpan? BodySpan { get; }

    internal IReadOnlyList<MarkdownHeadingFact> Headings { get; }

    internal IReadOnlyList<MarkdownSectionFact> Sections { get; }

    internal IReadOnlyList<MarkdownVisibleTextFact> VisibleText { get; }

    internal IReadOnlyList<MarkdownOpaqueSpan> OpaqueSpans { get; }

    internal IReadOnlyList<MarkdownLinkFact> Links { get; }

    internal MarkdownGeneratedRegionFact GeneratedRegion { get; }

    private static void ValidateSpan(string source, MarkdownTextSpan? span, string parameterName)
    {
        if (span is not null && (span.Start > source.Length || span.End > source.Length))
        {
            throw new ArgumentException("A Markdown span must be contained by its source.", parameterName);
        }
    }

    private static void ValidateBodySpan(
        MarkdownTextSpan span,
        MarkdownTextSpan? bodySpan,
        string parameterName)
    {
        if (bodySpan is null || span.Start < bodySpan.Start || span.End > bodySpan.End)
        {
            throw new ArgumentException("A body fact must be contained by the established body span.", parameterName);
        }
    }

    private static void ValidateHeadingOrder(IReadOnlyList<MarkdownHeadingFact> headings)
    {
        for (var index = 1; index < headings.Count; index++)
        {
            if (headings[index].Span.Start < headings[index - 1].Span.Start)
            {
                throw new ArgumentException("Markdown headings must retain source order.", nameof(headings));
            }
        }
    }

    private static void ValidateSections(
        IReadOnlyList<MarkdownHeadingFact> headings,
        IReadOnlyList<MarkdownSectionFact> sections,
        MarkdownTextSpan? bodySpan)
    {
        if (sections.Count != headings.Count)
        {
            throw new ArgumentException("Every Markdown heading must establish one section.", nameof(sections));
        }

        for (var index = 0; index < sections.Count; index++)
        {
            if (sections[index].Heading != headings[index])
            {
                throw new ArgumentException("Markdown sections must retain heading order and identity.", nameof(sections));
            }

            var expectedEnd = bodySpan?.End
                ?? throw new ArgumentException("Markdown sections require an established body span.", nameof(bodySpan));
            for (var next = index + 1; next < headings.Count; next++)
            {
                if (headings[next].Level <= headings[index].Level)
                {
                    expectedEnd = headings[next].Span.Start;
                    break;
                }
            }

            if (sections[index].Span.End != expectedEnd)
            {
                throw new ArgumentException("A Markdown section must end at its exact heading boundary.", nameof(sections));
            }
        }
    }

    private static void ValidateVisibleTextOrder(IReadOnlyList<MarkdownVisibleTextFact> facts)
    {
        for (var index = 1; index < facts.Count; index++)
        {
            if (facts[index].Span.Start < facts[index - 1].Span.Start)
            {
                throw new ArgumentException("Markdown visible-text facts must retain source order.", nameof(facts));
            }
        }
    }

    private static void ValidateOpaqueSpanOrder(IReadOnlyList<MarkdownOpaqueSpan> spans)
    {
        for (var index = 1; index < spans.Count; index++)
        {
            if (spans[index].Span.Start < spans[index - 1].Span.Start)
            {
                throw new ArgumentException("Markdown opaque spans must retain source order.", nameof(spans));
            }
        }
    }

    private static void ValidateLinkOrder(IReadOnlyList<MarkdownLinkFact> links)
    {
        for (var index = 1; index < links.Count; index++)
        {
            if (links[index].Span.Start < links[index - 1].Span.Start)
            {
                throw new ArgumentException("Markdown links must retain source order.", nameof(links));
            }
        }
    }
}
