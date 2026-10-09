using OpenForge.Cli.Core.Presentation.Shared.Prompts.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts;

internal sealed partial class CliPrompts(CliTerminal terminal, bool standardErrorColor = false)
{
    private delegate ValueTask<CliPromptReply<TAnswer>> CliPromptOperation<TQuestion, TAnswer>(
        CliPrompts prompts,
        TQuestion question,
        CancellationToken cancellationToken)
        where TQuestion : notnull
        where TAnswer : notnull;

    private readonly CliTerminal _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
    private readonly CliTextStyle _style = standardErrorColor ? CliTextStyle.Color : CliTextStyle.Plain;

    internal bool CanPrompt(CliPromptPolicy policy) => policy.Allowed && _terminal.CanPrompt;

    internal ValueTask<CliPromptReply<bool>> ConfirmAsync(CliConfirmQuestion question, CliPromptPolicy policy, CancellationToken cancellationToken)
        => RunAsync<CliConfirmQuestion, bool>(question, policy, cancellationToken,
            static (prompts, question, token) => prompts.ConfirmCoreAsync(question, token));

    private async ValueTask<CliPromptReply<bool>> ConfirmCoreAsync(CliConfirmQuestion question, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question.Sentence);
        while (true)
        {
            await WriteFrameAsync(CliText.Escape(question.Sentence) + "\n", cancellationToken).ConfigureAwait(false);
            if (_terminal.CanReadKeys)
            {
                CliKeyStroke? key;
                do
                {
                    key = await _terminal.ReadKeyAsync(cancellationToken).ConfigureAwait(false);
                }
                while (key is { Key: CliKey.Resize });
                if (key is null || key.Value.Key is CliKey.Escape or CliKey.Enter
                    || key.Value.Key == CliKey.Character && char.ToLowerInvariant(key.Value.Character) == 'n')
                    return CliPromptReply<bool>.Cancelled();
                if (key.Value.Key == CliKey.Character && char.ToLowerInvariant(key.Value.Character) == 'y')
                    return CliPromptReply<bool>.Answered(true);
            }
            else
            {
                var answer = (await _terminal.ReadLineAsync(cancellationToken).ConfigureAwait(false))?.Trim().ToLowerInvariant();
                if (answer is null or "" or "n" or "no") return CliPromptReply<bool>.Cancelled();
                if (answer is "y" or "yes") return CliPromptReply<bool>.Answered(true);
                await WriteFrameAsync(CliPromptWording.ConfirmationLineRule() + "\n", cancellationToken).ConfigureAwait(false);
            }
        }
    }

    internal ValueTask<CliPromptReply<T>> TextAsync<T>(CliTextQuestion<T> question, CliPromptPolicy policy, CancellationToken cancellationToken)
        where T : notnull
        => RunAsync<CliTextQuestion<T>, T>(question, policy, cancellationToken,
            static (prompts, question, token) => prompts.TextCoreAsync(question, token));

    private async ValueTask<CliPromptReply<T>> TextCoreAsync<T>(CliTextQuestion<T> question, CancellationToken cancellationToken)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(question.Validate);
        var emptyAnswers = 0;
        while (true)
        {
            await WriteFrameAsync(CliText.Escape(question.Label) + "\n", cancellationToken).ConfigureAwait(false);
            var answer = await _terminal.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (answer is null) return CliPromptReply<T>.Cancelled();
            if (question.Required && string.IsNullOrWhiteSpace(answer))
            {
                if (++emptyAnswers >= 2) return CliPromptReply<T>.Cancelled();
                await WriteFrameAsync(CliText.Escape(question.Rule) + "\n", cancellationToken).ConfigureAwait(false);
                continue;
            }
            emptyAnswers = 0;
            var validation = question.Validate(answer);
            if (validation.IsValid && validation.Value is { } value)
                return CliPromptReply<T>.Answered(value);
            if (validation.IsValid)
                throw new InvalidOperationException("A valid text answer must contain a value.");
            await WriteFrameAsync(CliText.Escape(validation.Error ?? question.Rule) + "\n", cancellationToken).ConfigureAwait(false);
        }
    }

    private async ValueTask<CliPromptReply<TAnswer>> RunAsync<TQuestion, TAnswer>(
        TQuestion question,
        CliPromptPolicy policy,
        CancellationToken cancellationToken,
        CliPromptOperation<TQuestion, TAnswer> operation)
        where TQuestion : notnull
        where TAnswer : notnull
    {
        if (!CanPrompt(policy)) return CliPromptReply<TAnswer>.Unavailable();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await operation(this, question, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CliPromptReply<TAnswer>.Cancelled();
        }
    }

    private ValueTask WriteFrameAsync(string frame, CancellationToken cancellationToken)
        => _terminal.WriteAsync(CliText.PlatformLineEndings(frame).AsMemory(), cancellationToken);

    private async ValueTask<bool> DrawSelectionAsync(CliSelectionFrame frame, CliTerminalViewport viewport, CancellationToken cancellationToken)
    {
        var rendered = CliSelectionFrameRenderer.Render(frame, viewport, _style);
        if (rendered is null) return false;
        if (!_terminal.ClearSelection(cancellationToken)) return false;
        await WriteFrameAsync(rendered, cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static void ValidateChoices<T>(IReadOnlyList<CliChoice<T>> choices) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(choices);
        if (choices.Count == 0 || choices.Select(choice => choice.Value).Distinct().Count() != choices.Count)
            throw new ArgumentException("Choices require distinct values and at least one row.", nameof(choices));
    }
}
