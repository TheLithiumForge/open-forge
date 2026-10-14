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
            ReadNext(formation, findings, status));
    }

    private static Shell.Pipeline.Models.Operation.CliNextAction? ReadNext(
        RouteInitResultFormation formation, IReadOnlyList<RouteInitFinding> findings, CliSemanticStatus status)
    {
        var next = RouteInitDefinitions.ReadNextAction(status, findings);
        if (next is null || formation.Request is not { } request) return next;
        if (next.Command is "open-forge route init" or "open-forge route init --detail debug")
            return new(RouteInitRetryCommand.From(request, diagnostics: status == CliSemanticStatus.Failed), next.Reason);
        return Shell.Definitions.Shared.Commands.CliRetryCommand.InWorkspace(next, formation.Workspace?.LexicalRoot,
            formation.Workspace?.SelectedBy == Framework.Workspace.Models.CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static CliSemanticStatus ReadStatus(
        IReadOnlyList<RouteInitFinding> findings)
        => CliStatusDefinitions.Collapse(findings.Select(finding => finding.Status));
}
