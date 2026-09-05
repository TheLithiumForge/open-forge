using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Result;

internal sealed class RouteRemoveResultBuilder
{
    internal RouteRemoveResult Build(RouteRemoveResultFormation formation)
    {
        ArgumentNullException.ThrowIfNull(formation);
        var status = ReadStatus(formation.Findings);
        return new RouteRemoveResult(formation, status, ReadNext(status, formation.Findings));
    }

    private static CliSemanticStatus ReadStatus(IEnumerable<RouteRemoveFinding> findings)
    {
        var statuses = findings.Select(finding => finding.Status).ToHashSet();
        if (statuses.Contains(CliSemanticStatus.Failed))
        {
            return CliSemanticStatus.Failed;
        }

        if (statuses.Contains(CliSemanticStatus.Interrupted))
        {
            return CliSemanticStatus.Interrupted;
        }

        if (statuses.Contains(CliSemanticStatus.Invalid))
        {
            return CliSemanticStatus.Invalid;
        }

        if (statuses.Contains(CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (statuses.Contains(CliSemanticStatus.Incomplete))
        {
            return CliSemanticStatus.Incomplete;
        }

        return statuses.Contains(CliSemanticStatus.Attention)
            ? CliSemanticStatus.Attention
            : CliSemanticStatus.Complete;
    }

    private static CliNextAction? ReadNext(
        CliSemanticStatus status,
        IEnumerable<RouteRemoveFinding> findings)
    {
        var codes = findings.Select(finding => finding.Code).ToHashSet();
        if (codes.Contains(RouteRemoveFindingCode.RecoveryArtifactRetained))
        {
            return RouteRemoveNextActions.RetainedRecovery;
        }

        if (status == CliSemanticStatus.Invalid)
        {
            return RouteRemoveNextActions.InvalidRequest;
        }

        if (codes.Overlaps([
            RouteRemoveFindingCode.WorkspaceLockUnavailable,
            RouteRemoveFindingCode.TargetChanged,
            RouteRemoveFindingCode.TargetChangedDuringApply]))
        {
            return RouteRemoveNextActions.RetryableBlock;
        }

        return status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Attention => RouteRemoveNextActions.Blocked,
            CliSemanticStatus.Blocked => RouteRemoveNextActions.Blocked,
            CliSemanticStatus.Incomplete => RouteRemoveNextActions.Incomplete,
            CliSemanticStatus.Failed => RouteRemoveNextActions.Failed,
            CliSemanticStatus.Interrupted => RouteRemoveNextActions.Interrupted,
            CliSemanticStatus.Invalid => RouteRemoveNextActions.InvalidRequest,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Route Remove result status is not defined."),
        };
    }
}
