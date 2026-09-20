using System.Globalization;
using OpenForge.Cli.Core.Presentation.Context.Models;
using OpenForge.Cli.Core.Presentation.Context.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Content;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Context.Shared.Rendering;

internal static class ContextDataTextRenderer
{
    internal static CliTextDocument Render(ContextData data, CliSelection _, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(style);
        var spans = new List<CliTextSpan>();
        if (data.Summary is { } summary)
        {
            spans.Add(new CliTextSpan(summary + "\n"));
        }

        spans.AddRange(ContentPartsTextRenderer.Render(
            data.ContentBlocks,
            style,
            data.PathsOnly).Spans);
        if (data.Links is { Count: > 0 } links)
        {
            AppendLinks(spans, links, style);
        }

        return new CliTextDocument(spans);
    }

    private static void AppendLinks(
        List<CliTextSpan> spans,
        IReadOnlyList<ContextDataLink> links,
        CliTextStyle style)
    {
        var hasContent = spans.Any(span => span.Content.Length > 0);
        if (hasContent && spans[^1].Content[^1] is not ('\r' or '\n'))
        {
            spans.Add(new CliTextSpan("\n"));
        }

        if (hasContent)
        {
            spans.Add(new CliTextSpan("\n"));
        }

        spans.Add(new CliTextSpan(CliText.Escape(ContextWording.LinksHeading()) + "\n"));
        foreach (var link in links)
        {
            var location = string.Create(
                CultureInfo.InvariantCulture,
                $"{link.From}:{link.Location.Line}:{link.Location.Column}");
            var row = ContextWording.LinkRow(
                location,
                link.Destination,
                link.Resolution,
                link.Followed);
            spans.Add(new CliTextSpan(
                $"  {style.Subject(CliText.Escape(row.Location))}"
                + $"{CliText.Escape(ContextWording.LinkArrow())}"
                + $"{CliText.Escape(row.Destination)}"
                + $"{CliText.Escape(ContextWording.LinkDetailSeparator())}"
                + $"{CliText.Escape(row.Resolution)}"
                + $"{CliText.Escape(ContextWording.LinkDetailSeparator())}"
                + $"{CliText.Escape(row.State)}\n"));
        }
    }
}
