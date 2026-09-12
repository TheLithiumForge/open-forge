using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Rendering;

internal static class ExtensionInstallPathsHumanRenderer
{
    internal static void Append(StringBuilder builder, CliPresentationRequest<ExtensionInstallResult> presentation)
    {
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var effects = result.Effects.ToLookup(effect => effect.Path, StringComparer.Ordinal);
        var navigation = (result.GeneratedNavigation?.Regions ?? []).ToLookup(region => region.Path, StringComparer.Ordinal);
        var payload = (result.Footprint?.PayloadTargets ?? []).ToHashSet(StringComparer.Ordinal);
        var regions = (result.Footprint?.GeneratedRegions ?? []).ToHashSet(StringComparer.Ordinal);
        var directories = (result.Footprint?.Directories ?? []).ToHashSet(StringComparer.Ordinal);
        var paths = effects.Select(group => group.Key).Concat(navigation.Select(group => group.Key))
            .Concat(payload).Concat(regions).Concat(directories).Distinct(StringComparer.Ordinal);
        builder.AppendLine(CultureInfo.InvariantCulture, $"Effects: {result.Effects.Count}");
        if (result.Footprint is null)
        {
            builder.AppendLine("Planned paths: unavailable");
        }

        var navigationCount = result.GeneratedNavigation is { } generated ? ExtensionHumanText.Count(generated.Regions.Count) : "unavailable";
        builder.AppendLine($"Generated navigation regions: {navigationCount}");
        var summarized = 0;
        foreach (var path in paths)
        {
            if (!expanded && !effects[path].Any() && !payload.Contains(path) && !directories.Contains(path)
                && navigation[path].Any() && navigation[path].All(region => region.State == ExtensionInstallGeneratedRegionState.Unchanged))
            {
                summarized++;
                continue;
            }

            builder.AppendLine($"  {ExtensionHumanText.Value(path)}");
            foreach (var effect in effects[path])
            {
                builder.AppendLine($"    {ExtensionInstallDefinitions.ReadMachineName(effect.Action)}: {ExtensionInstallDefinitions.ReadMachineName(effect.Outcome)}; remaining state {ExtensionInstallDefinitions.ReadMachineName(effect.Residual)}; package {ExtensionHumanText.Value(effect.PackageId ?? "none")}");
                if (expanded)
                {
                    builder.AppendLine($"      Kind: {ExtensionInstallDefinitions.ReadMachineName(effect.Kind)}");
                }
            }

            if (expanded || !effects[path].Any())
            {
                if (payload.Contains(path))
                {
                    builder.AppendLine("    Planned package file.");
                }
                if (regions.Contains(path))
                {
                    builder.AppendLine("    Generated navigation observed for installation.");
                }
                if (directories.Contains(path))
                {
                    builder.AppendLine("    Planned directory.");
                }
            }

            foreach (var region in navigation[path])
            {
                builder.AppendLine($"    Generated navigation: {ExtensionInstallDefinitions.ReadMachineName(region.State)}");
            }
        }

        if (summarized > 0)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"Unchanged navigation paths summarized: {summarized}; full observations are available with --view expanded.");
        }
    }
}
