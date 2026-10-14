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
            row.Label, SelectionMark(row.Value, question, chosen, required), question.Disabled.Contains(row.Value),
            question.Disabled.Contains(row.Value) ? CliPromptWording.InstalledSummary() : row.Summary ?? ChoiceDescription(row))).ToArray();
        var focused = question.Choices[cursor];
        var context = DependencyContext(question, focused.Value, chosen, required);
        var details = new List<string>();
        if (ChoiceDescription(focused) is { } description) details.Add(description);
        if (context is not null) details.Add(context);
        return new CliSelectionFrame
        {
            Question = question.Question,
            Legend = CliPromptWording.Legend(question.Action, question.Dependencies.Count > 0),
            NumberRows = false,
            Rows = rows,
            Focus = cursor,
            Position = CliPromptWording.MultiPosition(cursor, rows.Length, chosen.Count, required.Keys.Count(value => !chosen.Contains(value))),
            Details = details,
            KeepLastDetail = context is not null,
            Notice = notice,
            Controls = [CliPromptWording.MultiControls()],
        };
    }

    private string MultiLineFrame<T>(CliMultiSelectQuestion<T> question, HashSet<T> chosen,
        Dictionary<T, HashSet<T>> required, string? notice) where T : notnull
    {
        var frame = new StringBuilder();
        AppendLineProse(frame, question.Question);
        AppendLineItems(frame, CliPromptWording.Legend(question.Action, question.Dependencies.Count > 0));
        frame.Append('\n');
        var labelWidth = question.Choices.Max(row => CliSelectionText.Width(row.Label));
        var numberWidth = question.Choices.Count.ToString(CultureInfo.InvariantCulture).Length;
        for (var index = 0; index < question.Choices.Count; index++)
        {
            var row = question.Choices[index];
            var mark = SelectionMark(row.Value, question, chosen, required);
            var text = $"  {(index + 1).ToString(CultureInfo.InvariantCulture).PadLeft(numberWidth)}. {mark} {CliText.Escape(row.Label)}";
            var summary = question.Disabled.Contains(row.Value) ? CliPromptWording.InstalledSummary() : row.Summary ?? ChoiceDescription(row);
            AppendLineChoice(frame, text, summary, labelWidth - CliSelectionText.Width(row.Label) + 2);
            if (ChoiceDescription(row) is { } description && description != summary) AppendLineProse(frame, description, "    ");
            if (DependencyContext(question, row.Value, chosen, required) is { } context) AppendLineProse(frame, context, "    ");
        }
        frame.Append('\n');
        AppendLineProse(frame, CliPromptWording.MultiLine());
        if (notice is not null) AppendLineProse(frame, notice);
        return frame.ToString();
    }

    private static string SelectionMark<T>(T value, CliMultiSelectQuestion<T> question, HashSet<T> chosen,
        Dictionary<T, HashSet<T>> required) where T : notnull
    {
        if (chosen.Contains(value)) return question.Action == CliSelectionAction.Remove ? "[-]" : "[+]";
        if (required.ContainsKey(value) && !question.Disabled.Contains(value)) return "[*]";
        return "[ ]";
    }

    private static string? ChoiceDescription<T>(CliChoice<T> choice) where T : notnull
        => choice.Description ?? (choice.InstalledPackage is { } package
            ? global::OpenForge.Cli.OutputText.Shared.CliPromptWording.InstalledPackageSummary(package.Version, package.OwnedFiles)
            : null);

    private static string? DependencyContext<T>(CliMultiSelectQuestion<T> question, T value, HashSet<T> chosen,
        Dictionary<T, HashSet<T>> required) where T : notnull
    {
        if (question.Disabled.Contains(value)) return null;
        if (required.TryGetValue(value, out var owners) && !chosen.Contains(value))
            return question.Direction == CliDependencyDirection.Requires
                ? CliPromptWording.RequiredBy(Labels(question, owners))
                : CliPromptWording.Needs(Labels(question, owners));
        var related = Related(question, value).ToHashSet();
        if (related.Count == 0) return null;
        var labels = Labels(question, related);
        return question.Direction == CliDependencyDirection.Requires ? CliPromptWording.Needs(labels) : CliPromptWording.NeededBy(labels);
    }
}
