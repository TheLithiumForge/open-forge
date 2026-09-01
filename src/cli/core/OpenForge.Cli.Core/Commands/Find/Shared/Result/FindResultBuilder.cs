using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Result;

internal sealed class FindResultBuilder
{
    private readonly FindResultCoverageBuilder _coverageBuilder = new();
    private readonly FindResultFindingBuilder _findingBuilder = new();
    private readonly FindResultMatchBuilder _matchBuilder = new();
    private readonly FindResultUniverseBuilder _universeBuilder = new();

    internal FindResult Build(FindResultInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var contentRequested = input.Request.Presentation.Content.IsRequested;
        var orderedMatches = _matchBuilder.Build(new FindResultMatchInput(
            input.Matches,
            input.Projections,
            input.Request.Presentation.Content));
        var preliminaryFindings = _findingBuilder.Build(new FindResultFindingInput(
            input.Findings,
            input.Inspections,
            orderedMatches,
            contentRequested,
            input.StageCompletion,
            input.TerminalEvent));
        var status = FindResultStatusPolicy.Read(new FindResultStatusInput(
            input.StageCompletion,
            input.TerminalEvent,
            preliminaryFindings));
        var coverage = _coverageBuilder.Build(new FindResultCoverageInput(
            status,
            input.StageCompletion,
            preliminaryFindings,
            orderedMatches,
            contentRequested));

        if (status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked)
        {
            orderedMatches = [];
        }

        var orderedFindings = _findingBuilder.Finalize(new FindResultFindingFinalizationInput(
            status,
            preliminaryFindings,
            input.TerminalEvent,
            coverage.Projection));
        var universe = _universeBuilder.Build(new FindResultUniverseInput(
            input.Request,
            input.Universe,
            input.Inspections,
            orderedMatches.Count,
            coverage.Matching));

        return new FindResult(
            status,
            input.Request.Workspace,
            universe,
            input.Request.Query,
            input.Request.Presentation,
            coverage,
            orderedFindings,
            orderedMatches,
            FindResultNextActionPolicy.Read(new FindResultNextActionInput(status, orderedFindings)));
    }
}
