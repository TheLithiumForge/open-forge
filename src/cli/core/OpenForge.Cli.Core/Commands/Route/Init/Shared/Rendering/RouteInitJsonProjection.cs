using OpenForge.Cli.Core.Commands.Route.Init.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;

internal static class RouteInitJsonProjection
{
    internal static RouteInitJsonDocument Create(RouteInitResult result)
    {
        CliOperationStage.ValidateResult(result);
        return new RouteInitJsonDocument
        {
            SchemaVersion = RouteInitDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new RouteInitJsonResult
            {
                Mode = RouteInitDefinitions.ReadMachineName(result.Mode),
                Scaffold = RouteInitDefinitions.ReadMachineName(result.Scaffold),
                Target = Target(result.Target),
                Plan = Plan(result.Plan),
                Framework = result.Framework is null
                    ? null
                    : Framework(result.Framework),
                Entrypoints = result.Entrypoints.Select(Entrypoint).ToArray(),
                Effects = result.Effects.Select(Effect).ToArray(),
                UnchangedPaths = [.. result.UnchangedPaths],
                Lifecycle = Lifecycle(result.Lifecycle),
                Recovery = Recovery(result.Recovery),
                Verification = RouteInitDefinitions.ReadMachineName(result.Verification),
                Findings = result.Findings.Select(Finding).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new RouteInitJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static RouteInitJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy),
        };

    private static RouteInitJsonTarget Target(RouteInitTarget target)
        => new()
        {
            Requested = target.Requested,
            Id = target.Id,
            Path = target.Path,
        };

    private static RouteInitJsonPlan Plan(RouteInitPlanFacts plan)
        => new()
        {
            Completeness = RouteInitDefinitions.ReadMachineName(plan.Completeness),
            Safety = RouteInitDefinitions.ReadMachineName(plan.Safety),
        };

    private static RouteInitJsonFramework Framework(RouteInitFramework framework)
        => new()
        {
            InventoryFingerprint = framework.InventoryFingerprint,
            Segments = framework.Segments.Select(FrameworkSegment).ToArray(),
        };

    private static RouteInitJsonFrameworkSegment FrameworkSegment(
        RouteInitFrameworkSegment segment)
        => new()
        {
            Path = segment.Path,
            Role = RouteInitDefinitions.ReadMachineName(segment.Role),
            SourceAssetPath = segment.SourceAssetPath,
        };

    private static RouteInitJsonEntrypoint Entrypoint(RouteInitEntrypoint entrypoint)
        => new()
        {
            Id = entrypoint.Id,
            Path = entrypoint.Path,
            Form = RouteInitDefinitions.ReadMachineName(entrypoint.Form),
            Current = RouteInitDefinitions.ReadMachineName(entrypoint.Current),
            Ownership = RouteInitDefinitions.ReadMachineName(entrypoint.Ownership),
            Metadata = entrypoint.Metadata is null
                ? null
                : Metadata(entrypoint.Metadata),
            SourceAssetPath = entrypoint.SourceAssetPath,
            Outcome = RouteInitDefinitions.ReadMachineName(entrypoint.Outcome),
        };

    private static RouteInitJsonMetadata Metadata(RouteInitMetadata metadata)
        => new()
        {
            Description = metadata.Description,
            DescriptionSource = RouteInitDefinitions.ReadMachineName(
                metadata.DescriptionSource),
            Responsibility = metadata.Responsibility,
            ResponsibilitySource = RouteInitDefinitions.ReadMachineName(
                metadata.ResponsibilitySource),
            Tags = [.. metadata.Tags],
            TagsSource = RouteInitDefinitions.ReadMachineName(metadata.TagsSource),
        };

    private static RouteInitJsonEffect Effect(RouteInitEffect effect)
        => new()
        {
            Path = effect.Path,
            Kind = RouteInitDefinitions.ReadMachineName(effect.Kind),
            Action = RouteInitDefinitions.ReadMachineName(effect.Action),
            SourceAssetPath = effect.SourceAssetPath,
            Change = effect.Change is null
                ? null
                : new RouteInitJsonChange
                {
                    Before = effect.Change.Before,
                    Expected = effect.Change.Expected,
                },
            Outcome = RouteInitDefinitions.ReadMachineName(effect.Outcome),
            Residual = RouteInitDefinitions.ReadMachineName(effect.Residual),
        };

    private static RouteInitJsonLifecycle Lifecycle(RouteInitLifecycle lifecycle)
        => new()
        {
            Action = RouteInitDefinitions.ReadMachineName(lifecycle.Action),
            Outcome = RouteInitDefinitions.ReadMachineName(lifecycle.Outcome),
        };

    private static RouteInitJsonRecovery Recovery(RouteInitRecovery recovery)
        => new()
        {
            State = RouteInitDefinitions.ReadMachineName(recovery.State),
            ResidualPath = recovery.ResidualPath,
        };

    private static RouteInitJsonFinding Finding(RouteInitFinding finding)
        => new()
        {
            Code = RouteInitDefinitions.ReadMachineName(finding.Code),
            Status = CliStatusDefinitions.Read(finding.Status).MachineName,
            Target = finding.Target,
            Cause = finding.Cause,
        };
}
