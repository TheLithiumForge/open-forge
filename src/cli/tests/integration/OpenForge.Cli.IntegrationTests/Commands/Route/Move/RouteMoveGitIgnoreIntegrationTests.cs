using System.Text.Json;
using OpenForge.Cli.Composition.Shared.RouteReferences;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Models.Sharing;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.References;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class RouteMoveGitIgnoreIntegrationTests
{
    [Theory(DisplayName = "Route Move prunes ignored dependency and build folders before checking their junctions"),
     InlineData("node_modules"), InlineData("bin"), InlineData("obj"), InlineData("artifacts"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public static async Task IgnoredFoldersPreserveMarkdownAndDoNotBlock(string folder)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-git-ignored");
        using var repository = new RouteReferenceGitWorkspace(workspace.Workspace.LexicalRoot);
        using var target = TemporaryWorkspace.Create("move-ignored-link-target");
        await repository.InitializeAsync();
        workspace.WriteText(".gitignore", $"{folder}/\n");
        const string ignored = "[Old guide](../.agents/guidance/old%20guide.md#section)\n";
        workspace.WriteText($"{folder}/ignored.md", ignored);
        target.CreateFile("invalid.md", [0xff]);
        var link = workspace.Absolute($"{folder}/linked-dependency");
        await repository.CreateDirectoryLinkAsync(link, target.Path);
        workspace.OwnLeafDestination(crossRoute: true);
        try
        {
            var result = await CliHostCapture.RunAsync(
                ["route", "move", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.CrossRouteDestination,
                 "--format", "json", "--detail", "debug"], workspace.Workspace.LexicalRoot);
            Assert.Equal(0, result.ExitCode);
            Assert.Equal(ignored, workspace.ReadText($"{folder}/ignored.md"));
            Assert.False(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.LeafPath)));
            Assert.Contains(".agents/archive/new%20guide.md#section", workspace.ReadText("README.md"), StringComparison.Ordinal);
            using var json = JsonDocument.Parse(result.Output);
            Assert.DoesNotContain(json.RootElement.GetProperty("data").GetProperty("rewrittenLinks").EnumerateArray(),
                item => item.GetProperty("path").GetString() == $"{folder}/ignored.md");
            Assert.Contains("gitIgnoredPaths=1", result.Error, StringComparison.Ordinal);
            Assert.Equal([0xff], File.ReadAllBytes(target.Combine("invalid.md")));
        }
        finally
        {
            Directory.Delete(link);
        }
    }

    [Theory(DisplayName = "Route Move rewrites a Git-ignored recorded private route inside agents"),
     InlineData(".agents"), InlineData(".AGENTS"), InlineData(".AgEnTs"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public static async Task PrivateAgentsReferencesRemainCovered(string physicalRoot)
    {
        if (physicalRoot != ".agents" && !OperatingSystem.IsWindows())
            Assert.Skip("Mixed-case agents identity requires a case-insensitive Windows filesystem.");
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-git-private");
        using var repository = new RouteReferenceGitWorkspace(workspace.Workspace.LexicalRoot);
        await repository.InitializeAsync();
        if (physicalRoot != ".agents") repository.RenameAgentsRoot(physicalRoot);
        workspace.WriteText(".gitignore", $"{physicalRoot}/guidance/topics/\n");
        var git = new RouteGitIgnoredPathEnumerator(configureProcess: repository.Configure);
        var ignored = await git.EnumerateAsync(workspace.Workspace.LexicalRoot, TestContext.Current.CancellationToken);
        Assert.NotNull(ignored);
        Assert.Contains($"{physicalRoot}/guidance/topics", ignored.Value);
        var catalogue = await new RouteMarkdownCatalogueReader(new PhysicalPathResolver(), git.EnumerateAsync)
            .ReadAsync(new RouteMarkdownCatalogueRequest(workspace.Workspace, ["."], [], new RouteMarkdownCatalogueFilters([".md"])),
                TestContext.Current.CancellationToken);
        Assert.Equal(RouteMarkdownCatalogueCoverage.Complete, catalogue.Coverage);
        Assert.Contains(".agents/guidance/topics/child.md", catalogue.SelectedPaths);
        Assert.Contains(".agents/guidance/topics/child.overwrite.md", catalogue.SelectedPaths);
        Assert.DoesNotContain(catalogue.SelectedPaths, path => path.StartsWith(".AGENTS/", StringComparison.Ordinal)
            || path.StartsWith(".AgEnTs/", StringComparison.Ordinal));
        var ownership = WorkspaceOwnershipCodec.Read(File.ReadAllBytes(workspace.Absolute(RouteMoveIntegrationWorkspace.OwnershipPath))).Document!;
        workspace.WriteText(RouteMoveIntegrationWorkspace.OwnershipPath, System.Text.Encoding.UTF8.GetString(
            WorkspaceOwnershipCodec.Write(ownership with
            {
                Framework = Assert.IsType<FrameworkOwnership>(ownership.Framework) with
                {
                    GitIgnoredRoutes = [new SourceSharingRoute(".agents/guidance/topics", RouteMoveIntegrationWorkspace.CategoryPath)],
                },
            })));
        workspace.OwnLeafDestination(crossRoute: true);
        var result = await CliHostCapture.RunAsync(
            ["route", "move", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.CrossRouteDestination],
            workspace.Workspace.LexicalRoot);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("../../archive/new%20guide.md#section", workspace.ReadText(".agents/guidance/topics/child.md"), StringComparison.Ordinal);
        Assert.Contains("../../archive/new%20guide.md#section", workspace.ReadText(".agents/guidance/topics/child.overwrite.md"), StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Route Move keeps complete reference coverage without a usable Git inventory"),
     InlineData("non-git"), InlineData("missing-git"), InlineData("git-failed"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public static async Task GitFallbackKeepsCompleteScan(string scenario)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-git-fallback");
        using var repository = new RouteReferenceGitWorkspace(workspace.Workspace.LexicalRoot);
        if (scenario != "non-git") await repository.InitializeAsync();
        workspace.WriteText(".gitignore", "ignored.md\n");
        workspace.WriteText("ignored.md", "[Old guide](.agents/guidance/old%20guide.md#section)\n");
        var adapter = new RouteGitIgnoredPathEnumerator(
            scenario == "missing-git" ? workspace.Absolute("missing-git") : "git",
            start =>
            {
                repository.Configure(start);
                if (scenario == "git-failed" && start.ArgumentList.Contains("ls-files"))
                    start.ArgumentList.Insert(0, "--unsupported-open-forge-test-option");
            });
        workspace.OwnLeafDestination(crossRoute: true);
        var result = await RouteMoveOperationFactory.Create(workspace.LockStoreRoot, ignoredPathEnumerator: adapter.EnumerateAsync)
            .ExecuteAsync(workspace.Request(destinationTarget: RouteMoveIntegrationWorkspace.CrossRouteDestination, mode: RouteMoveMode.Apply),
                TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(0, result.References.IgnoredPathCount);
        Assert.Equal("[Old guide](.agents/archive/new%20guide.md#section)\n", workspace.ReadText("ignored.md"));
    }

    [Fact(DisplayName = "The reference catalogue honors all standard Git excludes while retaining tracked files and agents"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public static async Task StandardExcludesRetainTrackedFilesAndAgents()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-standard-excludes");
        using var repository = new RouteReferenceGitWorkspace(workspace.Workspace.LexicalRoot);
        await repository.InitializeAsync();
        workspace.WriteText(".gitignore", "tracked.md\n.agents/\n");
        workspace.WriteText("tracked.md", "Tracked.\n");
        await repository.GitAsync("add", "--force", "tracked.md");
        workspace.WriteText("nested/.gitignore", "ignored.md\n");
        workspace.WriteText("nested/ignored.md", "Ignored.\n");
        workspace.WriteText("global.md", "Ignored.\n");
        workspace.WriteText("info.md", "Ignored.\n");
        File.WriteAllText(workspace.Absolute(".git/global-ignore"), "global.md\n");
        File.WriteAllText(workspace.Absolute(".git/info/exclude"), "info.md\n");
        var catalogue = await new RouteMarkdownCatalogueReader(new PhysicalPathResolver(),
                new RouteGitIgnoredPathEnumerator(configureProcess: repository.Configure).EnumerateAsync)
            .ReadAsync(new RouteMarkdownCatalogueRequest(workspace.Workspace, ["."], [], new RouteMarkdownCatalogueFilters([".md"])),
                TestContext.Current.CancellationToken);
        Assert.Equal(RouteMarkdownCatalogueCoverage.Complete, catalogue.Coverage);
        Assert.Contains("tracked.md", catalogue.SelectedPaths);
        Assert.Contains("README.md", catalogue.SelectedPaths);
        Assert.Contains(RouteMoveIntegrationWorkspace.LeafPath, catalogue.SelectedPaths);
        Assert.DoesNotContain("nested/ignored.md", catalogue.SelectedPaths);
        Assert.DoesNotContain("global.md", catalogue.SelectedPaths);
        Assert.DoesNotContain("info.md", catalogue.SelectedPaths);
        Assert.Equal(3, catalogue.IgnoredPathCount);
    }

    [Fact(DisplayName = "A non-Git workspace still checks links in folders named by gitignore"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public static async Task NonGitWorkspaceKeepsLinkSafetyChecks()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-non-git-link");
        using var repository = new RouteReferenceGitWorkspace(workspace.Workspace.LexicalRoot);
        using var target = TemporaryWorkspace.Create("move-non-git-link-target");
        workspace.WriteText(".gitignore", "node_modules/\n");
        workspace.WriteText("node_modules/ignored.md", "Ignored only when Git confirms it.\n");
        var link = workspace.Absolute("node_modules/dependency");
        await repository.CreateDirectoryLinkAsync(link, target.Path);
        try
        {
            var result = await CliHostCapture.RunAsync(
                ["route", "move", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.CrossRouteDestination, "--dry-run"],
                workspace.Workspace.LexicalRoot);
            Assert.Equal(5, result.ExitCode);
            Assert.True(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.LeafPath)));
            Assert.False(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.CrossRouteDestination)));
        }
        finally
        {
            Directory.Delete(link);
        }
    }

    [Fact(DisplayName = "The Windows reference catalogue matches differently cased Git exclusions with physical path policy"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public static async Task GitExclusionsUseFilesystemCaseComparison()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This physical path comparison requires Windows.");
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-git-ignore-case");
        using var repository = new RouteReferenceGitWorkspace(workspace.Workspace.LexicalRoot);
        await repository.InitializeAsync();
        workspace.WriteText(".gitignore", "Node_Modules/\n");
        workspace.WriteText("Node_Modules/ignored.md", "Ignored.\n");
        var git = new RouteGitIgnoredPathEnumerator(configureProcess: repository.Configure);
        var ignored = await git.EnumerateAsync(workspace.Workspace.LexicalRoot, TestContext.Current.CancellationToken);
        Assert.NotNull(ignored);
        Assert.Contains("Node_Modules", ignored.Value);
        var catalogue = await new RouteMarkdownCatalogueReader(new PhysicalPathResolver(), git.EnumerateAsync)
            .ReadAsync(new RouteMarkdownCatalogueRequest(workspace.Workspace, ["node_modules"], [], new RouteMarkdownCatalogueFilters([".md"])),
                TestContext.Current.CancellationToken);
        Assert.Equal(RouteMarkdownCatalogueCoverage.Complete, catalogue.Coverage);
        Assert.Empty(catalogue.SelectedPaths);
        Assert.Equal(1, catalogue.IgnoredPathCount);
    }
}
