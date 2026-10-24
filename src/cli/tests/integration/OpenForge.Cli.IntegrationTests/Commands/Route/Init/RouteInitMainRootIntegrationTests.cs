using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

[Trait("Feature", "route-init"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteInitMainRootIntegrationTests
{
    private const string MainRoot = "handbook/context";
    private const string CaseAliasRoot = "HANDBOOK/context";
    private const string OtherRoot = "handbook/other";
    private const string Loader = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string Category = "notes";
    private const string Child = "releases";
    private const string RouteId = Category + "/" + Child;
    private const string ParentPath = MainRoot + "/" + Category + "/_" + Category + SourceFormClassifier.MarkdownExtension;
    private const string TargetPath = MainRoot + "/" + RouteId + "/_" + Child + SourceFormClassifier.MarkdownExtension;
    private const string LiteralPrefix = SourceWorkspaceRelativePath.LiteralReferencePrefix;

    [Fact(DisplayName = "Route Init maps stable IDs and literal paths to the same bounded custom-root chain")]
    public async Task StableIdAndLiteralPathHaveTheSameChain()
    {
        using var workspace = Fixture();
        var byId = await PlanAsync(workspace, RouteId);
        var byPath = await PlanAsync(workspace, LiteralPrefix + TargetPath);
        Assert.Equal([ParentPath, TargetPath], byId.Preview.Entrypoints.Select(entry => entry.Path));
        Assert.Equal(byId.Preview.Entrypoints.Select(entry => entry.Path), byPath.Preview.Entrypoints.Select(entry => entry.Path));
        Assert.Equal(MainRoot, byId.LayoutRead.Layout?.MainRoot);
        Assert.Equal([MainRoot + "/" + Category, MainRoot + "/" + RouteId], byId.DirectoryCreations.Select(directory =>
            Path.GetRelativePath(workspace.Path, directory.LogicalPath).Replace('\\', '/')));
        Assert.False(Directory.Exists(workspace.Combine(WorkspaceLayout.DefaultMainRoot, Category)));
    }

    [Theory(DisplayName = "Route Init refuses literal destinations outside its ordinal content roots")]
    [InlineData(WorkspaceLayout.DefaultMainRoot + "/" + RouteId + "/_" + Child + SourceFormClassifier.MarkdownExtension)]
    [InlineData(CaseAliasRoot + "/" + RouteId + "/_" + Child + SourceFormClassifier.MarkdownExtension)]
    [InlineData(MainRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName + "/_" + WorkspaceLayoutNames.SkillsDirectoryName + SourceFormClassifier.MarkdownExtension)]
    public async Task LiteralPathsAreNotRedirectedOrAdmittedByCase(string path)
    {
        using var workspace = Fixture();
        var build = await new RouteInitPlanBuilder().BuildAsync(Request(workspace, LiteralPrefix + path), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteInitFindingCode.InvalidTarget);
        Assert.False(File.Exists(workspace.Combine(path)));
    }

    [Theory(DisplayName = "Route Init restoration keeps package asset coordinates and maps workspace destinations")]
    [InlineData(WorkspaceCategoryNames.Patterns)]
    [InlineData(WorkspaceLayoutNames.SkillsDirectoryName)]
    public async Task RestorationSeparatesPackageAndWorkspacePaths(string category)
    {
        using var workspace = Fixture();
        var plan = await PlanAsync(workspace, category, RouteInitScaffold.Framework);
        var destination = category == WorkspaceLayoutNames.SkillsDirectoryName
            ? WorkspaceLayout.NativeSkillsPath : MainRoot + "/" + category;
        var packageDirectory = WorkspaceLayout.DefaultMainRoot + "/" + category;
        var payload = Assert.IsType<OpenForge.Cli.Core.Framework.Distribution.Models.FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        var expected = payload.Assets.Where(asset => asset.Path.StartsWith(packageDirectory + "/", StringComparison.Ordinal))
            .Select(asset => destination + asset.Path[packageDirectory.Length..]).Order(StringComparer.Ordinal).ToArray();
        var actual = plan.FileChanges.Select(change => Path.GetRelativePath(workspace.Path, change.LogicalPath).Replace('\\', '/'))
            .Where(path => path.StartsWith(destination + "/", StringComparison.Ordinal)).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(expected, actual);
        Assert.All(plan.Preview.Entrypoints.Where(entry => entry.SourceAssetPath is not null), entry =>
            Assert.Contains(entry.SourceAssetPath, payload.Assets.Select(asset => asset.Path)));
        Assert.False(Directory.Exists(workspace.Combine(destination)));
    }

    [Fact(DisplayName = "Route Init revalidation rejects a changed main root even when the literal target remains untouched")]
    public async Task LayoutChangeInvalidatesThePlan()
    {
        using var workspace = Fixture();
        var plan = await PlanAsync(workspace, LiteralPrefix + TargetPath);
        workspace.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings(OtherRoot));
        var validation = await new RouteInitPlanRevalidator(new RouteInitPlanBuilder())
            .RevalidateAsync(plan, TestContext.Current.CancellationToken);
        Assert.Equal(RouteInitPlanRevalidationState.Changed, validation.State);
        var outcome = await RouteInitOperationFactory.Create().ApplicationOperation.ExecuteAsync(plan, TestContext.Current.CancellationToken);
        Assert.Contains(outcome.Formation.Findings, finding => finding.Code == RouteInitFindingCode.TargetChanged);
        Assert.False(File.Exists(workspace.Combine(TargetPath)));
        workspace.DeleteRuntimeState(OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime.WorkspaceRuntimePaths.DirectoryName);
    }

    private static TemporaryWorkspace Fixture()
    {
        var workspace = TemporaryWorkspace.Create(nameof(RouteInitMainRootIntegrationTests));
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings(MainRoot));
        workspace.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        return workspace;
    }

    private static string Settings(string root)
        => $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},
             "{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}"}
            """;

    private static RouteInitRequest Request(TemporaryWorkspace workspace, string target, RouteInitScaffold scaffold = RouteInitScaffold.Generic)
        => new(new CliWorkspace(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace),
            target, scaffold, RouteInitMode.Apply, RouteInitMetadataInput.None);

    private static async Task<RouteInitPlan> PlanAsync(TemporaryWorkspace workspace, string target, RouteInitScaffold scaffold = RouteInitScaffold.Generic)
    {
        var build = await new RouteInitPlanBuilder().BuildAsync(Request(workspace, target, scaffold), TestContext.Current.CancellationToken);
        Assert.True(build.Plan is not null, string.Join(Environment.NewLine, build.Formation.Findings.Select(finding => finding.Cause)));
        return Assert.IsType<RouteInitPlan>(build.Plan);
    }
}
