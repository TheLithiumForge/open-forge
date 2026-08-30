using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Interaction;

internal sealed class RouteInspectInteractiveSourceSelector
{
    private const int FirstCandidateNumber = 1;
    private const int MinimumCollisionCandidates = 2;
    private const string ChoicePrompt = "Choose a source by number or exact path: ";

    private readonly CliInteractiveSession? _session;

    internal RouteInspectInteractiveSourceSelector(CliInteractiveSession? session)
    {
        _session = session;
    }

    internal async ValueTask<string?> TrySelectPathAsync(
        RouteInspectRequest request,
        RouteInspectSelection collisionSelection,
        CancellationToken cancellationToken)
    {
        if (collisionSelection.CandidatePaths.Count < MinimumCollisionCandidates)
        {
            throw new ArgumentException(
                "Interactive source selection requires more than one candidate path.",
                nameof(collisionSelection));
        }

        if (!request.AllowInteractiveSourceSelection || _session?.CanPrompt != true)
        {
            return null;
        }

        var sourceId = collisionSelection.RequestedReference
            ?? throw new InvalidOperationException("An ambiguous source selection requires its requested ID.");
        var response = await _session
            .AskAsync(CreatePrompt(sourceId, collisionSelection.CandidatePaths), cancellationToken)
            .ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (response.Answer is not { } answer)
        {
            return null;
        }

        if (int.TryParse(
                answer,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var oneBasedSelection)
            && oneBasedSelection >= FirstCandidateNumber
            && oneBasedSelection <= collisionSelection.CandidatePaths.Count)
        {
            return collisionSelection.CandidatePaths[oneBasedSelection - FirstCandidateNumber];
        }

        return collisionSelection.CandidatePaths.FirstOrDefault(candidatePath =>
            string.Equals(candidatePath, answer, StringComparison.Ordinal));
    }

    private static string CreatePrompt(
        string sourceId,
        IReadOnlyList<string> candidatePaths)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
                CultureInfo.InvariantCulture,
                $"The source ID `{sourceId}` matches more than one source.")
            .AppendLine();
        for (var index = 0; index < candidatePaths.Count; index++)
        {
            builder.AppendLine(
                CultureInfo.InvariantCulture,
                $"{index + FirstCandidateNumber}. {candidatePaths[index]}");
        }

        return builder
            .AppendLine()
            .Append(ChoicePrompt)
            .ToString();
    }
}
