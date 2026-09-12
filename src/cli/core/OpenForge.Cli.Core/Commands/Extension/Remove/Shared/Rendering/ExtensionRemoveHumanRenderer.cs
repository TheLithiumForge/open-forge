using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Rendering;

internal static class ExtensionRemoveHumanRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionRemoveResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var operation = result.Mode == ExtensionRemoveMode.DryRun ? "Extension removal preview" : "Extension removal";
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, CliHumanText.Outcome(operation, result.Status));
        builder.AppendLine($"""
            Mode: {ExtensionRemoveDefinitions.ReadMachineName(result.Mode)}
            Prune: {result.Prune.ToString().ToLowerInvariant()}
            Automatic: {result.Automatic.ToString().ToLowerInvariant()}
            Selected packages: {(result.Selection is { } selection ? ExtensionHumanText.Values(selection.Ids) : "unavailable")}
            """);
        var selectionMethod = result.Selection is { } selected ? ExtensionRemoveDefinitions.ReadMachineName(selected.SelectedBy) : "unavailable";
        builder.AppendLine($"Package selection: {selectionMethod}");
        foreach (var finding in result.Findings)
        {
            ExtensionHumanText.AppendFinding(builder, finding.Status, ExtensionRemoveDefinitions.ReadMachineName(finding.Code), finding.Cause, finding.Target);
        }

        AppendDependencies(builder, presentation);
        ExtensionPermissionPresentation.Append(builder, result.Permissions, presentation.Presentation.View);
        ExtensionRemovePathsHumanRenderer.Append(builder, presentation);
        builder.AppendLine($"""
            Installation record: {ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Action)}; {ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Outcome)}
              Observed before change: {ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Trust)}; coverage {ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Coverage)}
            Recovery: {ExtensionRemoveDefinitions.ReadMachineName(result.Recovery.State)}
              Protected paths: {ExtensionHumanText.Values(result.Recovery.ProtectedPaths)}
              Remaining recovery data: {ExtensionHumanText.Value(result.Recovery.ResidualPath ?? "none")}
            Verification: targets {ExtensionRemoveDefinitions.ReadMachineName(result.Verification.Targets)}; topology {ExtensionRemoveDefinitions.ReadMachineName(result.Verification.Topology)}; Extension record {ExtensionRemoveDefinitions.ReadMachineName(result.Verification.ExtensionsLifecycle)}
            Package source unchanged: {result.PackageSourceUnchanged.ToString().ToLowerInvariant()}
            """);
        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
    }

    private static void AppendDependencies(StringBuilder builder, CliPresentationRequest<ExtensionRemoveResult> presentation)
    {
        if (presentation.Result.Dependencies is not { } dependencies)
        {
            builder.AppendLine("Dependencies: unavailable");
            return;
        }

        foreach (var blocker in dependencies.RetainedDependentBlockers)
        {
            builder.AppendLine($"Cannot remove {ExtensionHumanText.Value(blocker.DependencyId)}: still required by {ExtensionHumanText.Values(blocker.RetainedDependentIds)}");
        }

        builder.AppendLine($"""
            Removal order: {ExtensionHumanText.Values(dependencies.RemovalOrder)}
            Unused dependencies left installed: {ExtensionHumanText.Values(dependencies.RetainedOrphanDependencyIds)}
            """);
        if (presentation.Presentation.View == CliView.Expanded)
        {
            foreach (var package in dependencies.Packages)
            {
                builder.AppendLine($"  {ExtensionHumanText.Value(package.Id)}: {(package.SelectedForRemoval ? "selected for removal" : "retained")}; requires {ExtensionHumanText.Values(package.Dependencies)}");
            }
        }
    }
}
