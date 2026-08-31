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
    {
        if (findings.Count == 0)
        {
            return CliSemanticStatus.Complete;
        }

        var statuses = findings.Select(finding => finding.Status).ToHashSet();
        foreach (var candidate in new[]
                 {
                     CliSemanticStatus.Failed,
                     CliSemanticStatus.Interrupted,
                     CliSemanticStatus.Invalid,
                     CliSemanticStatus.Blocked,
                     CliSemanticStatus.Incomplete,
                     CliSemanticStatus.Attention,
                 })
        {
            if (statuses.Contains(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Route Init findings did not map to a supported aggregate status.");
    }
}
