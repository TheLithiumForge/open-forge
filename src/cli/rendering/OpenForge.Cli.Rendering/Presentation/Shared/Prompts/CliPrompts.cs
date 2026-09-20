using System.Globalization;
using System.Text;
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
                var key = await _terminal.ReadKeyAsync(cancellationToken).ConfigureAwait(false);
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
            }
        }
    }

    internal ValueTask<CliPromptReply<T>> SelectAsync<T>(CliSelectQuestion<T> question, CliPromptPolicy policy, CancellationToken cancellationToken)
        where T : notnull
        => RunAsync<CliSelectQuestion<T>, T>(question, policy, cancellationToken,
            static (prompts, question, token) => prompts.SelectCoreAsync(question, token));

    private async ValueTask<CliPromptReply<T>> SelectCoreAsync<T>(CliSelectQuestion<T> question, CancellationToken cancellationToken)
        where T : notnull
    {
        ValidateChoices(question.Choices);
        if (_terminal.CanReadKeys)
            return await ReadChoiceKeysAsync(question.Choices, question.Question, leadingBlankLine: false, cancellationToken: cancellationToken).ConfigureAwait(false);

        var previousLines = 0;
        while (true)
        {
            var frame = new StringBuilder().Append(CliText.Escape(question.Question)).Append("\n\n");
            for (var index = 0; index < question.Choices.Count; index++)
            {
                var row = question.Choices[index];
                var prefix = string.Create(CultureInfo.InvariantCulture, $"  {index + 1}. ");
                frame.Append(prefix);
                frame.Append(CliText.Escape(row.Label));
                if (row.Description is { } description) frame.Append("   ").Append(CliText.Escape(description));
                frame.Append('\n');
            }
            frame.Append('\n').Append(CliPromptWording.SelectLine(question.Choices.Count)).Append('\n');
            previousLines = await DrawAsync(frame.ToString(), previousLines, cancellationToken).ConfigureAwait(false);
            var answer = await _terminal.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(answer)) return CliPromptReply<T>.Cancelled();
            if (int.TryParse(answer.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                && number > 0 && number <= question.Choices.Count)
                return CliPromptReply<T>.Answered(question.Choices[number - 1].Value);
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

    private async ValueTask<CliPromptReply<T>> ReadChoiceKeysAsync<T>(
        IReadOnlyList<CliChoice<T>> choices,
        string? heading,
        bool leadingBlankLine,
        CancellationToken cancellationToken)
        where T : notnull
    {
        var cursor = 0;
        var previousLines = 0;
        while (true)
        {
            var frame = new StringBuilder();
            if (heading is not null)
                frame.Append(CliText.Escape(heading)).Append("\n\n");
            if (leadingBlankLine)
                frame.Append('\n');
            for (var index = 0; index < choices.Count; index++)
            {
                var row = choices[index];
                string prefix;
                if (index == cursor)
                    prefix = "  > ";
                else
                    prefix = "    ";
                frame.Append(prefix).Append(CliText.Escape(row.Label));
                if (row.Description is { } description)
                    frame.Append("   ").Append(CliText.Escape(description));
                frame.Append('\n');
            }

            frame.Append('\n').Append(CliPromptWording.SelectKeys()).Append('\n');
            previousLines = await DrawAsync(frame.ToString(), previousLines, cancellationToken).ConfigureAwait(false);
            var key = await _terminal.ReadKeyAsync(cancellationToken).ConfigureAwait(false);
            if (key is null || key.Value.Key == CliKey.Escape)
                return CliPromptReply<T>.Cancelled();
            if (key.Value.Key == CliKey.Up)
                cursor = (cursor + choices.Count - 1) % choices.Count;
            else if (key.Value.Key == CliKey.Down)
                cursor = (cursor + 1) % choices.Count;
            else if (key.Value.Key == CliKey.Enter)
                return CliPromptReply<T>.Answered(choices[cursor].Value);
            else if (key.Value.Key == CliKey.Character && key.Value.Character is >= '1' and <= '9'
                && key.Value.Character - '1' < choices.Count)
                return CliPromptReply<T>.Answered(choices[key.Value.Character - '1'].Value);
        }
    }

    internal ValueTask<CliPromptReply<CliPermissionChoice>> PermissionAsync(CliPermissionQuestion question, CliPromptPolicy policy, CancellationToken cancellationToken)
        => RunAsync<CliPermissionQuestion, CliPermissionChoice>(question, policy, cancellationToken,
            static (prompts, question, token) => prompts.PermissionCoreAsync(question, token));

    private async ValueTask<CliPromptReply<CliPermissionChoice>> PermissionCoreAsync(
        CliPermissionQuestion question,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question.Owner);
        ArgumentNullException.ThrowIfNull(question.Paths);
        var heading = new StringBuilder().Append(CliText.Escape(CliPromptWording.Permission(question.Owner))).Append("\n\n");
        foreach (var path in question.Paths)
            heading.Append("  ").Append(CliText.Escape(path.IsDirectory ? CliPromptWording.Directory(path.Path) : path.Path)).Append('\n');
        await WriteFrameAsync(heading.ToString(), cancellationToken).ConfigureAwait(false);
        if (_terminal.CanReadKeys)
            return await ReadPermissionKeysAsync(cancellationToken).ConfigureAwait(false);

        while (true)
        {
            await WriteFrameAsync("\n" + CliPromptWording.PermissionLine() + "\n", cancellationToken).ConfigureAwait(false);
            var answer = (await _terminal.ReadLineAsync(cancellationToken).ConfigureAwait(false))?.Trim().ToLowerInvariant();
            if (answer is null or "" or "cancel") return CliPromptReply<CliPermissionChoice>.Cancelled();
            if (answer == "always") return CliPromptReply<CliPermissionChoice>.Answered(CliPermissionChoice.Always);
            if (answer == "once") return CliPromptReply<CliPermissionChoice>.Answered(CliPermissionChoice.Once);
        }
    }

    private async ValueTask<CliPromptReply<CliPermissionChoice>> ReadPermissionKeysAsync(CancellationToken cancellationToken)
    {
        var choices = new[]
        {
            new CliChoice<CliPermissionChoice>(CliPermissionChoice.Always, CliPromptWording.Always(), CliPromptWording.AlwaysReason()),
            new CliChoice<CliPermissionChoice>(CliPermissionChoice.Once, CliPromptWording.Once(), CliPromptWording.OnceReason()),
            new CliChoice<CliPermissionChoice>(CliPermissionChoice.Cancel, CliPromptWording.Cancel()),
        };
        var selected = await ReadChoiceKeysAsync(choices, heading: null, leadingBlankLine: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (selected.State != CliPromptState.Answered)
            return selected;
        return selected.Value == CliPermissionChoice.Cancel
            ? CliPromptReply<CliPermissionChoice>.Cancelled()
            : selected;
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

    private async ValueTask<int> DrawAsync(string frame, int previousLines, CancellationToken cancellationToken)
    {
        if (_terminal.CanRedraw && previousLines > 0)
            await _terminal.WriteAsync(string.Create(CultureInfo.InvariantCulture, $"\u001b[{previousLines}A\u001b[J").AsMemory(), cancellationToken).ConfigureAwait(false);
        await WriteFrameAsync(frame, cancellationToken).ConfigureAwait(false);
        return frame.Count(character => character == '\n');
    }

    private static void ValidateChoices<T>(IReadOnlyList<CliChoice<T>> choices) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(choices);
        if (choices.Count == 0 || choices.Select(choice => choice.Value).Distinct().Count() != choices.Count)
            throw new ArgumentException("Choices require distinct values and at least one row.", nameof(choices));
    }
}
