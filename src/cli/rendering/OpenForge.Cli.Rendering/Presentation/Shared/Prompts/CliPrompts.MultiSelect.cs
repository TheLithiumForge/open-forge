using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts;

internal sealed partial class CliPrompts
{
    internal ValueTask<CliPromptReply<CliMultiSelection<T>>> MultiSelectAsync<T>(CliMultiSelectQuestion<T> question,
        CliPromptPolicy policy, CancellationToken cancellationToken) where T : notnull
        => RunAsync<CliMultiSelectQuestion<T>, CliMultiSelection<T>>(question, policy, cancellationToken,
            static (prompts, question, token) => prompts.MultiSelectCoreAsync(question, token));

    private async ValueTask<CliPromptReply<CliMultiSelection<T>>> MultiSelectCoreAsync<T>(
        CliMultiSelectQuestion<T> question,
        CancellationToken cancellationToken)
        where T : notnull
    {
        ValidateChoices(question.Choices);
        var byId = question.Choices.ToDictionary(choice => choice.Value);
        if (!Enum.IsDefined(question.Direction) || question.Disabled.Any(value => !byId.ContainsKey(value))
            || question.Dependencies.Any(edge => !byId.ContainsKey(edge.Value) || !byId.ContainsKey(edge.Dependency)))
            throw new ArgumentException("Dependency rows must reference displayed choices.", nameof(question));
        var chosen = new HashSet<T>();
        var cursor = 0;
        var previousLines = 0;
        string? notice = null;
        while (true)
        {
            var required = RequiredBy(question, chosen);
            var frame = new StringBuilder().Append(CliText.Escape(question.Question)).Append("\n\n");
            for (var index = 0; index < question.Choices.Count; index++)
            {
                var row = question.Choices[index];
                var disabled = question.Disabled.Contains(row.Value);
                string mark;
                if (chosen.Contains(row.Value))
                    mark = "[x]";
                else if (required.ContainsKey(row.Value) && !disabled)
                    mark = "[+]";
                else
                    mark = "[ ]";

                string prefix;
                if (_terminal.CanReadKeys)
                {
                    if (index == cursor)
                        prefix = "> ";
                    else
                        prefix = "  ";
                }
                else
                    prefix = string.Create(CultureInfo.InvariantCulture, $"{index + 1}. ");
                var text = $"  {prefix}{mark} {CliText.Escape(row.Label)}";
                if (row.Description is { } description) text += "   " + CliText.Escape(description);
                var related = Related(question, row.Value).Select(value => byId[value].Label).ToArray();
                if (disabled) text += "   " + CliPromptWording.Installed();
                else if (required.TryGetValue(row.Value, out var owners) && !chosen.Contains(row.Value))
                    text += "   " + CliText.Escape(CliPromptWording.RequiredBy(Labels(question, owners)));
                else if (related.Length > 0)
                    text += "   " + CliText.Escape(question.Direction == CliDependencyDirection.Requires
                        ? CliPromptWording.Needs(string.Join(", ", related)) : CliPromptWording.NeededBy(string.Join(", ", related)));
                frame.Append(disabled ? _style.Dim(text) : text).Append('\n');
            }
            frame.Append('\n').Append(CliPromptWording.Legend()).Append('\n')
                .Append(_terminal.CanReadKeys ? CliPromptWording.MultiKeys() : CliPromptWording.MultiLine()).Append('\n');
            if (notice is not null) frame.Append(CliText.Escape(notice)).Append('\n');
            previousLines = await DrawAsync(frame.ToString(), previousLines, cancellationToken).ConfigureAwait(false);
            notice = null;
            if (!_terminal.CanReadKeys)
            {
                var answer = await _terminal.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(answer)) return CliPromptReply<CliMultiSelection<T>>.Cancelled();
                var values = new HashSet<T>();
                if (string.Equals(answer.Trim(), "all", StringComparison.OrdinalIgnoreCase))
                    values.UnionWith(question.Choices.Where(choice => !question.Disabled.Contains(choice.Value)).Select(choice => choice.Value));
                else
                {
                    var valid = true;
                    foreach (var token in answer.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var ordinal)
                            || ordinal < 1 || ordinal > question.Choices.Count
                            || question.Disabled.Contains(question.Choices[ordinal - 1].Value))
                        { valid = false; break; }
                        values.Add(question.Choices[ordinal - 1].Value);
                    }
                    if (!valid) continue;
                }
                if (values.Count == 0) { notice = CliPromptWording.EmptySelection(); continue; }
                var closure = RequiredBy(question, values);
                foreach (var row in question.Choices.Where(choice => closure.ContainsKey(choice.Value) && !values.Contains(choice.Value)))
                    await WriteFrameAsync(CliText.Escape(question.Direction == CliDependencyDirection.Requires
                        ? CliPromptWording.AlsoInstalling(row.Label, Labels(question, closure[row.Value]))
                        : CliPromptWording.AlsoRemoving(row.Label, Labels(question, closure[row.Value]))) + "\n", cancellationToken).ConfigureAwait(false);
                return Reply(question, values, closure);
            }

            var key = await _terminal.ReadKeyAsync(cancellationToken).ConfigureAwait(false);
            if (key is null || key.Value.Key == CliKey.Escape) return CliPromptReply<CliMultiSelection<T>>.Cancelled();
            if (key.Value.Key == CliKey.Up) cursor = (cursor + question.Choices.Count - 1) % question.Choices.Count;
            else if (key.Value.Key == CliKey.Down) cursor = (cursor + 1) % question.Choices.Count;
            else if (key.Value.Key == CliKey.Enter)
            {
                if (chosen.Count > 0) return Reply(question, chosen, required);
                notice = CliPromptWording.EmptySelection();
            }
            else if (key.Value.Key == CliKey.Character && char.ToLowerInvariant(key.Value.Character) == 'a')
                chosen.UnionWith(question.Choices.Where(choice => !question.Disabled.Contains(choice.Value)).Select(choice => choice.Value));
            else if (key.Value.Key == CliKey.Character && char.ToLowerInvariant(key.Value.Character) == 'n') chosen.Clear();
            else if (key.Value.Key == CliKey.Space)
            {
                var row = question.Choices[cursor];
                if (question.Disabled.Contains(row.Value)) continue;
                if (chosen.Remove(row.Value)) continue;
                if (required.TryGetValue(row.Value, out var owners))
                {
                    notice = question.Direction == CliDependencyDirection.Requires
                        ? CliPromptWording.Required(row.Label, Labels(question, owners))
                        : CliPromptWording.RemoveBoth(row.Label, Labels(question, owners));
                }
                else
                {
                    chosen.Add(row.Value);
                    if (question.Direction == CliDependencyDirection.Dependents)
                    {
                        var added = RequiredBy(question, chosen).Keys.Where(value => !chosen.Contains(value)).ToHashSet();
                        if (added.Count > 0) notice = CliPromptWording.RemoveBoth(row.Label, Labels(question, added));
                    }
                }
            }
        }
    }

    private static IEnumerable<T> Related<T>(CliMultiSelectQuestion<T> question, T value) where T : notnull
        => question.Dependencies.Where(edge => EqualityComparer<T>.Default.Equals(
            question.Direction == CliDependencyDirection.Requires ? edge.Value : edge.Dependency, value))
            .Select(edge => question.Direction == CliDependencyDirection.Requires ? edge.Dependency : edge.Value).Distinct();

    private static Dictionary<T, HashSet<T>> RequiredBy<T>(CliMultiSelectQuestion<T> question, HashSet<T> chosen) where T : notnull
    {
        var result = new Dictionary<T, HashSet<T>>();
        foreach (var root in chosen)
        {
            var visited = new HashSet<T> { root };
            var pending = new Stack<T>(Related(question, root));
            while (pending.TryPop(out var value))
            {
                if (!visited.Add(value) || question.Disabled.Contains(value)) continue;
                if (!result.TryGetValue(value, out var owners)) result.Add(value, owners = []);
                owners.Add(root);
                foreach (var dependency in Related(question, value)) pending.Push(dependency);
            }
        }
        return result;
    }

    private static string Labels<T>(CliMultiSelectQuestion<T> question, IReadOnlySet<T> values) where T : notnull
        => string.Join(", ", question.Choices.Where(choice => values.Contains(choice.Value)).Select(choice => choice.Label));

    private static CliPromptReply<CliMultiSelection<T>> Reply<T>(CliMultiSelectQuestion<T> question, HashSet<T> chosen,
        Dictionary<T, HashSet<T>> required) where T : notnull
        => CliPromptReply<CliMultiSelection<T>>.Answered(new(
            question.Choices.Where(choice => chosen.Contains(choice.Value)).Select(choice => choice.Value).ToArray(),
            question.Choices.Where(choice => required.ContainsKey(choice.Value) && !chosen.Contains(choice.Value)).Select(choice => choice.Value).ToArray()));
}
