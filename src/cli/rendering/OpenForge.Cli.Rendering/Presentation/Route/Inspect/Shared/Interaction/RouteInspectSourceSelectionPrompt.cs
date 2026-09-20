using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Interaction;
using OpenForge.Cli.Core.Presentation.Route.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Interaction;

internal static class RouteInspectSourceSelectionPrompt
{
    internal static CliPrompt<RouteInspectSourceSelectionQuestion, string> Create(
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
