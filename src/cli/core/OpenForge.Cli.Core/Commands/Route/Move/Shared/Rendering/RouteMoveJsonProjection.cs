using OpenForge.Cli.Core.Commands.Route.Move.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;

internal static partial class RouteMoveJsonProjection
{
    internal static RouteMoveJsonDocument Create(RouteMoveResult result)
    {
        CliOperationStage.ValidateResult(result);
        return new RouteMoveJsonDocument
        {
            SchemaVersion = RouteMoveDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new RouteMoveJsonResult
            {
                Mode = RouteMoveDefinitions.ReadMachineName(result.Mode),
                Source = Source(result.Source),
                Destination = Destination(result.Destination),
                Subject = Subject(result.Subject),
                Ownership = Ownership(result.Ownership),
                References = References(result.References),
                GeneratedNavigation = GeneratedNavigation(result.GeneratedNavigation),
                Plan = Plan(result.Plan),
                Effects = result.Effects.Select(Effect).ToArray(),
                UnchangedPaths = [.. result.UnchangedPaths],
                Recovery = Recovery(result.Recovery),
                Verification = RouteMoveDefinitions.ReadMachineName(result.Verification),
                Findings = result.Findings.Select(Finding).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new RouteMoveJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static RouteMoveJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy),
        };

    private static RouteMoveJsonSource Source(RouteMoveSource source)
        => new()
        {
            Requested = source.Requested,
            SelectedBy = source.SelectedBy is { } selectedBy
                ? RouteMoveDefinitions.ReadMachineName(selectedBy)
                : null,
            Id = source.Id,
            Path = source.Path,
            Form = source.Form is { } form
                ? RouteMoveDefinitions.ReadMachineName(form)
                : null,
        };

    private static RouteMoveJsonDestination Destination(RouteMoveDestination destination)
        => new()
        {
            Requested = destination.Requested,
            Id = destination.Id,
            Path = destination.Path,
            ParentId = destination.ParentId,
            ParentPath = destination.ParentPath,
        };

    private static RouteMoveJsonPlan Plan(RouteMovePlanFacts plan)
        => new()
        {
            Completeness = RouteMoveDefinitions.ReadMachineName(plan.Completeness),
            Safety = RouteMoveDefinitions.ReadMachineName(plan.Safety),
        };

    private static RouteMoveJsonRecovery Recovery(RouteMoveRecovery recovery)
        => new()
        {
            State = RouteMoveDefinitions.ReadMachineName(recovery.State),
            ProtectedPaths = [.. recovery.ProtectedPaths],
            ResidualPath = recovery.ResidualPath,
        };

    private static RouteMoveJsonFinding Finding(RouteMoveFinding finding)
        => new()
        {
            Code = RouteMoveDefinitions.ReadMachineName(finding.Code),
            Status = CliStatusDefinitions.Read(finding.Status).MachineName,
            Target = finding.Target,
            Cause = finding.Cause,
        };
}
