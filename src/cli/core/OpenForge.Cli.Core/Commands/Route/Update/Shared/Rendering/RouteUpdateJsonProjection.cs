using OpenForge.Cli.Core.Commands.Route.Update.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

internal static partial class RouteUpdateJsonProjection
{
    internal static RouteUpdateJsonDocument Create(RouteUpdateResult result)
    {
        CliOperationStage.ValidateResult(result);
        return new RouteUpdateJsonDocument
        {
            SchemaVersion = RouteUpdateDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new RouteUpdateJsonResult
            {
                Mode = RouteUpdateDefinitions.ReadMachineName(result.Mode),
                Target = Target(result.Target),
                Patch = Patch(result.Patch),
                Template = result.Template is null ? null : Template(result.Template),
                Plan = Plan(result.Plan),
                Effects = result.Effects.Select(Effect).ToArray(),
                UnchangedPaths = [.. result.UnchangedPaths],
                Recovery = Recovery(result.Recovery),
                Verification = RouteUpdateDefinitions.ReadMachineName(result.Verification),
                Findings = result.Findings.Select(Finding).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new RouteUpdateJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static RouteUpdateJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy),
        };

    private static RouteUpdateJsonTarget Target(RouteUpdateTarget target)
        => new()
        {
            Requested = target.Requested,
            SelectedBy = target.SelectedBy is { } selectedBy
                ? RouteUpdateDefinitions.ReadMachineName(selectedBy)
                : null,
            Id = target.Id,
            Path = target.Path,
            Form = target.Form is { } form
                ? RouteUpdateDefinitions.ReadMachineName(form)
                : null,
            OverwritePaths = [.. target.OverwritePaths],
        };

    private static RouteUpdateJsonPlan Plan(RouteUpdatePlanFacts plan)
        => new()
        {
            Completeness = RouteUpdateDefinitions.ReadMachineName(plan.Completeness),
            Safety = RouteUpdateDefinitions.ReadMachineName(plan.Safety),
            Body = RouteUpdateDefinitions.ReadMachineName(plan.Body),
        };

    private static RouteUpdateJsonRecovery Recovery(RouteUpdateRecovery recovery)
        => new()
        {
            State = RouteUpdateDefinitions.ReadMachineName(recovery.State),
            ResidualPath = recovery.ResidualPath,
        };

    private static RouteUpdateJsonFinding Finding(RouteUpdateFinding finding)
        => new()
        {
            Code = RouteUpdateDefinitions.ReadMachineName(finding.Code),
            Status = CliStatusDefinitions.Read(finding.Status).MachineName,
            Target = finding.Target,
            Cause = finding.Cause,
        };
}
