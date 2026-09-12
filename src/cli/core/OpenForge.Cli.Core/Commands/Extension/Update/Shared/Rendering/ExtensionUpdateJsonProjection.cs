using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Rendering;

internal static class ExtensionUpdateJsonProjection
{
    internal static string RenderJson(CliPresentationRequest<ExtensionUpdateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        return JsonSerializer.Serialize(
            Create(presentation.Result),
            ExtensionUpdateJsonContext.Default.ExtensionUpdateJsonDocument);
    }

    private static ExtensionUpdateJsonDocument Create(ExtensionUpdateResult result)
        => new()
        {
            SchemaVersion = ExtensionUpdateDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null
                ? null
                : new ExtensionUpdateJsonWorkspace
                {
                    Path = result.Workspace.LexicalRoot,
                    SelectedBy = result.Workspace.SelectedBy switch
                    {
                        CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
                        CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
                        _ => throw new ArgumentOutOfRangeException(nameof(result), result.Workspace.SelectedBy, "The workspace selection method is not defined."),
                    },
                },
            Result = new ExtensionUpdateJsonResult
            {
                Mode = ExtensionUpdateDefinitions.ReadMachineName(result.Mode),
                Force = result.Force,
                Prune = result.Prune,
                Automatic = result.Automatic,
                Selection = result.Selection is null
                    ? null
                    : new ExtensionUpdateJsonSelection
                    {
                        SelectedBy = ExtensionUpdateDefinitions.ReadMachineName(result.Selection.SelectedBy),
                        RootIds = [.. result.Selection.RootIds],
                    },
                Source = result.Source is null
                    ? null
                    : new ExtensionUpdateJsonSource
                    {
                        Kind = ExtensionUpdateDefinitions.ReadMachineName(result.Source.Kind),
                        Path = result.Source.Path,
                        Identity = result.Source.Identity,
                        PackageCount = result.Source.PackageCount,
                    },
                Packages = [.. result.Packages.Select(package => new ExtensionUpdateJsonPackage
                {
                    Id = package.Id,
                    SelectedRoot = package.SelectedRoot,
                    Dependencies = [.. package.Dependencies],
                })],
                Comparisons = [.. result.Comparisons.Select(comparison => new ExtensionUpdateJsonComparison
                {
                    Path = comparison.Path,
                    PackageId = comparison.PackageId,
                    Kind = ExtensionUpdateDefinitions.ReadMachineName(comparison.Kind),
                    Region = comparison.Region,
                    SourceAssetPath = comparison.SourceAssetPath,
                    FingerprintKind = ExtensionUpdateDefinitions.ReadMachineName(comparison.FingerprintKind),
                    BaselineFingerprint = comparison.BaselineFingerprint,
                    CurrentFingerprint = comparison.CurrentFingerprint,
                    IntendedFingerprint = comparison.IntendedFingerprint,
                    CurrentState = ExtensionUpdateDefinitions.ReadMachineName(comparison.CurrentState),
                    IntendedState = ExtensionUpdateDefinitions.ReadMachineName(comparison.IntendedState),
                    RetirementEligibility = ExtensionUpdateDefinitions.ReadMachineName(
                        comparison.RetirementEligibility),
                })],
                GeneratedNavigation = result.GeneratedNavigation is null
                    ? null
                    : new ExtensionUpdateJsonNavigation
                    {
                        Regions = [.. result.GeneratedNavigation.Regions.Select(region => new ExtensionUpdateJsonRegion
                        {
                            Path = region.Path,
                            State = ExtensionUpdateDefinitions.ReadMachineName(region.State),
                        })],
                    },
                Effects = [.. result.Effects.Select(effect => new ExtensionUpdateJsonEffect
                {
                    Path = effect.Path,
                    PackageId = effect.PackageId,
                    Kind = ExtensionUpdateDefinitions.ReadMachineName(effect.Kind),
                    Action = ExtensionUpdateDefinitions.ReadMachineName(effect.Action),
                    Changes = [.. effect.Changes.Select(change => new ExtensionUpdateJsonChange
                    {
                        Kind = ExtensionUpdateDefinitions.ReadMachineName(change.Kind),
                        Action = ExtensionUpdateDefinitions.ReadMachineName(change.Action),
                        Region = change.Region,
                        SourceAssetPath = change.SourceAssetPath,
                    })],
                    Outcome = ExtensionUpdateDefinitions.ReadMachineName(effect.Outcome),
                    Residual = ExtensionUpdateDefinitions.ReadMachineName(effect.Residual),
                })],
                Permissions = WorkspacePermissionJsonProjection.Create(result.Permissions),
                Lifecycle = new ExtensionUpdateJsonLifecycle
                {
                    Trust = ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Trust),
                    Coverage = ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Coverage),
                    Action = ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Action),
                    Outcome = ExtensionUpdateDefinitions.ReadMachineName(result.Lifecycle.Outcome),
                },
                Recovery = new ExtensionUpdateJsonRecovery
                {
                    State = ExtensionUpdateDefinitions.ReadMachineName(result.Recovery.State),
                    ProtectedPaths = [.. result.Recovery.ProtectedPaths],
                    ResidualPath = result.Recovery.ResidualPath,
                },
                Verification = new ExtensionUpdateJsonVerification
                {
                    Targets = ExtensionUpdateDefinitions.ReadMachineName(result.Verification.Targets),
                    Topology = ExtensionUpdateDefinitions.ReadMachineName(result.Verification.Topology),
                    ExtensionsLifecycle = ExtensionUpdateDefinitions.ReadMachineName(
                        result.Verification.ExtensionsLifecycle),
                },
                Findings = [.. result.Findings.Select(finding => new ExtensionUpdateJsonFinding
                {
                    Code = ExtensionUpdateDefinitions.ReadMachineName(finding.Code),
                    Status = CliStatusDefinitions.Read(finding.Status).MachineName,
                    Target = finding.Target,
                    Cause = finding.Cause,
                })],
            },
            Next = result.Next is null
                ? null
                : new ExtensionUpdateJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
}
