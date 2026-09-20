using OpenForge.Cli.Core.Commands.Route.Remove.Models.Interaction;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Remove;
using OpenForge.Cli.Core.Presentation.Route.Remove.Models;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Route.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Interaction;

internal static class RouteRemoveSourceSelectionPrompt
{
    internal static CliPrompt<RouteRemoveSourceSelectionQuestion, string> Create(
        CliPrompts prompts)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        return (question, policy, cancellationToken) => prompts.SelectAsync(
            new CliSelectQuestion<string>(
                RouteSourceSelectionWording.SourceSelection(
                    question.RequestedId,
                    question.CandidatePaths.Count),
                question.CandidatePaths
                    .Select(path => new CliChoice<string>(path, path))
                    .ToArray()),
            policy,
            cancellationToken);
    }

}

internal static class RouteRemoveConfirmationPrompt
{
    internal static CliPlanConfirmation<RouteRemoveResult, RouteRemoveConfirmationQuestion> Create(
        CliPrompts prompts)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        return prompts.PlanConfirmation<RouteRemoveResult, RouteRemoveData, RouteRemoveConfirmationQuestion>(
            RouteRemovePresentation.Rendering,
            question => new CliConfirmQuestion(
                RouteRemoveWording.Confirmation(question.FileCount)));
    }
}
