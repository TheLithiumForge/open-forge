using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;

internal sealed class RouteMoveResultBuilder
{
    internal RouteMoveResult Build(RouteMoveResultFormation formation)
    {
        ArgumentNullException.ThrowIfNull(formation);
        var status = ReadStatus(formation.Findings);
        return new RouteMoveResult(formation, status, ReadNext(status, formation.Findings));
    }

    private static CliSemanticStatus ReadStatus(IEnumerable<RouteMoveFinding> findings)
        => CliStatusDefinitions.Collapse(findings.Select(finding => finding.Status));

    private static CliNextAction? ReadNext(
        CliSemanticStatus status,
        IEnumerable<RouteMoveFinding> findings)
    {
        var codes = findings.Select(finding => finding.Code).ToHashSet();
        if (codes.Contains(RouteMoveFindingCode.RecoveryArtifactRetained))
        {
            return RouteMoveNextActions.RetainedRecovery;
        }

        if (codes.Contains(RouteMoveFindingCode.DestinationParentMissing))
        {
            return RouteMoveNextActions.MissingDestinationParent;
        }

        if (status == CliSemanticStatus.Invalid)
        {
            return RouteMoveNextActions.InvalidRequest;
        }

        if (codes.Overlaps([
            RouteMoveFindingCode.WorkspaceLockUnavailable,
            RouteMoveFindingCode.TargetChanged,
            RouteMoveFindingCode.TargetChangedDuringApply]))
        {
            return RouteMoveNextActions.RetryableBlock;
        }

        return status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Attention => RouteMoveNextActions.Blocked,
            CliSemanticStatus.Blocked => RouteMoveNextActions.Blocked,
            CliSemanticStatus.Incomplete => RouteMoveNextActions.Incomplete,
            CliSemanticStatus.Failed => RouteMoveNextActions.Failed,
            CliSemanticStatus.Interrupted => RouteMoveNextActions.Interrupted,
            CliSemanticStatus.Invalid => RouteMoveNextActions.InvalidRequest,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Route Move result status is not defined."),
        };
    }
}
