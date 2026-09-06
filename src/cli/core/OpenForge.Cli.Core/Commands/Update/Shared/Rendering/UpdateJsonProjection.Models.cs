using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Presentation;
using OpenForge.Cli.Core.Commands.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

internal static partial class UpdateJsonProjection
{
    private static UpdateJsonSource Source(UpdateSource source)
        => new()
        {
            Id = source.Id,
            Version = source.Version,
            InventoryFingerprint = source.InventoryFingerprint,
            AssetCount = source.AssetCount,
        };

    private static UpdateJsonComparison Comparison(UpdateComparison comparison)
        => new()
        {
            Path = comparison.RelativePath,
            Kind = UpdateDefinitions.ReadMachineName(comparison.Kind),
            Region = comparison.RegionIdentity,
            SourceAssetPath = comparison.SourceAssetPath,
            FingerprintKind = UpdateDefinitions.ReadMachineName(comparison.FingerprintKind),
            BaselineFingerprint = comparison.BaselineFingerprint,
            CurrentFingerprint = comparison.CurrentFingerprint,
            IntendedFingerprint = comparison.IntendedFingerprint,
            CurrentState = UpdateDefinitions.ReadMachineName(comparison.CurrentState),
            IntendedState = UpdateDefinitions.ReadMachineName(comparison.IntendedState),
            RetirementEligibility = UpdateDefinitions.ReadMachineName(comparison.RetirementEligibility),
        };

    private static UpdateJsonGeneratedNavigation GeneratedNavigation(
        UpdateGeneratedNavigation navigation)
        => new()
        {
            Coverage = UpdateDefinitions.ReadMachineName(navigation.Coverage),
            Regions = navigation.Regions.Select(GeneratedNavigationRegion).ToArray(),
        };

    private static UpdateJsonGeneratedNavigationRegion GeneratedNavigationRegion(
        UpdateGeneratedNavigationRegion region)
        => new()
        {
            Path = region.Path,
            State = UpdateDefinitions.ReadMachineName(region.State),
        };

    private static UpdateJsonEffect Effect(UpdatePhysicalEffect effect)
        => new()
        {
            Path = effect.Path,
            Action = UpdateDefinitions.ReadMachineName(effect.Action),
            Changes = effect.Changes.Select(LogicalChange).ToArray(),
            Outcome = UpdateDefinitions.ReadMachineName(effect.Outcome),
            Residual = UpdateDefinitions.ReadMachineName(effect.Residual),
        };

    private static UpdateJsonLogicalChange LogicalChange(UpdateLogicalChange change)
        => new()
        {
            Kind = UpdateDefinitions.ReadMachineName(change.Kind),
            Action = UpdateDefinitions.ReadMachineName(change.Action),
            Region = change.Region,
            SourceAssetPath = change.SourceAssetPath,
        };

    private static UpdateJsonLifecycle Lifecycle(UpdateLifecycle lifecycle)
        => new()
        {
            Trust = UpdateDefinitions.ReadMachineName(lifecycle.Trust),
            Coverage = UpdateDefinitions.ReadMachineName(lifecycle.Coverage),
            Action = UpdateDefinitions.ReadMachineName(lifecycle.Action),
            Outcome = UpdateDefinitions.ReadMachineName(lifecycle.Outcome),
        };

    private static UpdateJsonRecovery Recovery(UpdateRecovery recovery)
        => new()
        {
            State = UpdateDefinitions.ReadMachineName(recovery.State),
            ProtectedPaths = recovery.ProtectedPaths.ToArray(),
            ResidualPath = recovery.ResidualPath,
        };

    private static UpdateJsonFinding Finding(UpdateFinding finding)
        => new()
        {
            Code = UpdateDefinitions.ReadMachineName(finding.Code),
            Target = finding.Target,
            Cause = finding.Cause,
        };
}
