using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Rendering;

internal static class ExtensionUpdatePathsHumanRenderer
{
    internal static void Append(StringBuilder builder, CliPresentationRequest<ExtensionUpdateResult> presentation)
    {
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var comparisons = result.Comparisons.ToLookup(item => item.Path, StringComparer.Ordinal);
        var effects = result.Effects.ToLookup(item => item.Path, StringComparer.Ordinal);
        var navigation = (result.GeneratedNavigation?.Regions ?? []).ToLookup(item => item.Path, StringComparer.Ordinal);
        var paths = comparisons.Select(group => group.Key).Concat(effects.Select(group => group.Key))
            .Concat(navigation.Select(group => group.Key)).Distinct(StringComparer.Ordinal);
        builder.AppendLine(CultureInfo.InvariantCulture, $"Effects: {result.Effects.Count}; comparisons before change: {result.Comparisons.Count}");
        var navigationCount = result.GeneratedNavigation is { } generated ? ExtensionHumanText.Count(generated.Regions.Count) : "unavailable";
        builder.AppendLine($"Generated navigation regions: {navigationCount}");
        var summarized = 0;
        foreach (var path in paths)
        {
            if (!expanded && !effects[path].Any() && !comparisons[path].Any()
                && navigation[path].Any() && navigation[path].All(region => region.State == ExtensionUpdateGeneratedRegionState.Unchanged))
            {
                summarized++;
                continue;
            }

            builder.AppendLine($"  {Value(path)}");
            foreach (var comparison in comparisons[path])
            {
                builder.AppendLine($"    Package {Value(comparison.PackageId)}: workspace {ExtensionUpdateDefinitions.ReadMachineName(comparison.CurrentState)}; intended {ExtensionUpdateDefinitions.ReadMachineName(comparison.IntendedState)}; retired-content removal {ExtensionUpdateDefinitions.ReadMachineName(comparison.RetirementEligibility)}");
                if (comparison.Region is { } region)
                {
                    builder.AppendLine($"      Region: {Value(region)}");
                }

                if (expanded)
                {
                    builder.AppendLine($"""
                        Kind: {ExtensionUpdateDefinitions.ReadMachineName(comparison.Kind)}; source file {Value(comparison.SourceAssetPath)}
                        Fingerprints ({ExtensionUpdateDefinitions.ReadMachineName(comparison.FingerprintKind)}):
                          Installed baseline: {Value(comparison.BaselineFingerprint)}
                          Current workspace: {Value(comparison.CurrentFingerprint)}
                          Selected package: {Value(comparison.IntendedFingerprint)}
                        """);
                }
            }

            foreach (var effect in effects[path])
            {
                builder.AppendLine($"    {ExtensionUpdateDefinitions.ReadMachineName(effect.Action)}: {ExtensionUpdateDefinitions.ReadMachineName(effect.Outcome)}; remaining state {ExtensionUpdateDefinitions.ReadMachineName(effect.Residual)}; package {Value(effect.PackageId ?? "shared")}");
                foreach (var change in effect.Changes)
                {
                    builder.AppendLine($"      {ExtensionUpdateDefinitions.ReadMachineName(change.Kind)}: {ExtensionUpdateDefinitions.ReadMachineName(change.Action)}; region {Value(change.Region ?? "none")}; source {Value(change.SourceAssetPath ?? "none")}");
                }

                if (expanded)
                {
                    builder.AppendLine($"      Effect kind: {ExtensionUpdateDefinitions.ReadMachineName(effect.Kind)}");
                }
            }

            foreach (var region in navigation[path])
            {
                builder.AppendLine($"    Generated navigation: {ExtensionUpdateDefinitions.ReadMachineName(region.State)}");
            }
        }

        if (summarized > 0)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"Unchanged navigation paths summarized: {summarized}; full observations are available with --view expanded.");
        }
    }

    private static string Value(string? value) => ExtensionHumanText.Value(value);
}
