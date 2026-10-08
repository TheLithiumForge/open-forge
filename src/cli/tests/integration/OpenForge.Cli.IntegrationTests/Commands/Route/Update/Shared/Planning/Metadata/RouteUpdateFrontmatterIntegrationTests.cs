using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update.Shared.Planning.Metadata;

[Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
public sealed class RouteUpdateFrontmatterIntegrationTests
{
    [Fact(DisplayName = "Root metadata updates refresh parent Entries and converge"), Trait("Boundary", "OS")]
    public async Task RootUpdateRefreshesParentAndConverges()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-root");
        const string source = "---\ndescription: Before overview\nresponsibility: Define overview\ntags: [Before, Memory]\ncustom: retained\n---\n# Exact body\n";
        workspace.SeedTargetText(source);
        workspace.SeedSettingsText("{\"schemaVersion\":1,\"frontmatter\":\"scoped\"}");
        var request = workspace.Request();
        var result = await workspace.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(source.Replace("Before overview", "After overview", StringComparison.Ordinal), workspace.ReadText(RouteUpdateIntegrationWorkspace.TargetPath));
        Assert.Contains("[After overview](overview.md)", workspace.ReadText(RouteUpdateIntegrationWorkspace.ParentPath), StringComparison.Ordinal);
        var after = workspace.SnapshotHashes();
        var repeated = await workspace.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Effects);
        Assert.Equal(after, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Metadata creation uses root scoped or the missing-setting default"), Trait("Boundary", "OS")]
    [InlineData("root", true)]
    [InlineData("scoped", false)]
    [InlineData(null, false)]
    public async Task CreationUsesRootScopedAndMissingSetting(string? form, bool root)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-create");
        const string body = "# Exact body\n";
        workspace.SeedTargetText(body);
        if (form is not null)
        {
            workspace.SeedSettingsText($"{{\"schemaVersion\":1,\"frontmatter\":\"{form}\"}}");
        }

        var request = workspace.Request(patch: CompletePatch());
        var build = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(request);
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.NotNull(plan.Observation.MetadataSettings);
        var before = workspace.SnapshotHashes();
        var preview = await workspace.ExecuteAsync(workspace.Request(patch: CompletePatch(), mode: RouteUpdateMode.DryRun), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        var applied = await workspace.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        var members = root ? "description: After overview\ntags: [Memory]\n" : "open-forge:\n  description: After overview\n  tags: [Memory]\n";
        Assert.Equal($"---\n{members}---\n{body}", workspace.ReadText(RouteUpdateIntegrationWorkspace.TargetPath));
        var after = workspace.SnapshotHashes();
        var noOp = await workspace.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, noOp.Status);
        Assert.Empty(noOp.Effects);
        Assert.Equal(after, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "A settings change invalidates a metadata creation plan before effects"), Trait("Boundary", "OS")]
    public async Task SettingsChangeInvalidatesCreationPlan()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-settings-change");
        workspace.SeedTargetText("# Body\n");
        var build = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(workspace.Request(patch: CompletePatch()));
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        workspace.SeedSettingsText("{\"schemaVersion\":1,\"frontmatter\":\"scoped\"}");
        var current = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(workspace.Request(patch: CompletePatch()));
        var currentPlan = Assert.IsType<RouteUpdatePlan>(current.Plan);
        Assert.Equal(plan.FileChanges[0].IntendedBytes, currentPlan.FileChanges[0].IntendedBytes);
        var before = workspace.SnapshotHashes();
        var result = await workspace.ExecutePlanAsync(plan);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteUpdateFindingCode.TargetChanged);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Invalid or unavailable settings block creation while authored metadata remains editable"), Trait("Boundary", "OS")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UnavailableSettingsBlockOnlyCreation(bool unavailable)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-settings-unavailable");
        if (unavailable) workspace.SeedSettingsDirectory();
        else workspace.SeedSettingsText("{\"frontmatter\":\"unsupported\"}");
        var authored = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(workspace.Request(
            patch: RouteUpdateIntegrationWorkspace.ResponsibilityPatch("After")));
        var authoredPlan = Assert.IsType<RouteUpdatePlan>(authored.Plan);
        Assert.Null(authoredPlan.Observation.MetadataSettings);
        workspace.SeedTargetText("# Body\n");
        var before = workspace.SnapshotHashes();
        var created = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(workspace.Request(patch: CompletePatch()));
        Assert.Null(created.Plan);
        Assert.Contains(created.Formation.Findings, finding => finding.Code == RouteUpdateFindingCode.FrontmatterUnsafe);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Root entrypoint updates preserve compatibility filename and overwrite identity"), Trait("Boundary", "OS")]
    public async Task RootEntrypointAndOverwriteIdentityRemainStable()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-root-entrypoint");
        const string path = ".agents/memory/project-alpha/overview/index.md";
        workspace.SeedTargetForm(path);
        var source = OpenForgeDocumentSeed.Metadata("Before overview", ["Memory"],
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Prefix = "# Overview\n\n## Axioms\n\n- inherited - No local axioms.",
                Entries = string.Empty,
            }));
        var rootSource = source.Replace("open-forge:\n", string.Empty, StringComparison.Ordinal)
            .Replace("  description:", "description:", StringComparison.Ordinal)
            .Replace("  tags:", "tags:", StringComparison.Ordinal)
            .Replace("  responsibility:", "responsibility:", StringComparison.Ordinal);
        File.WriteAllText(workspace.Absolute(path), rootSource);
        var overwrite = path[..^3] + ".overwrite.md";
        workspace.SeedEntrypointOverwrite(overwrite, "# Exact overwrite\n");
        var request = workspace.Request(sourceReference: path, patch: RouteUpdateIntegrationWorkspace.ResponsibilityPatch("After"));
        var result = await workspace.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(path, result.Target?.Path);
        Assert.Equal("# Exact overwrite\n", workspace.ReadText(overwrite));
        Assert.False(File.Exists(workspace.Absolute(".agents/memory/project-alpha/overview/_overview.md")));
    }

    internal static RouteUpdatePatchRequest CompletePatch()
        => RouteUpdateIntegrationWorkspace.Patch(
            description: new RouteUpdateDescriptionRequest { Requested = true, Value = "After overview" },
            tags: new RouteUpdateTagsRequest { Requested = true, Values = ["Memory"] });
}
