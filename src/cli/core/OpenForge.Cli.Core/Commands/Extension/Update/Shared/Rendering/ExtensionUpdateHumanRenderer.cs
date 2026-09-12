using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Rendering;

internal static class ExtensionUpdateHumanRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionUpdateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var operation = result.Mode == ExtensionUpdateMode.DryRun ? "Extension update preview" : "Extension update";
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, CliHumanText.Outcome(operation, result.Status));
        builder.AppendLine($"""
            Mode: {ExtensionUpdateDefinitions.ReadMachineName(result.Mode)}
            Force: {result.Force.ToString().ToLowerInvariant()}
            Prune: {result.Prune.ToString().ToLowerInvariant()}
            Automatic: {result.Automatic.ToString().ToLowerInvariant()}
            Selected packages: {(result.Selection is { } selection ? ExtensionHumanText.Values(selection.RootIds) : "unavailable")}
            """);
        var selectionMethod = result.Selection is { } selected ? ExtensionUpdateDefinitions.ReadMachineName(selected.SelectedBy) : "unavailable";
        builder.AppendLine($"Package selection: {selectionMethod}");
        foreach (var finding in result.Findings)
        {
            ExtensionHumanText.AppendFinding(builder, finding.Status, ExtensionUpdateDefinitions.ReadMachineName(finding.Code), finding.Cause, finding.Target);
        }

        if (result.Source is { } source)
        {
            builder.AppendLine($"Source: {Value(source.Identity)}; {ExtensionUpdateDefinitions.ReadMachineName(source.Kind)}");
            if (expanded)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"  Path: {Value(source.Path ?? "embedded")}; packages: {source.PackageCount}");
            }
        }
        else
        {
            builder.AppendLine("Source: unavailable");
        }

        builder.AppendLine("Packages in dependency order:");
        foreach (var package in result.Packages)
        {
            builder.AppendLine($"  {Value(package.Id)}: {(package.SelectedRoot ? "selected" : "required dependency")}; requires {ExtensionHumanText.Values(package.Dependencies)}");
        }

        ExtensionPermissionPresentation.Append(builder, result.Permissions, presentation.Presentation.View);
        ExtensionUpdatePathsHumanRenderer.Append(builder, presentation);
        builder.AppendLine($"""
            Installation record: {ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Action)}; {ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Outcome)}
              Observed before change: {ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Trust)}; coverage {ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Coverage)}
            Recovery: {ExtensionUpdateDefinitions.ReadMachineName(result.Recovery.State)}
              Protected paths: {ExtensionHumanText.Values(result.Recovery.ProtectedPaths)}
              Remaining recovery data: {Value(result.Recovery.ResidualPath ?? "none")}
            Verification: targets {ExtensionUpdateDefinitions.ReadMachineName(result.Verification.Targets)}; topology {ExtensionUpdateDefinitions.ReadMachineName(result.Verification.Topology)}; Extension record {ExtensionUpdateDefinitions.ReadMachineName(result.Verification.ExtensionsLifecycle)}
            """);
        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
    }

    private static string Value(string? value) => ExtensionHumanText.Value(value);
}
