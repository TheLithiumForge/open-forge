using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

// Framework Markdown syntax owns the section boundary shared by Entries and Axioms.
internal static class MarkdownSemanticSectionReader
{
    internal static IReadOnlyList<MarkdownSectionFact> Find(
        string source,
        MarkdownTextSpan body,
        IReadOnlyList<MarkdownHeadingFact> headings,
        string name)
    {
        var sections = new List<MarkdownSectionFact>();
        for (var index = 0; index < headings.Count; index++)
        {
            var heading = headings[index];
            if (!heading.IsTopLevel || !heading.IsCanonical || heading.Level != 2
                || !source[heading.Span.Start..heading.Span.End].TrimEnd(' ', '\t').Equals($"## {name}", StringComparison.Ordinal))
            {
                continue;
            }

            var end = body.End;
            for (var next = index + 1; next < headings.Count; next++)
            {
                if (headings[next].IsTopLevel && headings[next].Level <= heading.Level)
                {
                    end = headings[next].Span.Start;
                    break;
                }
            }

            sections.Add(new MarkdownSectionFact(heading, new MarkdownTextSpan(heading.Span.Start, end - heading.Span.Start)));
        }

        return sections;
    }
}
