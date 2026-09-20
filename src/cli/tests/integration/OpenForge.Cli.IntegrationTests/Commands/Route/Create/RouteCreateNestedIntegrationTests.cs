using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

public sealed class RouteCreateNestedIntegrationTests
{
    private const string TeamPath = ".agents/memory/project-alpha/team";
    private const string TopicPath = TeamPath + "/topic";
    private const string TeamEntrypoint = TeamPath + "/_team.md";
    private const string TopicEntrypoint = TopicPath + "/_topic.md";
    private const string LeafPath = TopicPath + "/notes.md";

    [Theory, InlineData(false), InlineData(true)]
    [Trait("Boundary", "OS"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task NestedCreationOrdersEffectsPreservesAuthoredBytesAndConverges(bool reuseDirectory)
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-nested-order");
        workspace.SeedBase();
        if (reuseDirectory) workspace.SeedNestedTeamDirectory();
        var before = workspace.SnapshotHashes();
        var parentBefore = workspace.ReadText(RouteCreateIntegrationWorkspace.ParentPath);
        var request = Request(workspace);
        var build = await new RouteCreatePlanBuilder().BuildAsync(request, TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteCreatePlan>(build.Plan);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Absolute(TopicPath)));
        string[] expectedPaths = reuseDirectory
            ? [TopicPath, TeamEntrypoint, TopicEntrypoint, LeafPath, RouteCreateIntegrationWorkspace.ParentPath]
            : [TeamPath, TopicPath, TeamEntrypoint, TopicEntrypoint, LeafPath, RouteCreateIntegrationWorkspace.ParentPath];
        Assert.Equal(expectedPaths, build.Formation.Effects.Select(effect => effect.Path));
        Assert.Equal(reuseDirectory ? 1 : 2, plan.DirectoryCreations.Length);
        Assert.Single(plan.RecoveryTargets);
        workspace.OwnNestedApplicationPlan(plan);

        var applied = await new RouteCreateApplicationOperation(workspace.LockStoreRoot)
            .ExecuteAsync(plan, TestContext.Current.CancellationToken);
        if (applied.Recovery.ResidualPath is { } residual) workspace.TrackRecoveryPath(residual);
        Assert.Equal(RouteCreateVerificationState.Verified, applied.Verification);
        Assert.Empty(applied.Findings);
        Assert.Equal(expectedPaths, applied.Effects.Select(effect => effect.Path));
        Assert.All(applied.Effects, effect =>
        {
            Assert.Equal(RouteCreateEffectOutcome.Verified, effect.Outcome);
            Assert.Equal(RouteCreateEffectResidual.None, effect.Residual);
        });
        foreach (var change in plan.FileChanges)
            Assert.Equal(change.IntendedBytes.ToArray(), File.ReadAllBytes(change.LogicalPath));

        const string leafExpected = "---\nopen-forge:\n  description: Nested note\n  tags: [Docs]\n---\n";
        Assert.Equal(Encoding.UTF8.GetBytes(leafExpected), File.ReadAllBytes(workspace.Absolute(LeafPath)));
        foreach (var entrypoint in new[] { TeamEntrypoint, TopicEntrypoint })
        {
            var text = workspace.ReadText(entrypoint);
            Assert.StartsWith("---\nopen-forge: {}\n---\n", text, StringComparison.Ordinal);
            Assert.DoesNotContain("description:", text, StringComparison.Ordinal);
            Assert.DoesNotContain("tags:", text, StringComparison.Ordinal);
            Assert.Contains("## Axioms", text, StringComparison.Ordinal);
        }
        Assert.Contains("# memory/project-alpha/team\n", workspace.ReadText(TeamEntrypoint), StringComparison.Ordinal);
        Assert.Contains("# memory/project-alpha/team/topic\n", workspace.ReadText(TopicEntrypoint), StringComparison.Ordinal);
        var parentAfter = workspace.ReadText(RouteCreateIntegrationWorkspace.ParentPath);
        var entriesOffset = parentBefore.IndexOf("## Entries", StringComparison.Ordinal);
        Assert.True(entriesOffset >= 0);
        Assert.Equal(parentBefore[..entriesOffset], parentAfter[..entriesOffset]);
        var after = workspace.SnapshotHashes();
        var repeated = await new RouteCreatePlanBuilder().BuildAsync(request, TestContext.Current.CancellationToken);
        Assert.True(Assert.IsType<RouteCreatePlan>(repeated.Plan).IsNoOp);
        Assert.Empty(repeated.Formation.Effects);
        Assert.Equal(after, workspace.SnapshotHashes());
    }

    [Theory, InlineData("memory/unknown/note"), InlineData(".agents/root-note.md")]
    [Trait("Boundary", "OS"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task UnknownOrRootLevelTargetsAreInvalidWithoutWrites(string target)
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-unrecognized-root");
        workspace.SeedBase();
        var before = workspace.SnapshotHashes();
        var build = await new RouteCreatePlanBuilder().BuildAsync(Request(workspace, target), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteCreateFindingCode.InvalidTarget);
        Assert.Empty(build.Formation.Effects);
        var result = new RouteCreateResultBuilder().Build(build.Formation);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.DoesNotContain("route init", result.Next?.Command ?? string.Empty, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact, Trait("Boundary", "OS"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task OccupiedIntermediatePreventsEveryEffect()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-occupied-intermediate");
        workspace.SeedBase();
        var occupant = workspace.Absolute(TeamPath);
        File.WriteAllText(occupant, "Keep this ordinary file.");
        try
        {
            var before = workspace.SnapshotHashes();
            var build = await new RouteCreatePlanBuilder().BuildAsync(Request(workspace), TestContext.Current.CancellationToken);
            Assert.Null(build.Plan);
            Assert.Empty(build.Formation.Effects);
            Assert.Equal(CliSemanticStatus.Blocked, new RouteCreateResultBuilder().Build(build.Formation).Status);
            Assert.Equal(before, workspace.SnapshotHashes());
        }
        finally { File.Delete(occupant); }
    }

    [Fact, Trait("Boundary", "OS"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task DeniedParentReplacementRetainsCreatedEffectsAndRecovery()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This replacement denial requires Windows file sharing.");
        using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-nested-partial");
        workspace.SeedBase();
        var parent = workspace.Absolute(RouteCreateIntegrationWorkspace.ParentPath);
        var before = File.ReadAllBytes(parent);
        var build = await new RouteCreatePlanBuilder().BuildAsync(Request(workspace), TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteCreatePlan>(build.Plan);
        workspace.OwnNestedApplicationPlan(plan);
        RouteCreateResultFormation applied;
        using (var held = File.Open(parent, FileMode.Open, FileAccess.Read, FileShare.Read))
            applied = await new RouteCreateApplicationOperation(workspace.LockStoreRoot)
                .ExecuteAsync(plan, TestContext.Current.CancellationToken);
        if (applied.Recovery.ResidualPath is { } residual) workspace.TrackRecoveryPath(residual);
        Assert.Contains(applied.Findings, finding => finding.Code == RouteCreateFindingCode.WriteFailed);
        Assert.Equal(before, File.ReadAllBytes(parent));
        Assert.Equal(6, applied.Effects.Length);
        Assert.All(applied.Effects.Take(5), effect =>
        {
            Assert.Equal(RouteCreateEffectOutcome.Verified, effect.Outcome);
            Assert.Equal(RouteCreateEffectResidual.Retained, effect.Residual);
        });
        Assert.Equal(RouteCreateEffectOutcome.NotStarted, applied.Effects[^1].Outcome);
        Assert.Equal(RouteCreateRecoveryState.Retained, applied.Recovery.State);
        Assert.True(File.Exists(applied.Recovery.ResidualPath));
        Assert.Equal(plan.IntendedTargetBytes.ToArray(), File.ReadAllBytes(workspace.Absolute(LeafPath)));
    }

    private static RouteCreateRequest Request(RouteCreateIntegrationWorkspace workspace, string target = "memory/project-alpha/team/topic/notes")
        => new(workspace.Workspace, target, new RouteCreateMetadataInput("Nested note", ["Docs"], null), null, RouteCreateMode.Apply);
}
