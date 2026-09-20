using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Interaction;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Interaction;

internal sealed class RouteInspectSourceSelector
{
    private readonly CliPrompt<RouteInspectSourceSelectionQuestion, string>? _prompt;

    internal RouteInspectSourceSelector(
        CliPrompt<RouteInspectSourceSelectionQuestion, string>? prompt)
    {
        _prompt = prompt;
    }

    internal async ValueTask<CliPromptReply<string>> SelectAsync(
        RouteInspectRequest request,
        RouteInspectSelection collisionSelection,
        CancellationToken cancellationToken)
    {
        if (collisionSelection.CandidatePaths.Count < 2)
        {
            throw new ArgumentException(
                "Source selection requires more than one candidate path.",
                nameof(collisionSelection));
        }

        if (!request.AllowInteractiveSourceSelection || _prompt is null)
        {
            return CliPromptReply<string>.Unavailable();
        }

        var requestedId = collisionSelection.RequestedReference
            ?? throw new InvalidOperationException("An ambiguous source selection requires its requested ID.");
        var reply = await _prompt(
                new RouteInspectSourceSelectionQuestion(
                    requestedId,
                    collisionSelection.CandidatePaths),
                new CliPromptPolicy(true),
                cancellationToken)
            .ConfigureAwait(false);
        if (reply.State != CliPromptState.Answered)
        {
            return reply;
        }

        return collisionSelection.CandidatePaths.Contains(reply.Value, StringComparer.Ordinal)
            ? reply
            : CliPromptReply<string>.Unavailable();
    }
}
