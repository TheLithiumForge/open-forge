using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.OutputText.Tool;

namespace OpenForge.Cli.Core.Presentation.Tool;

internal static class ToolInteractionPresentation
{
    internal static ToolInteraction Create(CliPrompts prompts)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        return new()
        {
            CanPrompt = prompts.CanPrompt(new(true)),
            ConfirmPlan = prompts.PlanConfirmation<ToolWriteResult, ToolWriteData, ToolPlanQuestion>(
                ToolWritePresentation.Rendering, static _ => new(ToolText.PlanQuestion())),
            ConfirmUninstall = (question, policy, token) => prompts.SelectAsync(UninstallQuestion(question), policy, token),
        };
    }

    // The tool's own uninstall can reach the whole account, so Enter keeps it unrun.
    private static CliSelectQuestion<bool> UninstallQuestion(ToolUninstallQuestion question)
        => new(ToolText.UninstallQuestion(), [new(false, ToolText.No()), new(true, ToolText.Yes())])
        {
            Context = [ToolText.UninstallHeading(question.Tool.Name), question.Tool.UninstallDescription ?? string.Empty,
                .. question.Tool.Steps.Select(step => step.Command)],
            EnterChoice = 0,
        };
}
