using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Rendering;

internal static class ExtensionInstallPresentation
{
    internal static CliHelpContent CreateHelp()
        => new(
        [
            new CliHelpSection(
                "Command",
                "  open-forge extension install [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--automatic] [--dry-run] [global flags]"),
            new CliHelpSection(
                "Selection and dependencies",
                "  Select exact stable IDs, --all, or the sole package in a one-package source. Dependencies are mandatory and installed first."),
            new CliHelpSection(
                "Interaction and automatic mode",
                "  A prompt-capable human request may select from a finite catalogue. --automatic and non-interactive requests never choose packages or grant force."),
            new CliHelpSection(
                "Initial force",
                "  --force replaces only exact eligible initial occupants; it never reconciles managed divergence."),
            new CliHelpSection(
                "Results and streams",
                "  Dry-run writes nothing. Human and JSON output preserve the shared semantic status, stream, and exit mapping."),
        ]);

    internal static string RenderHuman(CliPresentationRequest<ExtensionInstallResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        var result = presentation.Result;
        var builder = new StringBuilder();
        builder.AppendLine($"""
            Open Forge extension install
            Workspace: {result.Workspace?.LexicalRoot ?? "unavailable"}
            Mode: {ExtensionInstallDefinitions.ReadMachineName(result.Mode)}
            Force: {result.Force.ToString().ToLowerInvariant()}
            Automatic: {result.Automatic.ToString().ToLowerInvariant()}
            """);
        if (result.Selection is { } selection)
        {
            builder.AppendLine(
                $"Selection: {ExtensionInstallDefinitions.ReadMachineName(selection.SelectedBy)}");
        }
        else
        {
            builder.AppendLine("Selection: unavailable");
        }

        AppendValues(
            builder,
            "Root IDs",
            result.Selection?.RootIds ?? []);
        if (result.Source is { } source)
        {
            builder.AppendLine($"""
                Source: {ExtensionInstallDefinitions.ReadMachineName(source.Kind)} / {source.Identity}
                  Path: {source.Path ?? "embedded"}
                  Package count: {source.PackageCount.ToString(CultureInfo.InvariantCulture)}
                """);
        }
        else
        {
            builder.AppendLine("Source: unavailable");
        }

        builder.AppendLine($"Packages: {result.Packages.Count.ToString(CultureInfo.InvariantCulture)}");
        foreach (var package in result.Packages)
        {
            builder.AppendLine($"""
                  {package.Id} (selected root: {package.SelectedRoot.ToString().ToLowerInvariant()})
                    Dependencies: {Values(package.Dependencies)}
                """);
        }

        if (result.Framework is { } framework)
        {
            builder.AppendLine($"""
                Framework: {framework.InventoryFingerprint}
                  Targets: {framework.TargetCount.ToString(CultureInfo.InvariantCulture)}
                  Generated regions: {framework.GeneratedRegionCount.ToString(CultureInfo.InvariantCulture)}
                """);
        }
        else
        {
            builder.AppendLine("Framework: unavailable");
        }

        if (result.Footprint is { } footprint)
        {
            builder.AppendLine(
                $"Footprint: {footprint.PackageCount.ToString(CultureInfo.InvariantCulture)} packages");
        }
        else
        {
            builder.AppendLine("Footprint: unavailable");
        }

        AppendValues(builder, "Payload targets", result.Footprint?.PayloadTargets ?? []);
        AppendValues(builder, "Generated regions", result.Footprint?.GeneratedRegions ?? []);
        AppendValues(builder, "Directories", result.Footprint?.Directories ?? []);
        builder.AppendLine($"Effects: {result.Effects.Count.ToString(CultureInfo.InvariantCulture)}");
        foreach (var effect in result.Effects)
        {
            builder.AppendLine($"""
                  {effect.Path}
                    Package: {effect.PackageId ?? "none"}
                    Kind: {ExtensionInstallDefinitions.ReadMachineName(effect.Kind)}
                    Action: {ExtensionInstallDefinitions.ReadMachineName(effect.Action)}
                    Outcome: {ExtensionInstallDefinitions.ReadMachineName(effect.Outcome)}
                    Residual: {ExtensionInstallDefinitions.ReadMachineName(effect.Residual)}
                """);
        }

        if (result.GeneratedNavigation is { } navigation)
        {
            builder.AppendLine(
                $"Generated navigation: {navigation.Regions.Count.ToString(CultureInfo.InvariantCulture)} regions");
            foreach (var region in navigation.Regions)
            {
                builder.AppendLine($"  {region.Path}: {ExtensionInstallDefinitions.ReadMachineName(region.State)}");
            }
        }
        else
        {
            builder.AppendLine("Generated navigation: unavailable");
        }

        ExtensionPermissionPresentation.Append(builder, result.Permissions);
        builder.AppendLine($"""
            Lifecycle: {ExtensionInstallDefinitions.ReadMachineName(result.Lifecycle.Action)} / {ExtensionInstallDefinitions.ReadMachineName(result.Lifecycle.Outcome)}
            Recovery: {ExtensionInstallDefinitions.ReadMachineName(result.Recovery.State)}
            """);
        AppendValues(builder, "Protected paths", result.Recovery.ProtectedPaths);
        builder.AppendLine($"""
            Residual path: {result.Recovery.ResidualPath ?? "none"}
            Verification:
              Targets: {ExtensionInstallDefinitions.ReadMachineName(result.Verification.Targets)}
              Topology: {ExtensionInstallDefinitions.ReadMachineName(result.Verification.Topology)}
              Extension lifecycle: {ExtensionInstallDefinitions.ReadMachineName(result.Verification.ExtensionsLifecycle)}
              Framework lifecycle: {ExtensionInstallDefinitions.ReadMachineName(result.Verification.FrameworkLifecycle)}
            """);
        builder.AppendLine($"Findings: {result.Findings.Count.ToString(CultureInfo.InvariantCulture)}");
        foreach (var finding in result.Findings)
        {
            builder.AppendLine($"  {ExtensionInstallDefinitions.ReadMachineName(finding.Code)}: {finding.Cause}");
            if (finding.Target is not null)
            {
                builder.AppendLine($"    Target: {finding.Target}");
            }
        }

        builder.AppendLine($"Status: {CliStatusDefinitions.Read(result.Status).MachineName}");
        if (result.Next is { } next)
        {
            builder.AppendLine($"""
                Next: {next.Command}
                  Reason: {next.Reason}
                """);
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendValues(
        StringBuilder builder,
        string label,
        IReadOnlyList<string> values)
        => builder.AppendLine($"{label}: {Values(values)}");

    private static string Values(IReadOnlyList<string> values)
        => values.Count == 0 ? "none" : string.Join(", ", values);

    internal static string? RenderDiagnostic(CliPresentationRequest<ExtensionInstallResult> presentation)
        => $"status={CliStatusDefinitions.Read(presentation.Result.Status).MachineName}; findings={presentation.Result.Findings.Count.ToString(CultureInfo.InvariantCulture)}";

}
