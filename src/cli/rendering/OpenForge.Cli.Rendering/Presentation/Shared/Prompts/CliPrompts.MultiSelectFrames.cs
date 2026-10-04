using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Prompts.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts;

internal sealed partial class CliPrompts
{
    private static CliSelectionFrame MultiFrame<T>(CliMultiSelectQuestion<T> question, HashSet<T> chosen,
        Dictionary<T, HashSet<T>> required, int cursor, string? notice) where T : notnull
    {
        var rows = question.Choices.Select(row => new CliSelectionRow(
            row.Label, SelectionMark(row.Value, question, chosen, required), question.Disabled.Contains(row.Value))).ToArray();
        var focused = question.Choices[cursor];
        var context = DependencyContext(question, focused.Value, chosen, required);
        var details = new List<string>();
        if (context is not null) details.Add(context);
        if (focused.Description is { } description) details.Add(description);
        return new CliSelectionFrame
        {
            Question = question.Question,
            Rows = rows,
            Focus = cursor,
            Position = CliPromptWording.MultiPosition(cursor, rows.Length, chosen.Count, required.Keys.Count(value => !chosen.Contains(value))),
            Details = details,
            Notice = notice,
            Controls = [CliPromptWording.CompactLegend(), CliPromptWording.ToggleControls(), CliPromptWording.MultiMoveControls()],
        };
    }

    private string MultiLineFrame<T>(CliMultiSelectQuestion<T> question, HashSet<T> chosen,
        Dictionary<T, HashSet<T>> required, string? notice) where T : notnull
    {
        var frame = new StringBuilder().Append(CliText.Escape(question.Question)).Append("\n\n");
        for (var index = 0; index < question.Choices.Count; index++)
        {
            var row = question.Choices[index];
            var mark = SelectionMark(row.Value, question, chosen, required);
            var text = string.Create(CultureInfo.InvariantCulture, $"  {index + 1}. {mark} {CliText.Escape(row.Label)}");
            if (row.Description is { } description) text += "   " + CliText.Escape(description);
            if (DependencyContext(question, row.Value, chosen, required) is { } context) text += "   " + CliText.Escape(context);
            frame.Append(question.Disabled.Contains(row.Value) ? _style.Dim(text) : text).Append('\n');
        }
        frame.Append('\n').Append(CliPromptWording.Legend()).Append('\n').Append(CliPromptWording.MultiLine()).Append('\n');
        if (notice is not null) frame.Append(CliText.Escape(notice)).Append('\n');
        return frame.ToString();
    }

    private static string SelectionMark<T>(T value, CliMultiSelectQuestion<T> question, HashSet<T> chosen,
        Dictionary<T, HashSet<T>> required) where T : notnull
    {
        if (chosen.Contains(value)) return "[x]";
        if (required.ContainsKey(value) && !question.Disabled.Contains(value)) return "[+]";
        return "[ ]";
    }

    private static string? DependencyContext<T>(CliMultiSelectQuestion<T> question, T value, HashSet<T> chosen,
        Dictionary<T, HashSet<T>> required) where T : notnull
    {
        if (question.Disabled.Contains(value)) return CliPromptWording.Installed();
        if (required.TryGetValue(value, out var owners) && !chosen.Contains(value)) return CliPromptWording.RequiredBy(Labels(question, owners));
        var related = Related(question, value).ToHashSet();
        if (related.Count == 0) return null;
        var labels = Labels(question, related);
        return question.Direction == CliDependencyDirection.Requires ? CliPromptWording.Needs(labels) : CliPromptWording.NeededBy(labels);
    }
}
