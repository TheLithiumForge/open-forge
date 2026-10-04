using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Prompts.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts;

internal static class CliSelectionFrameRenderer
{
    internal const int WidthReserve = 1;
    private const int BottomRowReserve = 1;
    private const int ExpandedHeight = 18;
    private const int CompactHeadingRows = 1;
    private const int ExpandedHeadingRows = 2;
    private const int CompactDetailRows = 2;
    private const int ExpandedDetailRows = 4;
    private const int MaximumNoticeRows = 2;
    private const int PreferredChoiceRows = 3;
    private const int MinimumChoiceRows = 1;
    private const int PositionRows = 1;
    private const int ContextDetailReserveRows = 1;

    internal static int ContextCapacity(CliTerminalViewport viewport, string question, int controlRows)
        => viewport.Height - BottomRowReserve - Heading(question, viewport).Count - PositionRows
            - controlRows - PreferredChoiceRows - ContextDetailReserveRows;

    internal static string? Render(CliSelectionFrame frame, CliTerminalViewport viewport, CliTextStyle style)
    {
        var width = viewport.Width - WidthReserve;
        var capacity = viewport.Height - BottomRowReserve;
        var heading = Heading(frame.Question, viewport);
        var controls = frame.Controls.Select(text => CliSelectionText.Clip(text, width)).ToArray();
        var context = frame.Context.Select(text => CliSelectionText.Clip(text, width)).ToArray();
        var details = frame.Details.SelectMany(text => CliSelectionText.Wrap(text, width))
            .Take(viewport.Height >= ExpandedHeight ? ExpandedDetailRows : CompactDetailRows).ToArray();
        var notices = frame.Notice is { } notice ? CliSelectionText.Wrap(notice, width).Take(MaximumNoticeRows).ToArray() : [];
        var reserved = heading.Count + controls.Length + context.Length + notices.Length + PositionRows;
        if (reserved + MinimumChoiceRows > capacity) return null;
        var detailRows = Math.Min(details.Length, Math.Max(0, capacity - reserved - Math.Min(frame.Rows.Count, PreferredChoiceRows)));
        var choiceRows = Math.Min(frame.Rows.Count, capacity - reserved - detailRows);
        var first = Math.Clamp(frame.Focus - choiceRows / 2, 0, frame.Rows.Count - choiceRows);
        var output = new StringBuilder();
        foreach (var line in heading) output.AppendLine(line);
        foreach (var line in context) output.AppendLine(line);
        output.AppendLine(CliSelectionText.Clip(frame.Position, width));
        for (var index = first; index < first + choiceRows; index++)
        {
            var row = frame.Rows[index];
            var focus = index == frame.Focus ? '>' : ' ';
            var text = string.Create(CultureInfo.InvariantCulture, $"{focus} {row.Mark} {index + 1}. {row.Label}");
            var clipped = CliSelectionText.Clip(text, width);
            output.AppendLine(row.Disabled ? style.Dim(clipped) : clipped);
        }
        foreach (var line in details.Take(detailRows)) output.AppendLine(line);
        foreach (var line in notices) output.AppendLine(line);
        foreach (var line in controls) output.AppendLine(line);
        return output.ToString();
    }

    private static IReadOnlyList<string> Heading(string question, CliTerminalViewport viewport)
        => CliSelectionText.Summarize(question, viewport.Width - WidthReserve,
            viewport.Height >= ExpandedHeight ? ExpandedHeadingRows : CompactHeadingRows);
}
