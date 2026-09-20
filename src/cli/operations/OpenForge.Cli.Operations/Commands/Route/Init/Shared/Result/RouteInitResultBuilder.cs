using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Result;

internal sealed class RouteInitResultBuilder
{
    internal RouteInitResult Build(RouteInitResultFormation formation)
    {
        ArgumentNullException.ThrowIfNull(formation);
        var findings = formation.Findings
            .OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.Target, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToImmutableArray();
        var status = ReadStatus(findings);
        return new RouteInitResult(
            formation,
            findings,
            status,
            RouteInitDefinitions.ReadNextAction(status, findings));
    }

    private static CliSemanticStatus ReadStatus(
        IReadOnlyList<RouteInitFinding> findings)
        => CliStatusDefinitions.Collapse(findings.Select(finding => finding.Status));
}
