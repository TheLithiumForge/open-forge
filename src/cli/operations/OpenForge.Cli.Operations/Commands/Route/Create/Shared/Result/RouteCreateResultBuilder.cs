using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Result;

internal sealed class RouteCreateResultBuilder
{
    internal RouteCreateResult Build(RouteCreateResultFormation formation)
    {
        var findings = formation.Findings
            .OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.Target, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToImmutableArray();
        var status = ReadStatus(findings);
        var normalized = formation with
        {
            Findings = findings,
        };
        return new RouteCreateResult(
            normalized,
            status,
            RouteCreateDefinitions.ReadNextAction(status, findings));
    }

    private static CliSemanticStatus ReadStatus(
        IReadOnlyList<RouteCreateFinding> findings)
        => CliStatusDefinitions.Collapse(findings.Select(finding => finding.Status));
}
