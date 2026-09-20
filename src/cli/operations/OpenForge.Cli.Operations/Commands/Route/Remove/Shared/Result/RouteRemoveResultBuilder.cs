using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Result;

internal sealed class RouteRemoveResultBuilder
{
    internal RouteRemoveResult Build(RouteRemoveResultFormation formation)
    {
        ArgumentNullException.ThrowIfNull(formation);
        var status = ReadStatus(formation.Findings);
        return new RouteRemoveResult(formation, status, ReadNext(status, formation));
    }

    private static CliSemanticStatus ReadStatus(IEnumerable<RouteRemoveFinding> findings)
        => CliStatusDefinitions.Collapse(findings.Select(finding => finding.Status));

    private static CliNextAction? ReadNext(
        CliSemanticStatus status,
        RouteRemoveResultFormation formation)
    {
        var findings = formation.Findings;
        var codes = findings.Select(finding => finding.Code).ToHashSet();
        if (codes.Contains(RouteRemoveFindingCode.RecoveryArtifactRetained))
        {
            return RouteRemoveNextActions.RetainedRecovery;
        }

        if (status == CliSemanticStatus.Invalid)
        {
            if (codes.Contains(RouteRemoveFindingCode.ConfirmationRequired))
            {
                return RouteRemoveNextActions.Automatic(
                    formation.Source.Path ?? formation.Source.Requested);
            }

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
