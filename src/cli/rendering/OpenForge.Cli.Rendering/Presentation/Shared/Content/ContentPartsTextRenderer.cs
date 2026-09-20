using OpenForge.Cli.Core.Presentation.Shared.Content.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Content;

internal static class ContentPartsTextRenderer
{
    internal static CliTextDocument Render(
        IReadOnlyList<CliContentBlock> blocks,
        CliTextStyle style,
        bool pathsOnly = false)
    {
        ArgumentNullException.ThrowIfNull(blocks);
        ArgumentNullException.ThrowIfNull(style);
        if (pathsOnly)
        {
            return RenderPaths(blocks);
        }

        var spans = new List<CliTextSpan>();
        var endsWithLineBreak = true;
        for (var blockIndex = 0; blockIndex < blocks.Count; blockIndex++)
        {
            var block = blocks[blockIndex] ?? throw new ArgumentException("Content blocks cannot contain null members.", nameof(blocks));
            if (blockIndex > 0)
            {
                AppendBlankLine(spans, ref endsWithLineBreak);
            }

            AppendGeneratedLine(
                spans,
                CliText.Escape(ContentPartsWording.Delimiter(block.Path, block.Id, block.DelimiterLayer)),
                ref endsWithLineBreak);
            endsWithLineBreak = true;
            foreach (var reason in block.IncludedBecause)
            {
                AppendGeneratedLine(
                    spans,
                    CliText.Escape(ContentPartsWording.IncludedBecause(reason)),
                    ref endsWithLineBreak);
            }

            if (block.Route is not null || block.Scope is not null || block.Order is not null || block.Layer is not null)
            {
                AppendDetail(spans, block, ref endsWithLineBreak);
            }

            foreach (var part in block.Parts)
            {
                ArgumentNullException.ThrowIfNull(part);
                AppendPart(spans, part, style, ref endsWithLineBreak);
            }
        }

        return new CliTextDocument(spans);
    }

    private static CliTextDocument RenderPaths(IReadOnlyList<CliContentBlock> blocks)
    {
        var spans = new List<CliTextSpan>();
        foreach (var block in blocks)
        {
            foreach (var part in block.Parts)
            {
                foreach (var path in part.Paths)
                {
                    spans.Add(new CliTextSpan(CliText.Escape(path) + "\n"));
                }
            }
        }

        return new CliTextDocument(spans);
    }

    private static void AppendDetail(
        List<CliTextSpan> spans,
        CliContentBlock block,
        ref bool endsWithLineBreak)
    {
        if (block.Route is { } route)
        {
            AppendGeneratedLine(spans, CliText.Escape(ContentPartsWording.Route(route)), ref endsWithLineBreak);
        }

        if (block.Scope is { } scope)
        {
            AppendGeneratedLine(spans, CliText.Escape(ContentPartsWording.Scope(scope)), ref endsWithLineBreak);
        }

        if (block.Order is { } order)
        {
            AppendGeneratedLine(spans, CliText.Escape(ContentPartsWording.Order(order)), ref endsWithLineBreak);
        }

        if (block.Layer is { } layer)
        {
            AppendGeneratedLine(spans, CliText.Escape(ContentPartsWording.Layer(layer)), ref endsWithLineBreak);
        }
    }

    private static void AppendPart(
        List<CliTextSpan> spans,
        CliContentPart part,
        CliTextStyle style,
        ref bool endsWithLineBreak)
    {
        switch (part.Kind)
        {
            case CliContentPartKind.Text:
                if (part.AuthoredText is { } authored)
                {
                    if (!endsWithLineBreak && !StartsWithLineBreak(authored.Content))
                    {
                        spans.Add(new CliTextSpan("\n"));
                    }

                    spans.Add(CliTextSpan.FromAuthored(authored));
                    endsWithLineBreak = EndsWithLineBreak(authored.Content);
                }

                break;
            case CliContentPartKind.Headings:
                foreach (var heading in part.Headings)
                {
                    AppendGeneratedLine(
                        spans,
                        style.Subject(CliText.Escape(ContentPartsWording.Heading(heading.Text, heading.Level, heading.Line))),
                        ref endsWithLineBreak);
                }

                break;
            case CliContentPartKind.Paths:
                foreach (var path in part.Paths)
                {
                    AppendGeneratedLine(spans, CliText.Escape(path), ref endsWithLineBreak);
                }

                break;
            case CliContentPartKind.Metadata:
                foreach (var row in part.Metadata)
                {
                    AppendGeneratedLine(
                        spans,
                        CliText.Escape(ContentPartsWording.Metadata(row.Name, row.Value)),
                        ref endsWithLineBreak);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(part), part.Kind, "The content part kind is not defined.");
        }
    }

    private static void AppendGeneratedLine(
        List<CliTextSpan> spans,
        string line,
        ref bool endsWithLineBreak)
    {
        if (!endsWithLineBreak)
        {
            spans.Add(new CliTextSpan("\n"));
        }

        spans.Add(new CliTextSpan(line + "\n"));
        endsWithLineBreak = true;
    }

    private static void AppendBlankLine(List<CliTextSpan> spans, ref bool endsWithLineBreak)
    {
        spans.Add(new CliTextSpan(endsWithLineBreak ? "\n" : "\n\n"));
        endsWithLineBreak = true;
    }

    private static bool EndsWithLineBreak(string value)
        => value.Length > 0 && value[^1] is '\r' or '\n';

    private static bool StartsWithLineBreak(string value)
        => value.Length > 0 && value[0] is '\r' or '\n';

}
