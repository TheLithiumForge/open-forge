using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Result;

internal sealed class RouteCreateResultBuilder
{
    private static readonly ImmutableArray<CliSemanticStatus> StatusPrecedence =
    [
        CliSemanticStatus.Failed,
        CliSemanticStatus.Interrupted,
        CliSemanticStatus.Invalid,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Incomplete,
        CliSemanticStatus.Attention,
    ];

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
    {
        if (findings.Count == 0)
        {
            return CliSemanticStatus.Complete;
        }

        foreach (var status in StatusPrecedence)
        {
            if (findings.Any(finding => finding.Status == status))
            {
                return status;
            }
        }

        throw new InvalidOperationException(
            "Route Create findings did not map to a supported aggregate status.");
    }
}
