using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Models.Operation;

internal sealed record FindResultFindingInput(
    IReadOnlyList<FindFinding> Findings,
    IReadOnlyList<FindLayerInspectionFacts> Inspections,
    IReadOnlyList<FindMatch> Matches,
    bool ContentRequested,
    FindStageCompletion StageCompletion,
    FindTerminalEvent? TerminalEvent);

internal sealed record FindResultFindingFinalizationInput(
    CliSemanticStatus Status,
    IReadOnlyList<FindFinding> Findings,
    FindTerminalEvent? TerminalEvent,
    FindProjectionCoverageState ProjectionCoverage);

internal sealed record FindResultStatusInput(
    FindStageCompletion StageCompletion,
    FindTerminalEvent? TerminalEvent,
    IReadOnlyList<FindFinding> Findings);

internal sealed record FindResultCoverageInput(
    CliSemanticStatus Status,
    FindStageCompletion StageCompletion,
    IReadOnlyList<FindFinding> Findings,
    IReadOnlyList<FindMatch> Matches,
    bool ContentRequested);

internal sealed record FindResultUniverseInput(
    FindRequestEcho Request,
    FindUniverse? Universe,
    IReadOnlyList<FindLayerInspectionFacts> Inspections,
    int MatchCount,
    FindCoverageState MatchingCoverage);

internal sealed record FindResultInspectedCountInput(
    FindRequestEcho Request,
    IReadOnlyList<FindLayerInspectionFacts> Inspections,
    int? CandidateCount,
    FindCoverageState MatchingCoverage);

internal sealed record FindResultNextActionInput(
    CliSemanticStatus Status,
    IReadOnlyList<FindFinding> Findings);
