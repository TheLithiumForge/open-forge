using System.Globalization;
using OpenForge.Cli.Core.Presentation.Find.Models;
using OpenForge.Cli.Core.Presentation.Find.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Content;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Find.Shared.Rendering;

internal static class FindDataTextRenderer
{
    internal static CliTextDocument Render(FindData data, CliSelection selection, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

        var spans = new List<CliTextSpan>();
        var standard = selection.Detail >= CliDetail.Standard;
        var full = selection.Detail >= CliDetail.Full;
        var rows = data.Matches
            .Select(match => standard
                ? new[] { match.Id, match.Path, match.Description ?? string.Empty }
                : new[] { match.Id, match.Path })
            .ToArray();
        var tableLines = CliTable.Render(
                rows,
                (column, cell) => column == 0 ? style.Subject(cell) : cell)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (data.PrependBlankLine && (tableLines.Length > 0 || data.ContentBlocks.Count > 0))
        {
            spans.Add(new CliTextSpan("\n"));
        }

        for (var index = 0; index < tableLines.Length; index++)
        {
            spans.Add(new CliTextSpan(tableLines[index] + "\n"));
            if (!full)
            {
                continue;
            }

            foreach (var evidence in data.Matches[index].Evidence ?? [])
            {
                var kind = evidence.Kind;
                spans.Add(new CliTextSpan(CliText.Escape(
                    global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatMatchedIn($"{kind}", $"{evidence.Value}", $"{evidence.Region}")) + "\n"));
            }
        }

        if (full && data.Matches.Count > 0)
        {
            spans.Add(new CliTextSpan("\n"));
            spans.Add(new CliTextSpan(CliText.Escape(global::OpenForge.Cli.OutputText.Find.FindText.HeadingSearchDetails()) + "\n"));
            if (data.Query is { } query)
            {
                AppendQuery(spans, query);
            }

            if (data.SourceSet is { } sourceSet)
            {
                spans.Add(new CliTextSpan(CliText.Escape(
                    global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatSourceSet($"{sourceSet.Mode}")) + "\n"));
                if (sourceSet.Inspected is { } inspected && sourceSet.Candidates is { } candidates)
                {
                    spans.Add(new CliTextSpan(CliText.Escape(
                        global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatOfSourcesInspected(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{inspected}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{candidates}"))) + "\n"));
                }
            }
        }

        if (data.ContentBlocks.Count > 0)
        {
            if (spans.Count > 0 && !spans[^1].Content.EndsWith('\n'))
            {
                spans.Add(new CliTextSpan("\n"));
            }

            if (spans.Count > 0 && spans[^1].Content.Length > 1)
            {
                spans.Add(new CliTextSpan("\n"));
            }

            spans.AddRange(ContentPartsTextRenderer.Render(data.ContentBlocks, style).Spans);
        }

        return new CliTextDocument(spans);
    }

    private static void AppendQuery(List<CliTextSpan> spans, FindDataQuery query)
    {
        var filters = query.Tags
            .Select(tag => $"--tag {tag}")
            .Concat(query.Headings.Select(heading => $"--heading {heading}"))
            .ToArray();
        spans.Add(new CliTextSpan(CliText.Escape(
            global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatFilters($"{(filters.Length == 0 ? "none" : string.Join(", ", filters))}")) + "\n"));
        spans.Add(new CliTextSpan(CliText.Escape(global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatRequire($"{query.Require}")) + "\n"));
        spans.Add(new CliTextSpan(CliText.Escape(
            global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatRegionsSearched($"{(query.Within.Count == 0 ? "none" : string.Join(", ", query.Within))}")) + "\n"));
    }
}
