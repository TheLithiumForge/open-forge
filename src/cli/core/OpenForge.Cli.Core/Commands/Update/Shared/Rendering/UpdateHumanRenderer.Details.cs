using System.Text;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

internal static partial class UpdateHumanRenderer
{
    private static void AppendComparisons(
        StringBuilder builder,
        IEnumerable<UpdateComparison> comparisons,
        bool expanded)
    {
        foreach (var comparison in comparisons)
        {
            var kind = UpdateDefinitions.ReadMachineName(comparison.Kind);
            var region = comparison.RegionIdentity is null
                ? string.Empty
                : $" / region={Value(comparison.RegionIdentity)}";
            var current = UpdateDefinitions.ReadMachineName(comparison.CurrentState);
            var intended = UpdateDefinitions.ReadMachineName(comparison.IntendedState);
            var retirement = UpdateDefinitions.ReadMachineName(comparison.RetirementEligibility);
            builder.AppendLine(
                $"    Comparison: {kind}{region}; current: {current}; intended: {intended}; retirement: {retirement}");
            if (!expanded)
            {
                continue;
            }

            var fingerprintKind = UpdateDefinitions.ReadMachineName(comparison.FingerprintKind);
            var baselineFingerprint = Value(comparison.BaselineFingerprint);
            var currentFingerprint = Value(comparison.CurrentFingerprint);
            var intendedFingerprint = Value(comparison.IntendedFingerprint);
            builder.AppendLine($"""
                    Source: {Value(comparison.SourceAssetPath)} / present={OptionalBoolean(comparison.SourceAssetPresentInCurrentInventory)}
                    Fingerprints: policy={fingerprintKind} / baseline={baselineFingerprint} / current={currentFingerprint} / intended={intendedFingerprint}
                """.Replace("\n", Environment.NewLine, StringComparison.Ordinal));
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
        UpdateResult result,
        bool expanded)
    {
        var comparisons = result.Comparisons.ToLookup(comparison => comparison.RelativePath, StringComparer.Ordinal);
        var represented = new HashSet<string>(StringComparer.Ordinal);
        builder.AppendLine($"Effects: {result.Effects.Count}");
        foreach (var effect in result.Effects)
        {
            var action = UpdateDefinitions.ReadMachineName(effect.Action);
            var outcome = UpdateDefinitions.ReadMachineName(effect.Outcome);
            var residual = UpdateDefinitions.ReadMachineName(effect.Residual);
            builder.AppendLine(
                $"  {Value(effect.Path)}: {action}; {outcome}; residual: {residual}");
            foreach (var change in effect.Changes)
            {
                AppendLogicalChange(builder, change);
            }

            if (represented.Add(effect.Path))
            {
                AppendComparisons(builder, comparisons[effect.Path], expanded);
            }
        }

        var otherPaths = comparisons.Where(group => !represented.Contains(group.Key)).ToArray();
        if (otherPaths.Length != 0)
        {
            builder.AppendLine("Inspected paths without effects:");
        }

        foreach (var group in otherPaths)
        {
            builder.AppendLine($"  {Value(group.Key)}");
            AppendComparisons(builder, group, expanded);
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
        UpdateRecovery recovery)
    {
        builder.AppendLine(
            $"Recovery: {UpdateDefinitions.ReadMachineName(recovery.State)} / residual={Value(recovery.ResidualPath)}");
        foreach (var path in recovery.ProtectedPaths)
        {
            builder.AppendLine($"  Protected: {Value(path)}");
        }
    }

    private static void AppendFindings(
        StringBuilder builder,
        IReadOnlyList<UpdateFinding> findings, CliHumanStyle style)
    {
        foreach (var finding in findings)
        {
            builder.AppendLine(
                $"{style.Finding(finding.Status)}: {FindingCause(finding)} [{UpdateDefinitions.ReadMachineName(finding.Code)}]");
            if (finding.Target is { } target)
            {
                builder.AppendLine($"  {Value(target)}");
            }
        }
    }

    private static string FindingCause(UpdateFinding finding) => finding.Code switch
    {
        UpdateFindingCode.ManagedDivergence => "Local changes were kept because replacement was not requested.",
        UpdateFindingCode.ManagedTargetMissing => "The managed path is missing. Update left it absent.",
        UpdateFindingCode.RetiredContentPreserved => "This path is no longer in the selected Framework. Update did not remove it.",
        _ => Value(finding.Cause),
    };

    private static string OptionalBoolean(bool? value)
        => value switch
        {
            true => "true",
            false => "false",
            null => "unavailable",
        };

    private static string Value(string? value)
        => value is null ? "unavailable" : CommandTextEscaping.Escape(value);
}
