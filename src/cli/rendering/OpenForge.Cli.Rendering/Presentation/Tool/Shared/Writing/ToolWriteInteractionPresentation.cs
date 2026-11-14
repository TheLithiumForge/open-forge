using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.OutputText.Tool.Shared.Writing;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared.Writing;

internal static class ToolWriteInteractionPresentation
{
    private const string CustomShellChoice = "<custom-shell-file>";
    internal static CliConfirmQuestion PlanQuestion() => new(ToolWriteText.PlanQuestion());

    internal static CliSelectQuestion<bool> OptionalQuestion(ToolWriteConsentQuestion question)
        => new(question.Kind switch
        {
            ToolWriteConsentKind.AccountUninstall => ToolWriteText.AccountQuestion(question.ToolId),
            ToolWriteConsentKind.SharedFileDeletion => ToolWriteText.DeleteQuestion(question.ToolId),
            ToolWriteConsentKind.PathChange => ToolWriteText.PathQuestion(),
            _ => throw new ArgumentOutOfRangeException(nameof(question)),
        }, [new(false, ToolWriteText.Keep()), new(true, ToolWriteText.Apply())])
        {
            Context = [.. question.Paths, .. question.Effects.Select(effect => effect.Description)],
        };

    internal static CliSelectQuestion<int> ManagerQuestion(ToolWriteManagerQuestion question)
        => new(ToolWriteText.Manager(question.ToolId),
            [.. question.Choices.Select(choice => new CliChoice<int>(choice.Value, choice.Name,
                $"{choice.Version}, {choice.GlobalBin}, {choice.PathReadiness}"))]);

    internal static async ValueTask<CliPromptReply<ToolWritePathAnswer>> ChoosePathAsync(
        CliPrompts prompts, ToolWritePathQuestion question, CliPromptPolicy policy, CancellationToken cancellationToken)
    {
        var choice = await prompts.SelectAsync(new CliSelectQuestion<bool>(ToolWriteText.PathChoice(question.BinPath),
            [new(false, ToolWriteText.ManualPath()), new(true, ToolWriteText.AddPath())]), policy, cancellationToken).ConfigureAwait(false);
        if (choice.State == CliPromptState.Cancelled) return CliPromptReply<ToolWritePathAnswer>.Cancelled();
        if (choice.State == CliPromptState.Unavailable) return CliPromptReply<ToolWritePathAnswer>.Unavailable();
        if (!choice.Value) return CliPromptReply<ToolWritePathAnswer>.Answered(new(false, []));
        if (!question.UsesShellFiles) return CliPromptReply<ToolWritePathAnswer>.Answered(new(true, []));
        var files = await prompts.MultiSelectAsync(new CliMultiSelectQuestion<string>(
            ToolWriteText.ShellFiles(), [.. question.Files.Select(file => new CliChoice<string>(file.Path, file.Path)),
                new(CustomShellChoice, ToolWriteText.CustomShellFile())],
            [], new HashSet<string>(StringComparer.Ordinal), CliSelectionAction.Install), policy, cancellationToken).ConfigureAwait(false);
        if (files.State == CliPromptState.Answered && files.Value.Chosen.Contains(CustomShellChoice, StringComparer.Ordinal))
        {
            var custom = await prompts.TextAsync(new CliTextQuestion<string>(
                ToolWriteText.CustomShellPath(), ToolWriteText.AbsolutePathRule(), true,
                path => new(Path.IsPathFullyQualified(path), path, ToolWriteText.AbsolutePathRule())), policy, cancellationToken).ConfigureAwait(false);
            if (custom.State == CliPromptState.Cancelled) return CliPromptReply<ToolWritePathAnswer>.Cancelled();
            if (custom.State == CliPromptState.Unavailable) return CliPromptReply<ToolWritePathAnswer>.Unavailable();
            var shell = await prompts.SelectAsync(new CliSelectQuestion<bool>(ToolWriteText.ShellSyntax(),
                [new(false, ToolWriteText.PosixShell()), new(true, ToolWriteText.FishShell())]), policy, cancellationToken).ConfigureAwait(false);
            if (shell.State == CliPromptState.Cancelled) return CliPromptReply<ToolWritePathAnswer>.Cancelled();
            if (shell.State == CliPromptState.Unavailable) return CliPromptReply<ToolWritePathAnswer>.Unavailable();
            return CliPromptReply<ToolWritePathAnswer>.Answered(new(true,
                [.. question.Files.Where(file => files.Value.Chosen.Contains(file.Path, StringComparer.Ordinal)), new(custom.Value, shell.Value)]));
        }
        return files.State switch
        {
            CliPromptState.Answered => CliPromptReply<ToolWritePathAnswer>.Answered(new(true,
                [.. question.Files.Where(file => files.Value.Chosen.Contains(file.Path, StringComparer.Ordinal))])),
            CliPromptState.Cancelled => CliPromptReply<ToolWritePathAnswer>.Cancelled(),
            CliPromptState.Unavailable => CliPromptReply<ToolWritePathAnswer>.Unavailable(),
            _ => throw new ArgumentOutOfRangeException(nameof(files)),
        };
    }
}
