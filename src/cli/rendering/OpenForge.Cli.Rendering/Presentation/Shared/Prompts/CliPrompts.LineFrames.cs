using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts;

internal sealed partial class CliPrompts
{
    private int LineWidth => Math.Max(1, _terminal.Width - CliSelectionFrameRenderer.WidthReserve);

    private ValueTask WriteProseAsync(string text, CancellationToken cancellationToken)
        => WriteFrameAsync(string.Join("\n", CliSelectionText.Wrap(text, LineWidth)) + "\n", cancellationToken);

    private void AppendLineProse(StringBuilder output, string text, string indent = "")
    {
        foreach (var line in CliSelectionText.Wrap(text, LineWidth - indent.Length)) output.Append(indent).AppendLine(line);
    }

    private void AppendLineItems(StringBuilder output, string text)
    {
        foreach (var line in CliSelectionText.WrapItems(text, LineWidth)) output.AppendLine(line);
    }

    private void AppendLineChoice(StringBuilder output, string label, string? summary, int padding)
    {
        if (summary is null)
        {
            output.AppendLine(label);
            return;
        }
        var offset = CliSelectionText.Width(label) + padding;
        if (offset + CliSelectionText.Width(summary) <= LineWidth)
        {
            output.Append(label).Append(' ', padding).AppendLine(CliText.Escape(summary));
            return;
        }
        output.AppendLine(label);
        AppendLineProse(output, summary, "    ");
    }
}
