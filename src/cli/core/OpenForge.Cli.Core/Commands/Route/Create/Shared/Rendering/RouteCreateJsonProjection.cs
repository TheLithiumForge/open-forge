using OpenForge.Cli.Core.Commands.Route.Create.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Rendering;

internal static class RouteCreateJsonProjection
{
    internal static RouteCreateJsonDocument Create(RouteCreateResult result)
    {
        CliOperationStage.ValidateResult(result);
        return new RouteCreateJsonDocument
        {
            SchemaVersion = RouteCreateDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new RouteCreateJsonResult
            {
                Mode = RouteCreateDefinitions.ReadMachineName(result.Mode),
                Target = Target(result.Target),
                Parent = result.Parent is null ? null : Parent(result.Parent),
                Metadata = Metadata(result.Metadata),
                Template = result.Template is null ? null : Template(result.Template),
                Plan = Plan(result.Plan),
                Effects = result.Effects.Select(Effect).ToArray(),
                UnchangedPaths = [.. result.UnchangedPaths],
                Recovery = Recovery(result.Recovery),
                Verification = RouteCreateDefinitions.ReadMachineName(result.Verification),
                Findings = result.Findings.Select(Finding).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new RouteCreateJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static RouteCreateJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy),
        };

    private static RouteCreateJsonTarget Target(RouteCreateTarget target)
        => new()
        {
            Requested = target.Requested,
            Id = target.Id,
            Path = target.Path,
        };

    private static RouteCreateJsonParent Parent(RouteCreateParent parent)
        => new()
        {
            Id = parent.Id,
            Path = parent.Path,
            Form = RouteCreateDefinitions.ReadMachineName(parent.Form),
        };

    private static RouteCreateJsonMetadata Metadata(RouteCreateMetadata metadata)
        => new()
        {
            Description = metadata.Description,
            Responsibility = metadata.Responsibility,
            Tags = [.. metadata.Tags],
        };

    private static RouteCreateJsonTemplate Template(RouteCreateTemplate template)
        => new()
        {
            Requested = template.Requested,
            Id = template.Id,
            Path = template.Path,
            Classification = RouteCreateDefinitions.ReadMachineName(template.Classification),
            BodyByteLength = template.BodyByteLength,
        };

    private static RouteCreateJsonPlan Plan(RouteCreatePlanFacts plan)
        => new()
        {
            Completeness = RouteCreateDefinitions.ReadMachineName(plan.Completeness),
            Safety = RouteCreateDefinitions.ReadMachineName(plan.Safety),
        };

    private static RouteCreateJsonEffect Effect(RouteCreateEffect effect)
        => new()
        {
            Path = effect.Path,
            Kind = RouteCreateDefinitions.ReadMachineName(effect.Kind),
            Action = RouteCreateDefinitions.ReadMachineName(effect.Action),
            Change = new RouteCreateJsonChange
            {
                Before = effect.Change.Before,
                Expected = effect.Change.Expected,
            },
            Outcome = RouteCreateDefinitions.ReadMachineName(effect.Outcome),
            Residual = RouteCreateDefinitions.ReadMachineName(effect.Residual),
        };

    private static RouteCreateJsonRecovery Recovery(RouteCreateRecovery recovery)
        => new()
        {
            State = RouteCreateDefinitions.ReadMachineName(recovery.State),
            ResidualPath = recovery.ResidualPath,
        };

    private static RouteCreateJsonFinding Finding(RouteCreateFinding finding)
        => new()
        {
            Code = RouteCreateDefinitions.ReadMachineName(finding.Code),
            Status = CliStatusDefinitions.Read(finding.Status).MachineName,
            Target = finding.Target,
            Cause = finding.Cause,
        };
}
