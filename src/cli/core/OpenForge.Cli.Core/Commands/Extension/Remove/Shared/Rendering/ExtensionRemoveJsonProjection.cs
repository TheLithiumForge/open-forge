using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Rendering;

internal static class ExtensionRemoveJsonProjection
{
    internal static string RenderJson(CliPresentationRequest<ExtensionRemoveResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        return JsonSerializer.Serialize(
            Create(presentation.Result),
            ExtensionRemoveJsonContext.Default.ExtensionRemoveJsonDocument);
    }

    private static ExtensionRemoveJsonDocument Create(ExtensionRemoveResult result)
        => new()
        {
            SchemaVersion = ExtensionRemoveDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null
                ? null
                : new ExtensionRemoveJsonWorkspace
                {
                    Path = result.Workspace.LexicalRoot,
                    SelectedBy = result.Workspace.SelectedBy switch
                    {
                        CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
                        CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
                        _ => throw new ArgumentOutOfRangeException(nameof(result), result.Workspace.SelectedBy, "The workspace selection method is not defined."),
                    },
                },
            Result = new ExtensionRemoveJsonResult
            {
                Mode = ExtensionRemoveDefinitions.ReadMachineName(result.Mode),
                Prune = result.Prune,
                Automatic = result.Automatic,
                Selection = result.Selection is null
                    ? null
                    : new ExtensionRemoveJsonSelection
                    {
                        SelectedBy = ExtensionRemoveDefinitions.ReadMachineName(result.Selection.SelectedBy),
                        Ids = [.. result.Selection.Ids],
                    },
                Dependencies = result.Dependencies is null
                    ? null
                    : new ExtensionRemoveJsonDependencyPlan
                    {
                        Packages = [.. result.Dependencies.Packages.Select(package => new ExtensionRemoveJsonPackage
                        {
                            Id = package.Id,
                            SelectedForRemoval = package.SelectedForRemoval,
                            Dependencies = [.. package.Dependencies],
                        })],
                        RemovalOrder = [.. result.Dependencies.RemovalOrder],
                        RetainedDependentBlockers = [.. result.Dependencies.RetainedDependentBlockers
                            .Select(blocker => new ExtensionRemoveJsonRetainedDependentBlocker
                            {
                                DependencyId = blocker.DependencyId,
                                RetainedDependentIds = [.. blocker.RetainedDependentIds],
                            })],
                        RetainedOrphanDependencyIds = [.. result.Dependencies.RetainedOrphanDependencyIds],
                    },
                Paths = [.. result.Paths.Select(path => new ExtensionRemoveJsonPath
                {
                    Path = path.Path,
                    Classification = ExtensionRemoveDefinitions.ReadMachineName(path.Classification),
                    SelectedOwnerIds = [.. path.SelectedOwnerIds],
                    RemainingOwnerIds = [.. path.RemainingOwnerIds],
                    Action = ExtensionRemoveDefinitions.ReadMachineName(path.Action),
                })],
                GeneratedNavigation = result.GeneratedNavigation is null
                    ? null
                    : new ExtensionRemoveJsonNavigation
                    {
                        Regions = [.. result.GeneratedNavigation.Regions.Select(region => new ExtensionRemoveJsonRegion
                        {
                            Path = region.Path,
                            State = ExtensionRemoveDefinitions.ReadMachineName(region.State),
                        })],
                    },
                Effects = [.. result.Effects.Select(effect => new ExtensionRemoveJsonEffect
                {
                    Path = effect.Path,
                    PackageId = effect.PackageId,
                    Kind = ExtensionRemoveDefinitions.ReadMachineName(effect.Kind),
                    Action = ExtensionRemoveDefinitions.ReadMachineName(effect.Action),
                    Outcome = ExtensionRemoveDefinitions.ReadMachineName(effect.Outcome),
                    Residual = ExtensionRemoveDefinitions.ReadMachineName(effect.Residual),
                })],
                Permissions = WorkspacePermissionJsonProjection.Create(result.Permissions),
                Lifecycle = new ExtensionRemoveJsonLifecycle
                {
                    Trust = ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Trust),
                    Coverage = ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Coverage),
                    Action = ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Action),
                    Outcome = ExtensionRemoveDefinitions.ReadMachineName(result.Lifecycle.Outcome),
                },
                Recovery = new ExtensionRemoveJsonRecovery
                {
                    State = ExtensionRemoveDefinitions.ReadMachineName(result.Recovery.State),
                    ProtectedPaths = [.. result.Recovery.ProtectedPaths],
                    ResidualPath = result.Recovery.ResidualPath,
                },
                Verification = new ExtensionRemoveJsonVerification
                {
                    Targets = ExtensionRemoveDefinitions.ReadMachineName(result.Verification.Targets),
                    Topology = ExtensionRemoveDefinitions.ReadMachineName(result.Verification.Topology),
                    ExtensionsLifecycle = ExtensionRemoveDefinitions.ReadMachineName(
                        result.Verification.ExtensionsLifecycle),
                },
                PackageSourceUnchanged = result.PackageSourceUnchanged,
                Findings = [.. result.Findings.Select(finding => new ExtensionRemoveJsonFinding
                {
                    Code = ExtensionRemoveDefinitions.ReadMachineName(finding.Code),
                    Status = CliStatusDefinitions.Read(finding.Status).MachineName,
                    Target = finding.Target,
                    Cause = finding.Cause,
                })],
            },
            Next = result.Next is null
                ? null
                : new ExtensionRemoveJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
}
