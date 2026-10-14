using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Prompts.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts;

internal sealed partial class CliPrompts
{
    private static CliSelectionFrame MarkedFrame<T>(CliMarkedListQuestion<T> question, IReadOnlyList<int> marks, int cursor) where T : notnull
    {
        var focused = question.Rows[cursor];
        var detail = question.Details(focused.Value, marks[cursor]);
        return new()
        {
            Question = question.Question,
            Legend = MarkedLegend(question.Marks),
            Rows = question.Rows.Select((row, index) => new CliSelectionRow(row.Label, $"[{question.Marks[marks[index]].Symbol}]",
                Disabled: row.Lock is not null, Summary: row.Summary)).ToArray(),
            NumberRows = false,
            Focus = cursor,
            Position = CliPromptWording.Position(cursor, question.Rows.Count),
            Details = detail is not null && detail != focused.Summary ? [detail] : [],
            Notice = focused.Lock,
            Controls = [CliPromptWording.MarkedControls(question.Marks)],
        };
    }

    private string MarkedLineFrame<T>(CliMarkedListQuestion<T> question, IReadOnlyList<int> marks) where T : notnull
    {
        var output = new StringBuilder();
        AppendLineProse(output, question.Question);
        AppendLineItems(output, MarkedLegend(question.Marks));
        foreach (var explanation in question.LineExplanations) AppendLineProse(output, explanation);
        output.AppendLine();
        var labelWidth = question.Rows.Max(row => CliSelectionText.Width(row.Label));
        var numberWidth = question.Rows.Count.ToString(CultureInfo.InvariantCulture).Length;
        for (var index = 0; index < question.Rows.Count; index++)
        {
            var row = question.Rows[index];
            var label = CliText.Escape(row.Label);
            AppendLineChoice(output, $"  {(index + 1).ToString(CultureInfo.InvariantCulture).PadLeft(numberWidth)}. [{question.Marks[marks[index]].Symbol}] {label}",
                row.Summary, labelWidth - CliSelectionText.Width(row.Label) + 2);
            if (row.Description is { } description && description != row.Summary) AppendLineProse(output, description, "    ");
            if (row.Lock is { } reason) AppendLineProse(output, reason, "    ");
        }
        AppendLineProse(output, CliPromptWording.MarkedLine(question.Marks, question.Rows.Count));
        AppendLineProse(output, global::OpenForge.Cli.OutputText.Shared.CliPromptWording.MarkedLineCancel());
        output.AppendLine(">");
        return output.ToString();
    }

    private static string MarkedLegend(IReadOnlyList<CliMark> marks)
        => string.Join("   ", marks.Select(mark => $"{CliText.Escape(mark.Symbol)} {CliText.Escape(mark.Legend)}"));
}
