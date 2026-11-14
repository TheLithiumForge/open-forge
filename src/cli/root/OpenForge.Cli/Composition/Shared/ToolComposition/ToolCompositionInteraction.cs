using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Tool.Shared.Writing;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Composition.Shared.ToolComposition;

internal sealed class ToolCompositionInteraction(CliPrompts prompts)
{
    internal bool HasLivePathConsent { get; private set; }

    internal ToolWriteInteraction Create() => new()
    {
        CanPrompt = prompts.CanPrompt(new(true)),
        ConfirmPlan = prompts.PlanConfirmation<ToolWriteResult, ToolWriteData, ToolWriteConsentQuestion>(
            ToolWritePresentation.Rendering, static _ => ToolWriteInteractionPresentation.PlanQuestion()),
        ConfirmOptional = ConfirmOptionalAsync,
        ChooseManager = (question, policy, token) => prompts.SelectAsync(ToolWriteInteractionPresentation.ManagerQuestion(question), policy, token),
        ChoosePath = ChoosePathAsync,
    };

    private async ValueTask<CliPromptReply<bool>> ConfirmOptionalAsync(ToolWriteConsentQuestion question, CliPromptPolicy policy, CancellationToken token)
    {
        var answer = await prompts.SelectAsync(ToolWriteInteractionPresentation.OptionalQuestion(question), policy, token).ConfigureAwait(false);
        if (question.Kind == ToolWriteConsentKind.PathChange)
            HasLivePathConsent = prompts.CanPrompt(policy) && answer.State == CliPromptState.Answered && answer.Value;
        return answer;
    }

    private async ValueTask<CliPromptReply<ToolWritePathAnswer>> ChoosePathAsync(ToolWritePathQuestion question, CliPromptPolicy policy, CancellationToken token)
    {
        var answer = await ToolWriteInteractionPresentation.ChoosePathAsync(prompts, question, policy, token).ConfigureAwait(false);
        HasLivePathConsent = prompts.CanPrompt(policy) && answer.State == CliPromptState.Answered && answer.Value.AddForMe;
        return answer;
    }
}
