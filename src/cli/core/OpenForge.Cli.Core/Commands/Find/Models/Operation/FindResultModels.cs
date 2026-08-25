using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Find.Models.Operation;

internal sealed record FindRequestEcho
{
    internal FindRequestEcho(
        CliWorkspace? workspace,
        FindUniverseFilter universeFilter,
        FindQuery query,
        FindPresentationSelection presentation)
    {
        ArgumentNullException.ThrowIfNull(universeFilter);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(presentation);
        Workspace = workspace;
        UniverseFilter = universeFilter;
        Query = query;
        Presentation = presentation;
    }

    internal CliWorkspace? Workspace { get; }

    internal FindUniverseFilter UniverseFilter { get; }

    internal FindQuery Query { get; }

    internal FindPresentationSelection Presentation { get; }
}

internal sealed record FindResultInput
{
    internal FindResultInput(
        FindRequestEcho request,
        FindUniverse? universe,
        IEnumerable<FindLayerInspectionFacts> inspections,
        IEnumerable<FindMatch> matches,
        IEnumerable<FindProjection> projections,
        IEnumerable<FindFinding> findings,
        FindStageCompletion stageCompletion,
        FindTerminalEvent? terminalEvent)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(inspections);
        ArgumentNullException.ThrowIfNull(matches);
        ArgumentNullException.ThrowIfNull(projections);
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentNullException.ThrowIfNull(stageCompletion);
        var materializedInspections = inspections.ToArray();
        var materializedMatches = matches.ToArray();
        var materializedProjections = projections.ToArray();
        var materializedFindings = findings.ToArray();
        if (materializedInspections.Any(inspection => inspection is null)
            || materializedMatches.Any(match => match is null)
            || materializedProjections.Any(projection => projection is null)
            || materializedFindings.Any(finding => finding is null))
        {
            throw new ArgumentException("Find result input collections cannot contain null values.");
        }

        Request = request;
        Universe = universe;
        Inspections = Array.AsReadOnly(materializedInspections);
        Matches = Array.AsReadOnly(materializedMatches);
        Projections = Array.AsReadOnly(materializedProjections);
        Findings = Array.AsReadOnly(materializedFindings);
        StageCompletion = stageCompletion;
        TerminalEvent = terminalEvent;
    }

    internal FindRequestEcho Request { get; }

    internal FindUniverse? Universe { get; }

    internal IReadOnlyList<FindLayerInspectionFacts> Inspections { get; }

    internal IReadOnlyList<FindMatch> Matches { get; }

    internal IReadOnlyList<FindProjection> Projections { get; }

    internal IReadOnlyList<FindFinding> Findings { get; }

    internal FindStageCompletion StageCompletion { get; }

    internal FindTerminalEvent? TerminalEvent { get; }
}

internal sealed record FindStageCompletion
{
    internal FindStageCompletion(
        FindCoverageState matching,
        FindProjectionCoverageState projection)
    {
        if (!Enum.IsDefined(matching))
        {
            throw new ArgumentOutOfRangeException(nameof(matching), matching, "The Find matching stage state is not defined.");
        }

        if (!Enum.IsDefined(projection))
        {
            throw new ArgumentOutOfRangeException(nameof(projection), projection, "The Find projection stage state is not defined.");
        }

        Matching = matching;
        Projection = projection;
    }

    internal FindCoverageState Matching { get; }

    internal FindProjectionCoverageState Projection { get; }
}

internal enum FindTerminalEventKind
{
    Failed,
    Interrupted,
}

internal sealed record FindTerminalEvent
{
    internal FindTerminalEvent(FindTerminalEventKind kind, string cause)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find terminal event kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        Kind = kind;
        Cause = cause;
    }

    internal FindTerminalEventKind Kind { get; }

    internal string Cause { get; }
}
