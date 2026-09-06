using System.Text;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

internal static partial class UpdateHumanRenderer
{
    private static void AppendComparisons(
        StringBuilder builder,
        IReadOnlyList<UpdateComparison> comparisons,
        bool expanded)
    {
        builder.AppendLine($"Comparisons: {comparisons.Count}");
        foreach (var comparison in comparisons)
        {
            var path = Value(comparison.RelativePath);
            var kind = UpdateDefinitions.ReadMachineName(comparison.Kind);
            var region = comparison.RegionIdentity is null
                ? string.Empty
                : $" / region={Value(comparison.RegionIdentity)}";
            var current = UpdateDefinitions.ReadMachineName(comparison.CurrentState);
            var intended = UpdateDefinitions.ReadMachineName(comparison.IntendedState);
            var retirement = UpdateDefinitions.ReadMachineName(comparison.RetirementEligibility);
            builder.AppendLine(
                $"  {path}: {kind}{region} / current={current} / intended={intended} / retirement={retirement}");
            if (!expanded)
            {
                continue;
            }

            var fingerprintKind = UpdateDefinitions.ReadMachineName(comparison.FingerprintKind);
            var baselineFingerprint = Value(comparison.BaselineFingerprint);
            var currentFingerprint = Value(comparison.CurrentFingerprint);
            var intendedFingerprint = Value(comparison.IntendedFingerprint);
            builder.AppendLine(
                $"    Source: {Value(comparison.SourceAssetPath)} / present={OptionalBoolean(comparison.SourceAssetPresentInCurrentInventory)}");
            builder.AppendLine(
                $"    Fingerprints: policy={fingerprintKind} / baseline={baselineFingerprint} / current={currentFingerprint} / intended={intendedFingerprint}");
        }
    }

    private static void AppendGeneratedNavigation(
        StringBuilder builder,
        UpdateGeneratedNavigation? navigation)
    {
        if (navigation is null)
        {
            builder.AppendLine("Generated navigation: unavailable");
            return;
        }

        builder.AppendLine(
            $"Generated navigation: {UpdateDefinitions.ReadMachineName(navigation.Coverage)}");
        foreach (var region in navigation.Regions)
        {
            builder.AppendLine(
                $"  {Value(region.Path)}: {UpdateDefinitions.ReadMachineName(region.State)}");
        }
    }

    private static void AppendEffects(
        StringBuilder builder,
        IReadOnlyList<UpdatePhysicalEffect> effects)
    {
        builder.AppendLine($"Effects: {effects.Count}");
        foreach (var effect in effects)
        {
            var action = UpdateDefinitions.ReadMachineName(effect.Action);
            var outcome = UpdateDefinitions.ReadMachineName(effect.Outcome);
            var residual = UpdateDefinitions.ReadMachineName(effect.Residual);
            builder.AppendLine(
                $"  {Value(effect.Path)}: {action} / {outcome} / residual={residual}");
            foreach (var change in effect.Changes)
            {
                AppendLogicalChange(builder, change);
            }
        }
    }

    private static void AppendLogicalChange(
        StringBuilder builder,
        UpdateLogicalChange change)
    {
        var kind = UpdateDefinitions.ReadMachineName(change.Kind);
        var action = UpdateDefinitions.ReadMachineName(change.Action);
        builder.AppendLine(
            $"    {kind}: {action} / region={Value(change.Region)} / source={Value(change.SourceAssetPath)}");
    }

    private static void AppendLifecycle(StringBuilder builder, UpdateLifecycle lifecycle)
    {
        var trust = UpdateDefinitions.ReadMachineName(lifecycle.Trust);
        var coverage = UpdateDefinitions.ReadMachineName(lifecycle.Coverage);
        var action = UpdateDefinitions.ReadMachineName(lifecycle.Action);
        var outcome = UpdateDefinitions.ReadMachineName(lifecycle.Outcome);
        builder.AppendLine(
            $"Lifecycle: trust={trust} / coverage={coverage} / action={action} / outcome={outcome}");
    }

    private static void AppendRecovery(
        StringBuilder builder,
        UpdateRecovery recovery,
        bool showProtectedPaths)
    {
        builder.AppendLine(
            $"Recovery: {UpdateDefinitions.ReadMachineName(recovery.State)} / residual={Value(recovery.ResidualPath)}");
        if (!showProtectedPaths)
        {
            return;
        }

        foreach (var path in recovery.ProtectedPaths)
        {
            builder.AppendLine($"  Protected: {Value(path)}");
        }
    }

    private static void AppendFindings(
        StringBuilder builder,
        IReadOnlyList<UpdateFinding> findings,
        bool showFindings)
    {
        builder.AppendLine($"Findings: {findings.Count}");
        if (!showFindings)
        {
            return;
        }

        foreach (var finding in findings)
        {
            builder.AppendLine(
                $"  {UpdateDefinitions.ReadMachineName(finding.Code)} / target={Value(finding.Target)} / {Value(finding.Cause)}");
        }
    }

    private static string OptionalBoolean(bool? value)
        => value switch
        {
            true => "true",
            false => "false",
            null => "unavailable",
        };

    private static string Value(string? value)
        => value is null ? "unavailable" : UpdateTextEscaping.Escape(value);
}
