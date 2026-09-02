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
    {
        if (findings.Count == 0)
        {
            return CliSemanticStatus.Complete;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Failed))
        {
            return CliSemanticStatus.Failed;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Interrupted))
        {
            return CliSemanticStatus.Interrupted;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Invalid))
        {
            return CliSemanticStatus.Invalid;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            return CliSemanticStatus.Incomplete;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Attention))
        {
            return CliSemanticStatus.Attention;
        }

        throw new ArgumentException(
            "Route Update findings do not form a supported semantic status.",
            nameof(findings));
    }
}
