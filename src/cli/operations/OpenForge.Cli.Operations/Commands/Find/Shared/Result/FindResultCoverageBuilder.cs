using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Result;

internal sealed class FindResultCoverageBuilder
{
    internal FindCoverage Build(FindResultCoverageInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new FindCoverage(
            ReadOverallCoverage(input.Status),
            ReadMatchingCoverage(input),
            ReadProjectionCoverage(input));
    }

    private static FindCoverageState ReadMatchingCoverage(FindResultCoverageInput input)
    {
        var status = input.Status;
        return status switch
        {
            CliSemanticStatus.Invalid => FindCoverageState.NotStarted,
            CliSemanticStatus.Blocked => FindCoverageState.Blocked,
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => FindCoverageState.Complete,
            CliSemanticStatus.Incomplete => ReadIncompleteMatchingCoverage(input),
            CliSemanticStatus.Failed => ReadTerminalMatchingCoverage(
                input.StageCompletion.Matching,
                FindCoverageState.Failed),
            CliSemanticStatus.Interrupted => ReadTerminalMatchingCoverage(
                input.StageCompletion.Matching,
                FindCoverageState.Interrupted),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };
    }

    private static FindCoverageState ReadIncompleteMatchingCoverage(FindResultCoverageInput input)
    {
        if (input.StageCompletion.Matching == FindCoverageState.Incomplete)
        {
            return FindCoverageState.Incomplete;
        }

        if (input.StageCompletion.Matching == FindCoverageState.Complete
            && input.Findings.All(finding => finding.Status != CliSemanticStatus.Incomplete
                || finding.Code is FindFindingCode.SectionAmbiguous
                    or FindFindingCode.ProjectionUnavailable))
        {
            return FindCoverageState.Complete;
        }

        return FindCoverageState.Incomplete;
    }

    private static FindCoverageState ReadTerminalMatchingCoverage(
        FindCoverageState stage,
        FindCoverageState terminal)
        => stage is FindCoverageState.Complete or FindCoverageState.Incomplete
            ? stage
            : terminal;

    private static FindProjectionCoverageState ReadProjectionCoverage(FindResultCoverageInput input)
    {
        if (!input.ContentRequested)
        {
            return FindProjectionCoverageState.NotRequested;
        }

        var stage = input.StageCompletion.Projection;
        var status = input.Status;
        return status switch
        {
            CliSemanticStatus.Invalid => FindProjectionCoverageState.NotStarted,
            CliSemanticStatus.Blocked => FindProjectionCoverageState.Blocked,
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => FindProjectionCoverageState.Complete,
            CliSemanticStatus.Incomplete => ReadIncompleteProjectionCoverage(input),
            CliSemanticStatus.Failed => ReadTerminalProjectionCoverage(stage, FindProjectionCoverageState.Failed),
            CliSemanticStatus.Interrupted => ReadTerminalProjectionCoverage(
                stage,
                FindProjectionCoverageState.Interrupted),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };
    }

    private static FindProjectionCoverageState ReadIncompleteProjectionCoverage(
        FindResultCoverageInput input)
    {
        var stage = input.StageCompletion.Projection;
        if (stage == FindProjectionCoverageState.Incomplete
            || input.Findings.Any(finding => finding.Code is FindFindingCode.ProjectionUnavailable or FindFindingCode.SectionAmbiguous)
            || input.Matches.SelectMany(match => match.Projections)
                .Any(projection => projection.State is FindProjectionState.Unavailable or FindProjectionState.Ambiguous))
        {
            return FindProjectionCoverageState.Incomplete;
        }

        if (stage == FindProjectionCoverageState.NotStarted && input.Matches.Count != 0)
        {
            return FindProjectionCoverageState.Incomplete;
        }

        return FindProjectionCoverageState.Complete;
    }

    private static FindProjectionCoverageState ReadTerminalProjectionCoverage(
        FindProjectionCoverageState stage,
        FindProjectionCoverageState terminal)
        => stage is FindProjectionCoverageState.Complete or FindProjectionCoverageState.Incomplete
            ? stage
            : terminal;

    private static FindCoverageState ReadOverallCoverage(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => FindCoverageState.Complete,
            CliSemanticStatus.Incomplete => FindCoverageState.Incomplete,
            CliSemanticStatus.Invalid => FindCoverageState.NotStarted,
            CliSemanticStatus.Blocked => FindCoverageState.Blocked,
            CliSemanticStatus.Failed => FindCoverageState.Failed,
            CliSemanticStatus.Interrupted => FindCoverageState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };
}
