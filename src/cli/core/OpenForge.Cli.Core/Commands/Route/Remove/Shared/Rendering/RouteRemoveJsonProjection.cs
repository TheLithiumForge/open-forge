using OpenForge.Cli.Core.Commands.Route.Remove.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Rendering;

internal static partial class RouteRemoveJsonProjection
{
    internal static RouteRemoveJsonDocument Create(RouteRemoveResult result)
    {
        CliOperationStage.ValidateResult(result);
        return new RouteRemoveJsonDocument
        {
            SchemaVersion = RouteRemoveDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new RouteRemoveJsonResult
            {
                Mode = RouteRemoveDefinitions.ReadMachineName(result.Mode),
                Source = Source(result.Source),
                Subject = Subject(result.Subject),
                Ownership = Ownership(result.Ownership),
                References = References(result.References),
                GeneratedNavigation = GeneratedNavigation(result.GeneratedNavigation),
                Plan = Plan(result.Plan),
                Effects = result.Effects.Select(Effect).ToArray(),
                UnchangedPaths = [.. result.UnchangedPaths],
                Recovery = Recovery(result.Recovery),
                Verification = RouteRemoveDefinitions.ReadMachineName(result.Verification),
                Findings = result.Findings.Select(Finding).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new RouteRemoveJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static RouteRemoveJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy),
        };

    private static RouteRemoveJsonSource Source(RouteRemoveSource source)
        => new()
        {
            Requested = source.Requested,
            SelectedBy = source.SelectedBy is { } selectedBy
                ? RouteRemoveDefinitions.ReadMachineName(selectedBy)
                : null,
            Id = source.Id,
            Path = source.Path,
            Form = source.Form is { } form
                ? RouteRemoveDefinitions.ReadMachineName(form)
                : null,
        };

    private static RouteRemoveJsonPlan Plan(RouteRemovePlanFacts plan)
        => new()
        {
            Completeness = RouteRemoveDefinitions.ReadMachineName(plan.Completeness),
            Safety = RouteRemoveDefinitions.ReadMachineName(plan.Safety),
        };

    private static RouteRemoveJsonRecovery Recovery(RouteRemoveRecovery recovery)
        => new()
        {
            State = RouteRemoveDefinitions.ReadMachineName(recovery.State),
            ProtectedPaths = [.. recovery.ProtectedPaths],
            ResidualPath = recovery.ResidualPath,
        };

    private static RouteRemoveJsonFinding Finding(RouteRemoveFinding finding)
        => new()
        {
            Code = RouteRemoveDefinitions.ReadMachineName(finding.Code),
            Status = CliStatusDefinitions.Read(finding.Status).MachineName,
            Target = finding.Target,
            Cause = finding.Cause,
        };
}
