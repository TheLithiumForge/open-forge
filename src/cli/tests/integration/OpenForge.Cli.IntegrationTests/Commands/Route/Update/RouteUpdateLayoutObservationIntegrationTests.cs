using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

[Trait("Feature", "route-update"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteUpdateLayoutObservationIntegrationTests
{
    private const string Description = "Updated overview";
    private const string Tag = "Memory";
    private const string Body = "# Overview\n";

    [Fact(DisplayName = "Settings changes with the same main root stop an authored metadata update before effects")]
    public async Task SettingsChangeInvalidatesAuthoredMetadataPlan()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(nameof(SettingsChangeInvalidatesAuthoredMetadataPlan));
        workspace.SeedSettingsText(Settings(WorkspaceSettingsDefinitions.FrontmatterRootName));
        var request = workspace.Request();
        var build = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(request);
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.Null(plan.Observation.MetadataSettings);
        Assert.NotEmpty(plan.FileChanges);

        File.WriteAllText(workspace.Absolute(WorkspaceSettingsDefinitions.RelativePath),
            Settings(WorkspaceSettingsDefinitions.FrontmatterScopedName));
        var freshBuild = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(request);
        var fresh = Assert.IsType<RouteUpdatePlan>(freshBuild.Plan);
        Assert.Equal(plan.LayoutRead.Layout?.MainRoot, fresh.LayoutRead.Layout?.MainRoot);
        Assert.Equal(plan.Observation.TargetSnapshot.Expectation, fresh.Observation.TargetSnapshot.Expectation);
        Assert.Equal(plan.FileChanges.Length, fresh.FileChanges.Length);
        foreach (var (expected, actual) in plan.FileChanges.Zip(fresh.FileChanges))
        {
            Assert.Equal(expected.Expectation, actual.Expectation);
            Assert.Equal(expected.IntendedBytes, actual.IntendedBytes);
        }

        var before = workspace.SnapshotHashes();
        var result = await workspace.ExecutePlanAsync(plan);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteUpdateFindingCode.TargetChanged);
        Assert.All(result.Effects, effect => Assert.Equal(RouteUpdateEffectOutcome.NotStarted, effect.Outcome));
        Assert.Equal(RouteUpdateRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(RouteUpdateVerificationState.NotRequested, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Metadata creation consumes the settings retained by the selected layout observation")]
    public async Task MetadataCreationUsesSelectedSettings()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(nameof(MetadataCreationUsesSelectedSettings));
        workspace.SeedTargetText(Body);
        workspace.SeedSettingsText(Settings(WorkspaceSettingsDefinitions.FrontmatterRootName));
        var patch = RouteUpdateIntegrationWorkspace.DescriptionPatch(Description) with
        {
            Tags = new RouteUpdateTagsRequest { Requested = true, Values = [Tag] },
        };

        var build = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(workspace.Request(patch: patch));
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);

        Assert.Same(plan.LayoutRead.Settings, plan.Observation.MetadataSettings);
    }

    private static string Settings(string frontmatter) => $$"""
        {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.FrontmatterProperty}}":"{{frontmatter}}"}
        """;
}
