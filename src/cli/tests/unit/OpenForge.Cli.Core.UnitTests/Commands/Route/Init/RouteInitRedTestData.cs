using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

internal static class RouteInitRedTestData
{
    internal static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), "open-forge-route-init-red"));
        return new CliWorkspace(
            root,
            root,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    internal static CliInvocation Invocation(
        CliWorkspace workspace,
        CliFormat format = CliFormat.Json,
        CliDetail view = CliDetail.Standard,
        CliDetail? diagnosticDetail = null)
    {
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(format, diagnosticDetail ?? view, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }

    internal static RouteInitRequest Request(
        CliWorkspace? workspace = null,
        string routeTarget = "memory/project-alpha/documents",
        RouteInitScaffold scaffold = RouteInitScaffold.Generic,
        RouteInitMode mode = RouteInitMode.Apply,
        RouteInitMetadataInput? metadata = null)
    {
        return new RouteInitRequest(
            workspace ?? Workspace(),
            routeTarget,
            scaffold,
            mode,
            metadata ?? RouteInitMetadataInput.None);
    }

    internal static RouteInitMetadataInput Metadata(
        string? description = null,
        bool responsibilitySpecified = false,
        string? responsibility = null,
        IEnumerable<string>? tags = null)
    {
        return new RouteInitMetadataInput(
            description,
            responsibilitySpecified,
            responsibility,
            tags ?? []);
    }

    internal static RouteInitFinding Finding(
        RouteInitFindingCode code,
        string target = "memory/project-alpha/documents",
        string cause = "A bounded Route Init finding.")
    {
        return new RouteInitFinding(code, cause, target);
    }

    internal static RouteInitResultFormation Formation(
        CliWorkspace? workspace = null,
        RouteInitMode mode = RouteInitMode.Apply,
        RouteInitScaffold scaffold = RouteInitScaffold.Generic,
        RouteInitTarget? target = null,
        RouteInitPlanFacts? plan = null,
        RouteInitFramework? framework = null,
        IEnumerable<RouteInitEntrypoint>? entrypoints = null,
        IEnumerable<RouteInitEffect>? effects = null,
        IEnumerable<string>? unchangedPaths = null,
        RouteInitLifecycle? lifecycle = null,
        RouteInitRecovery? recovery = null,
        RouteInitVerificationState verification = RouteInitVerificationState.Verified,
        IEnumerable<RouteInitFinding>? findings = null)
    {
        var selectedWorkspace = workspace ?? Workspace();
        var selectedTarget = target ?? new RouteInitTarget(
            "memory/project-alpha/documents",
            "memory/project-alpha/documents",
            Path.Combine(
                selectedWorkspace.LexicalRoot,
                ".agents",
                "memory",
                "project-alpha",
                "documents",
                "_documents.md"));
        var selectedEntrypoints = entrypoints ??
        [
            new RouteInitEntrypoint(
                "memory/project-alpha/documents",
                selectedTarget.Path ?? "",
                RouteInitEntrypointForm.Canonical,
                RouteInitEntrypointCurrent.Missing,
                RouteInitEntrypointOwnership.User,
                new RouteInitMetadata(
                    "Draft route for memory/project-alpha/documents",
                    RouteInitDescriptionSource.Draft,
                    null,
                    RouteInitResponsibilitySource.DefaultOmitted,
                    ["NeedsAuthoring"],
                    RouteInitTagsSource.Draft),
                null,
                RouteInitEntrypointOutcome.Planned),
        ];
        var selectedEffects = effects ??
        [
            new RouteInitEffect(
                selectedTarget.Path ?? "",
                RouteInitEffectKind.Entrypoint,
                RouteInitEffectAction.Create,
                null,
                new RouteInitEffectChange(null, "draft-entrypoint-bytes"),
                RouteInitEffectOutcome.Planned,
                RouteInitEffectResidual.None),
        ];
        return new RouteInitResultFormation(
            selectedWorkspace,
            mode,
            scaffold,
            selectedTarget,
            plan ?? new RouteInitPlanFacts(
                RouteInitPlanCompleteness.Complete,
                RouteInitPlanSafety.Safe),
            framework,
            selectedEntrypoints,
            selectedEffects,
            unchangedPaths ?? [".agents/loader.md"],
            lifecycle ?? new RouteInitLifecycle(
                RouteInitLifecycleAction.None,
                RouteInitLifecycleOutcome.NotRequested),
            recovery ?? new RouteInitRecovery(
                RouteInitRecoveryState.NotRequired,
                null),
            verification,
            findings ?? []);
    }

    internal static RouteInitResult Result(
        RouteInitResultFormation? formation = null,
        IEnumerable<RouteInitFinding>? orderedFindings = null,
        CliSemanticStatus status = CliSemanticStatus.Complete,
        CliNextAction? next = null)
    {
        return new RouteInitResult(
            formation ?? Formation(),
            (orderedFindings ?? []).ToImmutableArray(),
            status,
            next);
    }

    internal static PlannedDirectoryCreation Directory(string logicalPath)
    {
        return PlannedDirectoryCreation.Create(FileExpectation.Missing(logicalPath));
    }

    internal static PlannedFileChange CreateFile(string logicalPath, ReadOnlySpan<byte> bytes)
    {
        return PlannedFileChange.Create(FileExpectation.Missing(logicalPath), bytes);
    }

    internal static RecoveryBundleTarget RecoveryTarget(PlannedFileChange change, byte[] beforeBytes)
    {
        var before = FileStateSnapshot.File(
            change.LogicalPath,
            change.Expectation.PhysicalPath ?? change.LogicalPath,
            beforeBytes);
        return RecoveryBundleTarget.Create(change, before);
    }
}
