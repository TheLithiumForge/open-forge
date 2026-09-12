using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.Construction;

internal static class MarkdownDocumentStructureValidator
{
    internal static void Validate(
        string source,
        MarkdownDocumentStructure structure)
    {
        ValidateSpan(source, structure.Frontmatter.BlockSpan, nameof(structure));
        ValidateSpan(source, structure.Frontmatter.YamlSpan, nameof(structure));
        if (structure.Frontmatter.BodyStart is { } bodyStart
            && (bodyStart < 0 || bodyStart > source.Length))
        {
            throw new ArgumentException("The Markdown body boundary must be within the source.", nameof(structure));
        }

        ValidateSpan(source, structure.BodySpan, nameof(structure));
        ValidateFrontmatter(source, structure);
        foreach (var heading in structure.Headings)
        {
            ValidateBodyFact(source, heading.Span, structure.BodySpan, nameof(structure));
        }

        foreach (var section in structure.Sections)
        {
            ValidateBodyFact(source, section.Span, structure.BodySpan, nameof(structure));
        }

        ValidateSpan(source, structure.GeneratedRegion.RegionSpan, nameof(structure));
        ValidateSpan(source, structure.GeneratedRegion.ContentSpan, nameof(structure));
        if (structure.GeneratedRegion.RegionSpan is { } region)
        {
            ValidateBodySpan(region, structure.BodySpan, nameof(structure));
        }

        ValidateHeadingOrder(structure.Headings);
        ValidateSections(structure.Headings, structure.Sections, structure.BodySpan);
    }

    internal static void ValidateBodyFact(
        string source,
        MarkdownTextSpan span,
        MarkdownTextSpan? bodySpan,
        string parameterName)
    {
        ValidateSpan(source, span, parameterName);
        ValidateBodySpan(span, bodySpan, parameterName);
    }

    internal static void ValidateSpan(
        string source,
        MarkdownTextSpan? span,
        string parameterName)
    {
        if (span is not null && (span.Start > source.Length || span.End > source.Length))
        {
            throw new ArgumentException("A Markdown span must be contained by its source.", parameterName);
        }
    }

    internal static void ValidateBodySpan(
        MarkdownTextSpan span,
        MarkdownTextSpan? bodySpan,
        string parameterName)
    {
        if (bodySpan is null || span.Start < bodySpan.Start || span.End > bodySpan.End)
        {
            throw new ArgumentException("A body fact must be contained by the established body span.", parameterName);
        }
    }

    private static void ValidateFrontmatter(
        string source,
        MarkdownDocumentStructure structure)
    {
        switch (structure.Frontmatter.State)
        {
            case MarkdownFrontmatterState.Missing:
                if (structure.BodySpan is null
                    || structure.BodySpan.Start != 0
                    || structure.BodySpan.End != source.Length)
                {
                    throw new ArgumentException("Missing frontmatter must establish the whole source as the body.", nameof(structure));
                }

                break;
            case MarkdownFrontmatterState.Complete:
                if (structure.Frontmatter.BlockSpan?.Start != 0
                    || structure.BodySpan is null
                    || structure.BodySpan.Start != structure.Frontmatter.BodyStart
                    || structure.BodySpan.End != source.Length)
                {
                    throw new ArgumentException("Complete frontmatter must establish exact frontmatter and body spans.", nameof(structure));
                }

                break;
            case MarkdownFrontmatterState.Unavailable:
                if (structure.BodySpan is not null)
                {
                    throw new ArgumentException("Unavailable frontmatter cannot establish a body span.", nameof(structure));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(structure),
                    structure.Frontmatter.State,
                    "The Markdown frontmatter state is not defined.");
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
}
