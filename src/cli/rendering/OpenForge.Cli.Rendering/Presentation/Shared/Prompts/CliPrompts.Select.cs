using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Prompts.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts;

internal sealed partial class CliPrompts
{
    internal ValueTask<CliPromptReply<T>> SelectAsync<T>(CliSelectQuestion<T> question, CliPromptPolicy policy, CancellationToken cancellationToken)
        where T : notnull
        => RunAsync<CliSelectQuestion<T>, T>(question, policy, cancellationToken,
            static (prompts, question, token) => prompts.SelectCoreAsync(question, token));

    private async ValueTask<CliPromptReply<T>> SelectCoreAsync<T>(CliSelectQuestion<T> question, CancellationToken cancellationToken)
        where T : notnull
    {
        ValidateChoices(question.Choices);
        var cursor = 0;
        while (_terminal.ReadSelectionViewport() is { } viewport)
        {
            var focused = question.Choices[cursor];
            var frame = new CliSelectionFrame
            {
                Question = question.Question,
                Rows = question.Choices.Select(row => new CliSelectionRow(row.Label)).ToArray(),
                Focus = cursor,
                Position = CliPromptWording.Position(cursor, question.Choices.Count),
                Details = focused.Description is { } description ? [description] : [],
                Controls = [CliPromptWording.MoveControls(), CliPromptWording.AcceptControls()],
            };
            if (!await DrawSelectionAsync(frame, viewport, cancellationToken).ConfigureAwait(false)) break;
            var key = await _terminal.ReadKeyAsync(cancellationToken).ConfigureAwait(false);
            if (key is null || key.Value.Key == CliKey.Escape) return CliPromptReply<T>.Cancelled();
            if (key.Value.Key == CliKey.Up) cursor = (cursor + question.Choices.Count - 1) % question.Choices.Count;
            else if (key.Value.Key == CliKey.Down) cursor = (cursor + 1) % question.Choices.Count;
            else if (key.Value.Key == CliKey.Enter) return CliPromptReply<T>.Answered(focused.Value);
            else if (key.Value.Key == CliKey.Character && key.Value.Character is >= '1' and <= '9'
                && key.Value.Character - '1' < question.Choices.Count)
                return CliPromptReply<T>.Answered(question.Choices[key.Value.Character - '1'].Value);
        }
        return await ReadChoiceLinesAsync(question, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<CliPromptReply<T>> ReadChoiceLinesAsync<T>(CliSelectQuestion<T> question, CancellationToken cancellationToken)
        where T : notnull
    {
        while (true)
        {
            var frame = new StringBuilder().Append(CliText.Escape(question.Question)).Append("\n\n");
            for (var index = 0; index < question.Choices.Count; index++)
            {
                var row = question.Choices[index];
                frame.Append(CultureInfo.InvariantCulture, $"  {index + 1}. {CliText.Escape(row.Label)}");
                if (row.Description is { } description) frame.Append("   ").Append(CliText.Escape(description));
                frame.Append('\n');
            }
            frame.Append('\n').Append(CliPromptWording.SelectLine(question.Choices.Count)).Append('\n');
            await WriteFrameAsync(frame.ToString(), cancellationToken).ConfigureAwait(false);
            var answer = await _terminal.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(answer)) return CliPromptReply<T>.Cancelled();
            if (int.TryParse(answer.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                && number > 0 && number <= question.Choices.Count)
                return CliPromptReply<T>.Answered(question.Choices[number - 1].Value);
        }
    }
}
