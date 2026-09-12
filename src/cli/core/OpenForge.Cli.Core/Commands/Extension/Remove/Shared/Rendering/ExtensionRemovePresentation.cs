using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Rendering;

internal static class ExtensionRemovePresentation
{
    internal static CliHelpContent CreateHelp()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge extension remove [<stable-id>...] [--prune] [--automatic] [--dry-run] [global options]"),
            new CliHelpSection(
                "Selection and dependencies",
                """
                  Select exact managed stable IDs, or choose from the interactive package list.
                  A dependency cannot be removed while a retained package needs it. Unused dependencies remain registered.
                """),
            new CliHelpSection(
                "Ownership and changed content",
                """
                  Shared paths remain owned by retained packages.
                  When the last owner is removed, unchanged files may be deleted. Changed files stay unmanaged
                  unless this request includes --prune.
                """),
            new CliHelpSection(
                "Results and streams",
                """
                  Dry-run writes nothing.
                  Human and JSON output preserve the shared semantic status, stream, recovery, and next-action mapping.
                """),
        ]);

    internal static string RenderHuman(CliPresentationRequest<ExtensionRemoveResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        var result = presentation.Result;
        var selection = result.Selection is null
            ? "unavailable"
            : ExtensionRemoveDefinitions.ReadMachineName(result.Selection.SelectedBy);
        var builder = new StringBuilder();
        builder.AppendLine($"""
            Open Forge extension remove
            Workspace: {result.Workspace?.LexicalRoot ?? "unavailable"}
            Mode: {ExtensionRemoveDefinitions.ReadMachineName(result.Mode)}
            Prune: {result.Prune.ToString().ToLowerInvariant()}
            Automatic: {result.Automatic.ToString().ToLowerInvariant()}
            Selection: {selection}
            IDs: {Values(result.Selection?.Ids ?? [])}
            """);

        if (result.Dependencies is { } dependencies)
        {
            builder.AppendLine($"Dependencies: {dependencies.Packages.Count.ToString(CultureInfo.InvariantCulture)}");
            foreach (var package in dependencies.Packages)
            {
                builder.AppendLine($"""
                      {package.Id} (selected: {package.SelectedForRemoval.ToString().ToLowerInvariant()})
                        Dependencies: {Values(package.Dependencies)}
                    """);
            }

            builder.AppendLine($"Removal order: {Values(dependencies.RemovalOrder)}");
            var blockerCount = dependencies.RetainedDependentBlockers.Count.ToString(CultureInfo.InvariantCulture);
            builder.AppendLine($"Retained-dependent blockers: {blockerCount}");
            foreach (var blocker in dependencies.RetainedDependentBlockers)
            {
                builder.AppendLine(
                    $"  {blocker.DependencyId}: retained {Values(blocker.RetainedDependentIds)}");
            }

            builder.AppendLine(
                $"Retained orphan dependencies: {Values(dependencies.RetainedOrphanDependencyIds)}");
        }
        else
        {
            builder.AppendLine("Dependencies: unavailable");
        }

        builder.AppendLine($"Paths: {result.Paths.Count.ToString(CultureInfo.InvariantCulture)}");
        foreach (var path in result.Paths)
        {
            builder.AppendLine($"  {path.Path}");
            builder.AppendLine(
                $"    Classification: {ExtensionRemoveDefinitions.ReadMachineName(path.Classification)}");
            builder.AppendLine($"    Selected owners: {Values(path.SelectedOwnerIds)}");
            builder.AppendLine($"    Remaining owners: {Values(path.RemainingOwnerIds)}");
            builder.AppendLine($"    Action: {ExtensionRemoveDefinitions.ReadMachineName(path.Action)}");
        }

        if (result.GeneratedNavigation is { } navigation)
        {
            var regionCount = navigation.Regions.Count.ToString(CultureInfo.InvariantCulture);
            builder.AppendLine($"Generated navigation: {regionCount} regions");
            foreach (var region in navigation.Regions)
            {
                builder.AppendLine(
                    $"  {region.Path}: {ExtensionRemoveDefinitions.ReadMachineName(region.State)}");
            }
        }
        else
        {
            builder.AppendLine("Generated navigation: unavailable");
        }

        builder.AppendLine($"Effects: {result.Effects.Count.ToString(CultureInfo.InvariantCulture)}");
        foreach (var effect in result.Effects)
        {
            builder.AppendLine($"  {effect.Path}");
            builder.AppendLine($"    Package: {effect.PackageId ?? "none"}");
            builder.AppendLine($"    Kind: {ExtensionRemoveDefinitions.ReadMachineName(effect.Kind)}");
            builder.AppendLine($"    Action: {ExtensionRemoveDefinitions.ReadMachineName(effect.Action)}");
            builder.AppendLine($"    Outcome: {ExtensionRemoveDefinitions.ReadMachineName(effect.Outcome)}");
            builder.AppendLine($"    Residual: {ExtensionRemoveDefinitions.ReadMachineName(effect.Residual)}");
        }

        var lifecycle = string.Join(
            " / ",
            ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Trust),
            ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Coverage),
            ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Action),
            ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Outcome));
        ExtensionPermissionPresentation.Append(builder, result.Permissions);
        builder.AppendLine($"Lifecycle: {lifecycle}");
        builder.AppendLine($"Recovery: {ExtensionRemoveDefinitions.ReadMachineName(result.Recovery.State)}");
        builder.AppendLine($"Protected paths: {Values(result.Recovery.ProtectedPaths)}");
        builder.AppendLine($"Residual path: {result.Recovery.ResidualPath ?? "none"}");
        builder.AppendLine("Verification:");
        builder.AppendLine(
            $"  Targets: {ExtensionRemoveDefinitions.ReadMachineName(result.Verification.Targets)}");
        builder.AppendLine(
            $"  Topology: {ExtensionRemoveDefinitions.ReadMachineName(result.Verification.Topology)}");
        var extensionsLifecycle = ExtensionRemoveDefinitions.ReadMachineName(
            result.Verification.ExtensionsLifecycle);
        builder.AppendLine($"  Extension lifecycle: {extensionsLifecycle}");
        builder.AppendLine(
            $"Package source unchanged: {result.PackageSourceUnchanged.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Findings: {result.Findings.Count.ToString(CultureInfo.InvariantCulture)}");
        foreach (var finding in result.Findings)
        {
            builder.AppendLine(
                $"  {ExtensionRemoveDefinitions.ReadMachineName(finding.Code)}: {finding.Cause}");
            if (finding.Target is not null)
            {
                builder.AppendLine($"    Target: {finding.Target}");
            }
        }

        builder.AppendLine($"Status: {CliStatusDefinitions.Read(result.Status).MachineName}");
        if (result.Next is { } next)
        {
            builder.AppendLine($"Next: {next.Command}");
            builder.AppendLine($"  Reason: {next.Reason}");
        }

        return builder.ToString().TrimEnd();
    }

    internal static string? RenderDiagnostic(
        CliPresentationRequest<ExtensionRemoveResult> presentation)
    {
        var status = CliStatusDefinitions.Read(presentation.Result.Status).MachineName;
        var findings = presentation.Result.Findings.Count.ToString(CultureInfo.InvariantCulture);
        return $"status={status}; findings={findings}";
    }

    private static string Values(IReadOnlyList<string> values)
        => values.Count == 0 ? "none" : string.Join(", ", values);
}
