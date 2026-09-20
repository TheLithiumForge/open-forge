using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Result;

internal sealed class RouteUpdateResultBuilder
{
    internal RouteUpdateResult Build(RouteUpdateResultFormation formation)
    {
        var status = ReadStatus(formation.Findings);
        return new RouteUpdateResult(
            formation,
            status,
            RouteUpdateDefinitions.ReadNextAction(status, formation.Findings));
    }

    private static CliSemanticStatus ReadStatus(
        IReadOnlyList<RouteUpdateFinding> findings)
        => CliStatusDefinitions.Collapse(findings.Select(finding => finding.Status));
}
