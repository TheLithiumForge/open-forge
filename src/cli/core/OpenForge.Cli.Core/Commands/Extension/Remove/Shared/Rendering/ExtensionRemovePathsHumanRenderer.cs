using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Rendering;

internal static class ExtensionRemovePathsHumanRenderer
{
    internal static void Append(StringBuilder builder, CliPresentationRequest<ExtensionRemoveResult> presentation)
    {
        var result = presentation.Result;
        var plans = result.Paths.ToLookup(item => item.Path, StringComparer.Ordinal);
        var effects = result.Effects.ToLookup(item => item.Path, StringComparer.Ordinal);
        var navigation = (result.GeneratedNavigation?.Regions ?? []).ToLookup(item => item.Path, StringComparer.Ordinal);
        var paths = plans.Select(group => group.Key).Concat(effects.Select(group => group.Key))
            .Concat(navigation.Select(group => group.Key)).Distinct(StringComparer.Ordinal);
        builder.AppendLine(CultureInfo.InvariantCulture, $"Effects: {result.Effects.Count}; planned paths: {result.Paths.Count}");
        var navigationCount = result.GeneratedNavigation is { } generated ? ExtensionHumanText.Count(generated.Regions.Count) : "unavailable";
        builder.AppendLine($"Generated navigation regions: {navigationCount}");
        var summarized = 0;
        foreach (var path in paths)
        {
            if (presentation.Presentation.View == CliView.Compact && !effects[path].Any() && !plans[path].Any()
                && navigation[path].Any() && navigation[path].All(region => region.State == ExtensionRemoveGeneratedRegionState.Unchanged))
            {
                summarized++;
                continue;
            }

            builder.AppendLine($"  {ExtensionHumanText.Value(path)}");
            foreach (var plan in plans[path])
            {
                builder.AppendLine($"""
                        Planned action: {Action(plan.Action)}
                        Owners selected for removal: {ExtensionHumanText.Values(plan.SelectedOwnerIds)}
                        Owners kept: {ExtensionHumanText.Values(plan.RemainingOwnerIds)}
                    """);
                if (presentation.Presentation.View == CliView.Expanded)
                {
                    builder.AppendLine($"    Observed before change: {ExtensionRemoveDefinitions.ReadMachineName(plan.Classification)}");
                }
            }

            foreach (var effect in effects[path])
            {
                builder.AppendLine($"    {ExtensionRemoveDefinitions.ReadMachineName(effect.Action)}: {ExtensionRemoveDefinitions.ReadMachineName(effect.Outcome)}; remaining state {ExtensionRemoveDefinitions.ReadMachineName(effect.Residual)}; package {ExtensionHumanText.Value(effect.PackageId ?? "none")}");
                if (presentation.Presentation.View == CliView.Expanded)
                {
                    builder.AppendLine($"      Kind: {ExtensionRemoveDefinitions.ReadMachineName(effect.Kind)}");
                }
            }

            foreach (var region in navigation[path])
            {
                builder.AppendLine($"    Generated navigation: {ExtensionRemoveDefinitions.ReadMachineName(region.State)}");
            }
        }

        if (summarized > 0)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"Unchanged navigation paths summarized: {summarized}; full observations are available with --view expanded.");
        }
    }

    internal static string Action(ExtensionRemovePathAction value) => value switch
    {
        ExtensionRemovePathAction.RetainShared => "keep under the remaining package owners",
        ExtensionRemovePathAction.KeepAsUnmanaged => "keep local content as unmanaged files",
        ExtensionRemovePathAction.Delete => "delete",
        ExtensionRemovePathAction.ReleaseOwnership => "release ownership of the missing path",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The removal action is not defined."),
    };
}
