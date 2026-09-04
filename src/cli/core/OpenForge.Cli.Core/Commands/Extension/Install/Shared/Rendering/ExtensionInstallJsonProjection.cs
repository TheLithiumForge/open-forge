using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Rendering;

internal static class ExtensionInstallJsonProjection
{
    internal static string RenderJson(CliPresentationRequest<ExtensionInstallResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        return JsonSerializer.Serialize(
            Create(presentation.Result),
            ExtensionInstallJsonContext.Default.ExtensionInstallJsonDocument);
    }

    private static ExtensionInstallJsonDocument Create(ExtensionInstallResult result)
        => new()
        {
            SchemaVersion = ExtensionInstallDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null
                ? null
                : new ExtensionInstallJsonWorkspace
                {
                    Path = result.Workspace.LexicalRoot,
                    SelectedBy = result.Workspace.SelectedBy switch
                    {
                        CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
                        CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
                        _ => throw new ArgumentOutOfRangeException(),
                    },
                },
            Result = new ExtensionInstallJsonResult
            {
                Mode = ExtensionInstallDefinitions.ReadMachineName(result.Mode),
                Force = result.Force,
                Automatic = result.Automatic,
                Selection = result.Selection is null ? null : new ExtensionInstallJsonSelection
                {
                    SelectedBy = ExtensionInstallDefinitions.ReadMachineName(result.Selection.SelectedBy),
                    RootIds = result.Selection.RootIds.ToArray(),
                },
                Source = result.Source is null ? null : new ExtensionInstallJsonSource
                {
                    Kind = ExtensionInstallDefinitions.ReadMachineName(result.Source.Kind),
                    Path = result.Source.Path,
                    Identity = result.Source.Identity,
                    PackageCount = result.Source.PackageCount,
                },
                Packages = result.Packages.Select(package => new ExtensionInstallJsonPackage
                {
                    Id = package.Id,
                    SelectedRoot = package.SelectedRoot,
                    Dependencies = package.Dependencies.ToArray(),
                }).ToArray(),
                Framework = result.Framework is null ? null : new ExtensionInstallJsonFramework
                {
                    InventoryFingerprint = result.Framework.InventoryFingerprint,
                    TargetCount = result.Framework.TargetCount,
                    GeneratedRegionCount = result.Framework.GeneratedRegionCount,
                },
                Footprint = result.Footprint is null ? null : new ExtensionInstallJsonFootprint
                {
                    PackageCount = result.Footprint.PackageCount,
                    PayloadTargets = result.Footprint.PayloadTargets.ToArray(),
                    GeneratedRegions = result.Footprint.GeneratedRegions.ToArray(),
                    Directories = result.Footprint.Directories.ToArray(),
                },
                Effects = result.Effects.Select(effect => new ExtensionInstallJsonEffect
                {
                    Path = effect.Path,
                    PackageId = effect.PackageId,
                    Kind = ExtensionInstallDefinitions.ReadMachineName(effect.Kind),
                    Action = ExtensionInstallDefinitions.ReadMachineName(effect.Action),
                    Outcome = ExtensionInstallDefinitions.ReadMachineName(effect.Outcome),
                    Residual = ExtensionInstallDefinitions.ReadMachineName(effect.Residual),
                }).ToArray(),
                GeneratedNavigation = result.GeneratedNavigation is null
                    ? null
                    : new ExtensionInstallJsonNavigation
                    {
                        Regions = result.GeneratedNavigation.Regions.Select(region => new ExtensionInstallJsonRegion
                        {
                            Path = region.Path,
                            State = ExtensionInstallDefinitions.ReadMachineName(region.State),
                        }).ToArray(),
                    },
                Lifecycle = new ExtensionInstallJsonLifecycle
                {
                    Action = ExtensionInstallDefinitions.ReadMachineName(result.Lifecycle.Action),
                    Outcome = ExtensionInstallDefinitions.ReadMachineName(result.Lifecycle.Outcome),
                },
                Recovery = new ExtensionInstallJsonRecovery
                {
                    State = ExtensionInstallDefinitions.ReadMachineName(result.Recovery.State),
                    ProtectedPaths = result.Recovery.ProtectedPaths.ToArray(),
                    ResidualPath = result.Recovery.ResidualPath,
                },
                Verification = new ExtensionInstallJsonVerification
                {
                    Targets = ExtensionInstallDefinitions.ReadMachineName(result.Verification.Targets),
                    Topology = ExtensionInstallDefinitions.ReadMachineName(result.Verification.Topology),
                    ExtensionsLifecycle = ExtensionInstallDefinitions.ReadMachineName(
                        result.Verification.ExtensionsLifecycle),
                    FrameworkLifecycle = ExtensionInstallDefinitions.ReadMachineName(
                        result.Verification.FrameworkLifecycle),
                },
                Findings = result.Findings.Select(finding => new ExtensionInstallJsonFinding
                {
                    Code = ExtensionInstallDefinitions.ReadMachineName(finding.Code),
                    Status = CliStatusDefinitions.Read(finding.Status).MachineName,
                    Target = finding.Target,
                    Cause = finding.Cause,
                }).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new ExtensionInstallJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ExtensionInstallJsonDocument))]
internal sealed partial class ExtensionInstallJsonContext : JsonSerializerContext;
