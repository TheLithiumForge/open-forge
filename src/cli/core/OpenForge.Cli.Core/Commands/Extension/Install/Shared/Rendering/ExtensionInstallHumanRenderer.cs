using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Rendering;

internal static class ExtensionInstallHumanRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionInstallResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var mode = ExtensionInstallDefinitions.ReadMachineName(result.Mode);
        var operation = result.Mode == ExtensionInstallMode.DryRun ? "Extension installation preview" : "Extension installation";
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, CliHumanText.Outcome(operation, result.Status));
        builder.AppendLine($"""
            Mode: {mode}
            Force: {result.Force.ToString().ToLowerInvariant()}
            Automatic: {result.Automatic.ToString().ToLowerInvariant()}
            Selected packages: {(result.Selection is { } selection ? ExtensionHumanText.Values(selection.RootIds) : "unavailable")}
            """);
        var selectionMethod = result.Selection is { } selected ? ExtensionInstallDefinitions.ReadMachineName(selected.SelectedBy) : "unavailable";
        builder.AppendLine($"Package selection: {selectionMethod}");
        foreach (var finding in result.Findings)
        {
            ExtensionHumanText.AppendFinding(builder, finding.Status, ExtensionInstallDefinitions.ReadMachineName(finding.Code), finding.Cause, finding.Target);
        }

        if (result.Source is { } source)
        {
            builder.AppendLine($"Source: {Value(source.Identity)}; {ExtensionInstallDefinitions.ReadMachineName(source.Kind)}");
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
        ExtensionInstallPathsHumanRenderer.Append(builder, presentation);
        builder.AppendLine($"""
            Installation record: {ExtensionInstallDefinitions.ReadMachineName(result.Lifecycle.Action)}; {ExtensionInstallDefinitions.ReadMachineName(result.Lifecycle.Outcome)}
            Recovery: {ExtensionInstallDefinitions.ReadMachineName(result.Recovery.State)}
              Protected paths: {ExtensionHumanText.Values(result.Recovery.ProtectedPaths)}
              Remaining recovery data: {Value(result.Recovery.ResidualPath ?? "none")}
            Verification: targets {ExtensionInstallDefinitions.ReadMachineName(result.Verification.Targets)}; topology {ExtensionInstallDefinitions.ReadMachineName(result.Verification.Topology)}; Extension record {ExtensionInstallDefinitions.ReadMachineName(result.Verification.ExtensionsLifecycle)}; Framework record {ExtensionInstallDefinitions.ReadMachineName(result.Verification.FrameworkLifecycle)}
            """);
        if (expanded)
        {
            if (result.Framework is { } framework)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"Framework: fingerprint {Value(framework.InventoryFingerprint)}; {framework.TargetCount} targets; {framework.GeneratedRegionCount} generated regions");
            }
            else
            {
                builder.AppendLine("Framework: unavailable");
            }
        }

        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
    }

    private static string Value(string? value) => ExtensionHumanText.Value(value);
}
