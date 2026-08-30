using OpenForge.Cli.Core.Commands.Install.Models.Presentation;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Rendering;

internal static class InstallJsonProjection
{
    internal static InstallJsonDocument Create(InstallResult result)
    {
        CliOperationStage.ValidateResult(result);
        return new InstallJsonDocument
        {
            SchemaVersion = InstallDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new InstallJsonResult
            {
                Mode = InstallDefinitions.ReadMachineName(result.Mode),
                Force = result.Force,
                Automatic = result.Automatic,
                Source = result.Facts.Source is null ? null : Source(result.Facts.Source),
                Classification = result.Facts.Classification is { } classification
                    ? InstallDefinitions.ReadMachineName(classification)
                    : null,
                Footprint = result.Facts.Footprint is null
                    ? null
                    : Footprint(result.Facts.Footprint),
                Effects = result.Facts.Effects.Select(Effect).ToArray(),
                Lifecycle = Lifecycle(result.Facts.Lifecycle),
                Recovery = Recovery(result.Facts.Recovery),
                Verification = InstallDefinitions.ReadMachineName(
                    result.Facts.Verification.State),
                Findings = result.Findings.Select(Finding).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new InstallJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static InstallJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = workspace.SelectedBy switch
            {
                CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
                CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(workspace),
                    workspace.SelectedBy,
                    "The workspace selection method is not defined."),
            },
        };

    private static InstallJsonSource Source(InstallSource source)
        => new()
        {
            InventoryFingerprint = source.InventoryFingerprint,
            AssetCount = source.AssetCount,
        };

    private static InstallJsonFootprint Footprint(InstallFootprint footprint)
        => new()
        {
            PayloadFiles = footprint.PayloadFiles,
            ManagedRegions = footprint.ManagedRegions,
            GeneratedRegions = footprint.GeneratedRegions,
        };

    private static InstallJsonEffect Effect(InstallEffect effect)
        => new()
        {
            Path = effect.Path,
            Kind = InstallDefinitions.ReadMachineName(effect.Kind),
            Action = InstallDefinitions.ReadMachineName(effect.Action),
            SourceAssetPath = effect.SourceAssetPath,
            Outcome = InstallDefinitions.ReadMachineName(effect.Outcome),
            Residual = InstallDefinitions.ReadMachineName(effect.Residual),
        };

    private static InstallJsonLifecycle Lifecycle(InstallLifecycle lifecycle)
        => new()
        {
            Action = InstallDefinitions.ReadMachineName(lifecycle.Action),
            Outcome = InstallDefinitions.ReadMachineName(lifecycle.Outcome),
        };

    private static InstallJsonRecovery Recovery(InstallRecovery recovery)
        => new()
        {
            State = InstallDefinitions.ReadMachineName(recovery.State),
            ResidualPath = recovery.ResidualPath,
        };

    private static InstallJsonFinding Finding(InstallFinding finding)
        => new()
        {
            Code = InstallDefinitions.ReadMachineName(finding.Code),
            Target = finding.Subject,
            Cause = finding.Cause,
        };
}
