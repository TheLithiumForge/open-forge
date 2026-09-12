using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

public sealed class RouteInitModelsTests
{
    [Fact(DisplayName = "Route Init request validates required workspace target mode and scaffold boundaries"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void RequestValidatesRequiredWorkspaceTargetAndFiniteModes()
    {
        var workspace = RouteInitRedTestData.Workspace();

        Assert.Throws<ArgumentNullException>(() =>
            new RouteInitRequest(null!, "memory/docs", RouteInitScaffold.Generic, RouteInitMode.Apply, RouteInitMetadataInput.None));
        Assert.Throws<ArgumentException>(() =>
            new RouteInitRequest(workspace, "", RouteInitScaffold.Generic, RouteInitMode.Apply, RouteInitMetadataInput.None));
        Assert.Throws<ArgumentException>(() =>
            new RouteInitRequest(workspace, "   ", RouteInitScaffold.Generic, RouteInitMode.Apply, RouteInitMetadataInput.None));
        Assert.Throws<ArgumentNullException>(() =>
            new RouteInitRequest(workspace, "memory/docs", RouteInitScaffold.Generic, RouteInitMode.Apply, null!));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RouteInitRequest(workspace, "memory/docs", (RouteInitScaffold)int.MaxValue, RouteInitMode.Apply, RouteInitMetadataInput.None));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RouteInitRequest(workspace, "memory/docs", RouteInitScaffold.Generic, (RouteInitMode)int.MaxValue, RouteInitMetadataInput.None));

        var dryRun = RouteInitRedTestData.Request(
            workspace,
            mode: RouteInitMode.DryRun,
            scaffold: RouteInitScaffold.Framework);
        Assert.True(dryRun.IsDryRun);
        Assert.Equal(RouteInitScaffold.Framework, dryRun.Scaffold);
        Assert.Equal(RouteInitMode.DryRun, dryRun.Mode);
    }

    [Fact(DisplayName = "Route Init metadata snapshots ordered tags and rejects null members"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void MetadataSnapshotsOrderedTagsAndRejectsNullMembers()
    {
        var tags = new List<string> { "First", "Second" };
        var metadata = new RouteInitMetadataInput(
            "Description",
            responsibilitySpecified: true,
            responsibility: "Responsibility",
            tags);
        tags[0] = "Mutated";

        Assert.Equal("Description", metadata.Description);
        Assert.True(metadata.ResponsibilitySpecified);
        Assert.Equal("Responsibility", metadata.Responsibility);
        Assert.Equal(["First", "Second"], metadata.Tags);
        Assert.Throws<ArgumentNullException>(() =>
            new RouteInitMetadataInput(null, false, null, null!));
        Assert.Throws<ArgumentException>(() =>
            new RouteInitMetadataInput(null, false, null, new[] { "valid", (string)null! }));
    }

    [Fact(DisplayName = "Route Init formation snapshots immutable ordered collections and keeps nullable coordinates"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void FormationSnapshotsImmutableCollectionsAndNullableCoordinates()
    {
        var target = new RouteInitTarget(
            "memory/project-alpha/documents",
            "memory/project-alpha/documents",
            null);
        var entrypoints = new List<RouteInitEntrypoint>
        {
            new(
                "memory/project-alpha",
                "/workspace/.agents/memory/project-alpha/_project-alpha.md",
                RouteInitEntrypointForm.Canonical,
                RouteInitEntrypointCurrent.Existing,
                RouteInitEntrypointOwnership.User,
                null,
                null,
                RouteInitEntrypointOutcome.Unchanged),
        };
        var effects = new List<RouteInitEffect>
        {
            new(
                "/workspace/.agents/memory/project-alpha",
                RouteInitEffectKind.Directory,
                RouteInitEffectAction.Create,
                null,
                null,
                RouteInitEffectOutcome.Planned,
                RouteInitEffectResidual.None),
        };
        var unchanged = new List<string> { "/workspace/.agents/loader.md" };
        var findings = new List<RouteInitFinding>
        {
            RouteInitRedTestData.Finding(RouteInitFindingCode.InspectionIncomplete),
        };
        var formation = new RouteInitResultFormation(
            workspace: null,
            RouteInitMode.DryRun,
            RouteInitScaffold.Generic,
            target,
            new RouteInitPlanFacts(
                RouteInitPlanCompleteness.Incomplete,
                RouteInitPlanSafety.NotEstablished),
            framework: null,
            entrypoints,
            effects,
            unchanged,
            new RouteInitLifecycle(
                RouteInitLifecycleAction.None,
                RouteInitLifecycleOutcome.NotRequested),
            new RouteInitRecovery(RouteInitRecoveryState.NotRequired, null),
            RouteInitVerificationState.NotRequested,
            findings);

        entrypoints.Clear();
        effects.Clear();
        unchanged.Add("/workspace/.agents/changed.md");
        findings.Clear();

        Assert.False(formation.Entrypoints.IsDefault);
        Assert.False(formation.Effects.IsDefault);
        Assert.False(formation.UnchangedPaths.IsDefault);
        Assert.False(formation.Findings.IsDefault);
        Assert.Single(formation.Entrypoints);
        Assert.Single(formation.Effects);
        Assert.Single(formation.UnchangedPaths);
        Assert.Single(formation.Findings);
        Assert.Null(formation.Workspace);
        Assert.Null(formation.Target.Path);
        Assert.Equal(RouteInitPlanCompleteness.Incomplete, formation.Plan.Completeness);
    }

    [Fact(DisplayName = "Route Init plan copies effects and directories and exposes only a true no-op as no-op"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void PlanCopiesEffectsAndDirectoriesAndExposesOnlyTrueNoOp()
    {
        var request = RouteInitRedTestData.Request();
        var formation = RouteInitRedTestData.Formation();
        var directories = new List<OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories.PlannedDirectoryCreation>
        {
            RouteInitRedTestData.Directory(
                Path.Combine(request.Workspace.LexicalRoot, ".agents")),
        };
        var changes = new List<OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files.PlannedFileChange>
        {
            RouteInitRedTestData.CreateFile(
                Path.Combine(request.Workspace.LexicalRoot, ".agents", "memory", "_memory.md"),
                "created"u8),
        };
        var recovery = new List<RecoveryBundleTarget>();
        var plan = new RouteInitPlan(
            request,
            formation,
            directories,
            changes,
            recovery,
            intendedLifecycle: null);
        directories.Clear();
        changes.Clear();

        Assert.False(plan.DirectoryCreations.IsDefault);
        Assert.False(plan.FileChanges.IsDefault);
        Assert.False(plan.RecoveryTargets.IsDefault);
        Assert.Single(plan.DirectoryCreations);
        Assert.Single(plan.FileChanges);
        Assert.False(plan.IsNoOp);

        var noOp = new RouteInitPlan(
            request,
            formation,
            [],
            [],
            [],
            intendedLifecycle: null);
        Assert.True(noOp.IsNoOp);
    }

    [Fact(DisplayName = "Route Init finding bounds causes while retaining code status and target"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void FindingBoundsCauseAndRetainsIdentity()
    {
        var longCause = new string('x', 512);
        var finding = new RouteInitFinding(
            RouteInitFindingCode.WriteFailed,
            longCause,
            "memory/project-alpha/documents");

        Assert.Equal(RouteInitFindingCode.WriteFailed, finding.Code);
        Assert.Equal(CliSemanticStatus.Failed, finding.Status);
        Assert.Equal("memory/project-alpha/documents", finding.Target);
        Assert.Equal(256, finding.Cause.Length);
        Assert.Throws<ArgumentException>(() =>
            new RouteInitFinding(RouteInitFindingCode.WriteFailed, "", "memory/docs"));
        Assert.Throws<ArgumentException>(() =>
            new RouteInitFinding(RouteInitFindingCode.WriteFailed, "cause", ""));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RouteInitFinding((RouteInitFindingCode)int.MaxValue, "cause", null));
    }

    [Fact(DisplayName = "Route Init result builder derives one ordered status and next action from the typed formation"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void ResultBuilderDerivesOrderedStatusAndNextActionFromFormation()
    {
        var findings = new[]
        {
            RouteInitRedTestData.Finding(RouteInitFindingCode.NeedsAuthoring),
            RouteInitRedTestData.Finding(RouteInitFindingCode.WriteFailed),
        };
        var result = new RouteInitResultBuilder().Build(
            RouteInitRedTestData.Formation(findings: findings));

        Assert.Equal("route init", result.Command);
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(
            [RouteInitFindingCode.NeedsAuthoring, RouteInitFindingCode.WriteFailed],
            result.Findings.Select(finding => finding.Code));
        Assert.Equal("open-forge route init --verbose", result.Next?.Command);
    }

    [Fact(DisplayName = "Route Init result builder retains bounded lifecycle recovery and change facts"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void ResultBuilderRetainsBoundedLifecycleRecoveryAndChangeFacts()
    {
        var targetPath = Path.Combine(
            RouteInitRedTestData.Workspace().LexicalRoot,
            ".agents",
            "memory",
            "project-alpha",
            "documents",
            "_documents.md");
        var effect = new RouteInitEffect(
            targetPath,
            RouteInitEffectKind.GeneratedRegion,
            RouteInitEffectAction.Replace,
            null,
            new RouteInitEffectChange("before", "expected-generated-interior"),
            RouteInitEffectOutcome.Verified,
            RouteInitEffectResidual.None);
        var formation = RouteInitRedTestData.Formation(
            effects: [effect],
            lifecycle: new RouteInitLifecycle(
                RouteInitLifecycleAction.Publish,
                RouteInitLifecycleOutcome.Verified),
            recovery: new RouteInitRecovery(
                RouteInitRecoveryState.NotRequired,
                null),
            verification: RouteInitVerificationState.Verified);

        var result = new RouteInitResultBuilder().Build(formation);

        Assert.Equal(RouteInitLifecycleAction.Publish, result.Lifecycle.Action);
        Assert.Equal(RouteInitLifecycleOutcome.Verified, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitEffectKind.GeneratedRegion, Assert.Single(result.Effects).Kind);
        Assert.Equal("before", Assert.Single(result.Effects).Change?.Before);
        Assert.Equal("expected-generated-interior", Assert.Single(result.Effects).Change?.Expected);
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
    }
}
