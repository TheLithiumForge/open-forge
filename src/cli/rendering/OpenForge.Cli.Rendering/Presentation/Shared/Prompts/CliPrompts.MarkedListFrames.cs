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

    private static string MarkedLineFrame<T>(CliMarkedListQuestion<T> question, IReadOnlyList<int> marks) where T : notnull
    {
        var output = new StringBuilder().AppendLine(CliText.Escape(question.Question)).AppendLine(MarkedLegend(question.Marks)).AppendLine();
        var labelWidth = question.Rows.Max(row => CliSelectionText.Width(row.Label));
        var numberWidth = question.Rows.Count.ToString(CultureInfo.InvariantCulture).Length;
        for (var index = 0; index < question.Rows.Count; index++)
        {
            var row = question.Rows[index];
            var label = CliText.Escape(row.Label);
            var summary = row.Summary is { } text
                ? $"{new string(' ', labelWidth - CliSelectionText.Width(row.Label) + 2)}{CliText.Escape(text)}" : string.Empty;
            output.AppendLine($"  {(index + 1).ToString(CultureInfo.InvariantCulture).PadLeft(numberWidth)}. [{question.Marks[marks[index]].Symbol}] {label}{summary}");
            if (row.Lock is { } reason) output.AppendLine(CliText.Escape(reason));
        }
        output.AppendLine(CliPromptWording.MarkedLine(question.Marks, question.Rows.Count)).AppendLine(">");
        return output.ToString();
    }

    private static string MarkedLegend(IReadOnlyList<CliMark> marks)
        => string.Join("   ", marks.Select(mark => $"{CliText.Escape(mark.Symbol)} {CliText.Escape(mark.Legend)}"));
}
