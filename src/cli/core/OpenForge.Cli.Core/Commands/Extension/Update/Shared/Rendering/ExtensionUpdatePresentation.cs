using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Rendering;

internal static class ExtensionUpdatePresentation
{
    internal static CliHelpContent CreateHelp()
        => new(
        [
            new CliHelpSection(
                "Command",
                "  open-forge extension update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic] [--dry-run] [global flags]"),
            new CliHelpSection(
                "Selection and dependencies",
                "  Select exact managed stable IDs or --all from one reviewed source. Dependencies are resolved transitively and dependency-first."),
            new CliHelpSection(
                "Authority",
                "  Normal mode preserves changed, missing, retired, shared, and unknown content. --force replaces or restores current expected content; --prune deletes eligible retired content."),
            new CliHelpSection(
                "Results and streams",
                "  Dry-run writes nothing. Human and JSON output preserve the shared semantic status, stream, and exit mapping."),
        ]);

    internal static string RenderHuman(CliPresentationRequest<ExtensionUpdateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        var result = presentation.Result;
        var builder = new StringBuilder();
        builder.AppendLine($"""
            Open Forge extension update
            Workspace: {result.Workspace?.LexicalRoot ?? "unavailable"}
            Mode: {ExtensionUpdateDefinitions.ReadMachineName(result.Mode)}
            Force: {result.Force.ToString().ToLowerInvariant()}
            Prune: {result.Prune.ToString().ToLowerInvariant()}
            Automatic: {result.Automatic.ToString().ToLowerInvariant()}
            Selection: {(result.Selection is null ? "unavailable" : ExtensionUpdateDefinitions.ReadMachineName(result.Selection.SelectedBy))}
            Root IDs: {Values(result.Selection?.RootIds ?? [])}
            """);
        if (result.Source is { } source)
        {
            builder.AppendLine($"""
                Source: {ExtensionUpdateDefinitions.ReadMachineName(source.Kind)} / {source.Identity}
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

        builder.AppendLine($"Comparisons: {result.Comparisons.Count.ToString(CultureInfo.InvariantCulture)}");
        foreach (var comparison in result.Comparisons)
        {
            builder.AppendLine($"""
                  {comparison.Path} / {comparison.PackageId}
                    Kind: {ExtensionUpdateDefinitions.ReadMachineName(comparison.Kind)}
                    Current: {ExtensionUpdateDefinitions.ReadMachineName(comparison.CurrentState)}
                    Intended: {ExtensionUpdateDefinitions.ReadMachineName(comparison.IntendedState)}
                    Retirement: {ExtensionUpdateDefinitions.ReadMachineName(comparison.RetirementEligibility)}
                """);
        }

        if (result.GeneratedNavigation is { } navigation)
        {
            builder.AppendLine(
                $"Generated navigation: {navigation.Regions.Count.ToString(CultureInfo.InvariantCulture)} regions");
            foreach (var region in navigation.Regions)
            {
                builder.AppendLine(
                    $"  {region.Path}: {ExtensionUpdateDefinitions.ReadMachineName(region.State)}");
            }
        }
        else
        {
            builder.AppendLine("Generated navigation: unavailable");
        }

        builder.AppendLine($"Effects: {result.Effects.Count.ToString(CultureInfo.InvariantCulture)}");
        foreach (var effect in result.Effects)
        {
            builder.AppendLine($"""
                  {effect.Path}
                    Package: {effect.PackageId ?? "shared"}
                    Kind: {ExtensionUpdateDefinitions.ReadMachineName(effect.Kind)}
                    Action: {ExtensionUpdateDefinitions.ReadMachineName(effect.Action)}
                    Outcome: {ExtensionUpdateDefinitions.ReadMachineName(effect.Outcome)}
                    Residual: {ExtensionUpdateDefinitions.ReadMachineName(effect.Residual)}
                """);
        }

        var lifecycle = string.Join(
            " / ",
            ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Trust),
            ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Coverage),
            ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Action),
            ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Outcome));
        ExtensionPermissionPresentation.Append(builder, result.Permissions);
        builder.AppendLine($"""
            Lifecycle: {lifecycle}
            Recovery: {ExtensionUpdateDefinitions.ReadMachineName(result.Recovery.State)}
            Protected paths: {Values(result.Recovery.ProtectedPaths)}
            Residual path: {result.Recovery.ResidualPath ?? "none"}
            Verification:
              Targets: {ExtensionUpdateDefinitions.ReadMachineName(result.Verification.Targets)}
              Topology: {ExtensionUpdateDefinitions.ReadMachineName(result.Verification.Topology)}
              Extension lifecycle: {ExtensionUpdateDefinitions.ReadMachineName(result.Verification.ExtensionsLifecycle)}
            Findings: {result.Findings.Count.ToString(CultureInfo.InvariantCulture)}
            """);
        foreach (var finding in result.Findings)
        {
            builder.AppendLine(
                $"  {ExtensionUpdateDefinitions.ReadMachineName(finding.Code)}: {finding.Cause}");
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

    internal static string? RenderDiagnostic(CliPresentationRequest<ExtensionUpdateResult> presentation)
        => $"status={CliStatusDefinitions.Read(presentation.Result.Status).MachineName}; findings={presentation.Result.Findings.Count.ToString(CultureInfo.InvariantCulture)}";

    private static string Values(IReadOnlyList<string> values)
        => values.Count == 0 ? "none" : string.Join(", ", values);
}
