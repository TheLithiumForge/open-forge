using System.Globalization;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts;

internal sealed partial class CliPrompts
{
    internal ValueTask<CliPromptReply<CliMarkedSelection<T>>> MarkedListAsync<T>(CliMarkedListQuestion<T> question,
        CliPromptPolicy policy, CancellationToken cancellationToken) where T : notnull
        => RunAsync<CliMarkedListQuestion<T>, CliMarkedSelection<T>>(question, policy, cancellationToken,
            static (prompts, question, token) => prompts.MarkedListCoreAsync(question, token));

    private async ValueTask<CliPromptReply<CliMarkedSelection<T>>> MarkedListCoreAsync<T>(
        CliMarkedListQuestion<T> question, CancellationToken cancellationToken) where T : notnull
    {
        ValidateMarkedList(question);
        var marks = question.Rows.Select(row => row.Mark).ToArray();
        var cursor = 0;
        while (_terminal.ReadSelectionViewport() is { } viewport)
        {
            if (!await DrawSelectionAsync(MarkedFrame(question, marks, cursor), viewport, cancellationToken).ConfigureAwait(false)) break;
            var key = await _terminal.ReadKeyAsync(cancellationToken).ConfigureAwait(false);
            if (key is null || key.Value.Key == CliKey.Escape) return CliPromptReply<CliMarkedSelection<T>>.Cancelled();
            if (key.Value.Key == CliKey.Enter) return MarkedReply(question, marks);
            if (key.Value.Key == CliKey.Up) cursor = (cursor + marks.Length - 1) % marks.Length;
            else if (key.Value.Key == CliKey.Down) cursor = (cursor + 1) % marks.Length;
            else if (question.Rows[cursor].Lock is null)
            {
                if (key.Value.Key == CliKey.Space) marks[cursor] = (marks[cursor] + 1) % question.Marks.Count;
                else if (key.Value.Key == CliKey.Character && MarkIndex(question.Marks, key.Value.Character) is var mark && mark >= 0)
                    marks[cursor] = mark;
            }
        }

        while (true)
        {
            await WriteFrameAsync(MarkedLineFrame(question, marks), cancellationToken).ConfigureAwait(false);
            var answer = await _terminal.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (answer is null || string.Equals(answer.Trim(), "cancel", StringComparison.OrdinalIgnoreCase))
                return CliPromptReply<CliMarkedSelection<T>>.Cancelled();
            if (string.IsNullOrWhiteSpace(answer)) return MarkedReply(question, marks);
            var edits = (int[])marks.Clone();
            string? error = null;
            var tokens = answer.Split([' ', '\t', ','], StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) error = CliPromptWording.MarkedRule(question.Marks, marks.Length);
            foreach (var edit in tokens)
            {
                if (edit.Length < 2 || !int.TryParse(edit.AsSpan(0, edit.Length - 1), NumberStyles.None, CultureInfo.InvariantCulture, out var row)
                    || row < 1 || row > marks.Length || MarkIndex(question.Marks, edit[^1]) < 0)
                {
                    error = CliPromptWording.MarkedRule(question.Marks, marks.Length);
                    break;
                }
                if (question.Rows[row - 1].Lock is { } reason)
                {
                    error = reason;
                    break;
                }
                edits[row - 1] = MarkIndex(question.Marks, edit[^1]);
            }
            if (error is null) marks = edits;
            else await WriteFrameAsync(CliText.Escape(error) + "\n", cancellationToken).ConfigureAwait(false);
        }
    }

    private static int MarkIndex(IReadOnlyList<CliMark> marks, char symbol)
    {
        for (var index = 0; index < marks.Count; index++)
            if (marks[index].Symbol[0] == symbol) return index;
        return -1;
    }

    private static CliPromptReply<CliMarkedSelection<T>> MarkedReply<T>(CliMarkedListQuestion<T> question, IReadOnlyList<int> marks) where T : notnull
        => CliPromptReply<CliMarkedSelection<T>>.Answered(new(question.Rows.Select((row, index) => new CliMarkedValue<T>(row.Value, marks[index])).ToArray()));

    private static void ValidateMarkedList<T>(CliMarkedListQuestion<T> question) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(question.Marks);
        ArgumentNullException.ThrowIfNull(question.Rows);
        ArgumentNullException.ThrowIfNull(question.Details);
        if (question.Marks.Count == 0 || question.Marks.Any(mark => mark.Symbol.Length != 1 || char.IsWhiteSpace(mark.Symbol[0])
            || char.IsDigit(mark.Symbol[0]) || mark.Symbol[0] == ',') || question.Marks.Select(mark => mark.Symbol).Distinct().Count() != question.Marks.Count)
            throw new ArgumentException("Marks require distinct single-key symbols.", nameof(question));
        if (question.Rows.Count == 0 || question.Rows.Select(row => row.Value).Distinct().Count() != question.Rows.Count
            || question.Rows.Any(row => row.Mark < 0 || row.Mark >= question.Marks.Count))
            throw new ArgumentException("Rows require distinct values and a declared mark.", nameof(question));
    }
}
