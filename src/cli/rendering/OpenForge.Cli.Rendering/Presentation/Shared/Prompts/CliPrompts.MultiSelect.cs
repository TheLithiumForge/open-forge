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
        if (!Enum.IsDefined(question.Action)) throw new ArgumentOutOfRangeException(nameof(question));
        if (!Enum.IsDefined(question.Direction) || question.Disabled.Any(value => !byId.ContainsKey(value))
            || question.Dependencies.Any(edge => !byId.ContainsKey(edge.Value) || !byId.ContainsKey(edge.Dependency)))
            throw new ArgumentException("Dependency rows must reference displayed choices.", nameof(question));
        var chosen = new HashSet<T>();
        var cursor = 0;
        string? notice = null;
        while (true)
        {
            var required = RequiredBy(question, chosen);
            var viewport = _terminal.ReadSelectionViewport();
            var keyMode = viewport is { } size && await DrawSelectionAsync(
                MultiFrame(question, chosen, required, cursor, notice), size, cancellationToken).ConfigureAwait(false);
            if (!keyMode)
            {
                await WriteFrameAsync(MultiLineFrame(question, chosen, required, notice), cancellationToken).ConfigureAwait(false);
                notice = null;
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
                        ? CliPromptWording.AlsoIncluded(row.Label, Labels(question, closure[row.Value]))
                        : CliPromptWording.Removing(Labels(question, closure[row.Value]), row.Label)) + "\n", cancellationToken).ConfigureAwait(false);
                return Reply(question, values, closure);
            }

            var key = await _terminal.ReadKeyAsync(cancellationToken).ConfigureAwait(false);
            if (key is null || key.Value.Key == CliKey.Escape) return CliPromptReply<CliMultiSelection<T>>.Cancelled();
            if (key.Value.Key == CliKey.Resize) continue;
            notice = null;
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
                        : CliPromptWording.Removing(Labels(question, owners), row.Label);
                }
                else
                {
                    chosen.Add(row.Value);
                    if (question.Direction == CliDependencyDirection.Dependents)
                    {
                        var added = RequiredBy(question, chosen).Keys.Where(value => !chosen.Contains(value)).ToHashSet();
                        if (added.Count > 0) notice = CliPromptWording.Removing(row.Label, Labels(question, added));
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
