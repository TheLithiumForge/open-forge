using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Definitions;
using OpenForge.Cli.Invocation;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

public sealed class RouteListPhysicalAliasRefinementIntegrationTests
{
    [Fact(DisplayName = "Route-list component containment enumerates an ordinary owned workspace"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task OrdinaryWorkspaceEnumeratesCanonicalRows()
    {
        using var workspace = CreateRootedWorkspace();

        var result = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(["root", "root/child"], result.Result.Rows.Select(row => row.Id));
    }

    [Fact(DisplayName = "Route-list component containment accepts an owned workspace-root alias"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task WorkspaceRootAliasEnumeratesCanonicalRows()
    {
        using var workspace = CreateRootedWorkspace(nested: true);
        var aliasPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(workspace.Path)!,
            "workspace-alias");
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(aliasPath, workspace.Path);
        var request = new RouteListRequest(
            new CliWorkspace(aliasPath, CliWorkspaceSelection.ExplicitWorkspace),
            RouteListSourceReference.Parse("root"),
            RouteListDepth.All);

        var result = await RunWithoutWritesAsync(workspace, request);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(["root", "root/child"], result.Result.Rows.Select(row => row.Id));
        Assert.NotNull(new DirectoryInfo(aliasPath).LinkTarget);
    }

    [Fact(DisplayName = "Route-list component containment traverses one contained directory alias at its canonical logical path"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task ContainedDirectoryAliasIsNotSeparatelyTraversed()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.Write("contained-target/_alias.md", Route("Contained alias", "Alias"));
        var linkPath = System.IO.Path.Combine(workspace.Path, ".agents", "root", "alias");
        var targetPath = System.IO.Path.Combine(workspace.Path, "contained-target");
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(linkPath, targetPath);

        var result = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var row = Assert.Single(result.Result.Rows, candidate => candidate.Id == "root/alias");
        Assert.Equal(".agents/root/alias/_alias.md", row.Path);
        Assert.DoesNotContain(result.Result.Rows, candidate => candidate.Path.Contains("contained-target", StringComparison.Ordinal));
        Assert.NotNull(new DirectoryInfo(linkPath).LinkTarget);
    }

    [Fact(DisplayName = "Route-list component containment resolves a contained relative directory link at its canonical logical path"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task ContainedRelativeDirectoryAliasUsesLogicalIdentity()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.Write("contained-relative-target/_relative.md", Route("Contained relative alias", "Alias"));
        var linkPath = System.IO.Path.Combine(workspace.Path, ".agents", "root", "relative");
        var targetPath = System.IO.Path.Combine(workspace.Path, "contained-relative-target");
        var relativeTarget = System.IO.Path.GetRelativePath(System.IO.Path.GetDirectoryName(linkPath)!, targetPath);
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(linkPath, relativeTarget);

        var result = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var row = Assert.Single(result.Result.Rows, candidate => candidate.Id == "root/relative");
        Assert.Equal(".agents/root/relative/_relative.md", row.Path);
        Assert.DoesNotContain(result.Result.Rows, candidate => candidate.Path.Contains("contained-relative-target", StringComparison.Ordinal));
        Assert.Equal(relativeTarget, new DirectoryInfo(linkPath).LinkTarget);
    }

    [Fact(DisplayName = "Route-list component containment reads a contained file alias under its canonical logical identity"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task ContainedFileAliasUsesLogicalIdentity()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.Write("contained-target.md", Route("Contained file", "Alias"));
        var linkPath = System.IO.Path.Combine(workspace.Path, ".agents", "root", "linked.md");
        var targetPath = System.IO.Path.Combine(workspace.Path, "contained-target.md");
        RouteListLinkTestSupport.CreateFileSymbolicLinkOrSkip(linkPath, targetPath);

        var result = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var row = Assert.Single(result.Result.Rows, candidate => candidate.Id == "root/linked");
        Assert.Equal(".agents/root/linked.md", row.Path);
        Assert.NotNull(new FileInfo(linkPath).LinkTarget);
    }

    [Fact(DisplayName = "Route-list component containment blocks a link chain that leaves and re-enters the workspace"),
     Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task ExternalThenContainedDirectoryLinkChainIsBlockedAtFirstLogicalLink()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.Write("contained-return/_return.md", Route("Contained return target", "Return"));
        using var outside = TemporaryWorkspace.Create("route-list-external-return-chain");
        var insideTargetPath = System.IO.Path.Combine(workspace.Path, "contained-return");
        var externalLinkPath = outside.Combine("return-inside");
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(externalLinkPath, insideTargetPath);
        var logicalLinkPath = System.IO.Path.Combine(workspace.Path, ".agents", "root", "return");
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(logicalLinkPath, externalLinkPath);
        var selectedBefore = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();
        var request = workspace.Request("root", RouteListDepth.All);

        var first = await RouteListOperation.RunAsync(request, TestContext.Current.CancellationToken);
        var second = await RouteListOperation.RunAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(
            RouteListResultProjectionTestSupport.Create(first),
            RouteListResultProjectionTestSupport.Create(second));
        Assert.Equal(selectedBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
        Assert.NotNull(new DirectoryInfo(logicalLinkPath).LinkTarget);
        Assert.NotNull(new DirectoryInfo(externalLinkPath).LinkTarget);
        Assert.Equal(CliSemanticStatus.Blocked, first.Status);
        Assert.Contains(
            first.Result.Findings,
            finding => finding.Code == RouteListFindingCodes.PhysicalEscape
                && finding.Path == ".agents/root/return");
        Assert.DoesNotContain(
            first.Result.Rows,
            row => row.Id == "root/return"
                || row.Id.StartsWith("root/return/", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Route-list component containment blocks an external directory alias without traversing outside"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task ExternalDirectoryAliasIsBlocked()
    {
        using var workspace = CreateRootedWorkspace();
        using var outside = TemporaryWorkspace.Create("route-list-external-directory");
        outside.WriteText("index.md", Route("Outside", "Outside"));
        var outsideBefore = outside.SnapshotHashes();
        var linkPath = System.IO.Path.Combine(workspace.Path, ".agents", "root", "escape");
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(linkPath, outside.Path);

        var result = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Result.Findings,
            finding => finding.Code == RouteListFindingCodes.PhysicalEscape
                && finding.Path == ".agents/root/escape");
        Assert.DoesNotContain(result.Result.Rows, row => row.Id.StartsWith("root/escape", StringComparison.Ordinal));
        Assert.NotNull(new DirectoryInfo(linkPath).LinkTarget);
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    [Fact(DisplayName = "Route-list component containment blocks an external file alias without reading outside"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task ExternalFileAliasIsBlocked()
    {
        using var workspace = CreateRootedWorkspace();
        using var outside = TemporaryWorkspace.Create("route-list-external-file");
        outside.WriteText("outside.md", Route("Outside", "Outside"));
        var outsideBefore = outside.SnapshotHashes();
        var linkPath = System.IO.Path.Combine(workspace.Path, ".agents", "root", "escape.md");
        var targetPath = outside.Combine("outside.md");
        RouteListLinkTestSupport.CreateFileSymbolicLinkOrSkip(linkPath, targetPath);

        var result = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Result.Findings,
            finding => finding.Code == RouteListFindingCodes.PhysicalEscape
                && finding.Path == ".agents/root/escape.md");
        Assert.DoesNotContain(result.Result.Rows, row => row.Id == "root/escape");
        Assert.NotNull(new FileInfo(linkPath).LinkTarget);
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    [Fact(DisplayName = "Route-list component containment blocks an escaping agents-directory ancestor link without descendant traversal"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task EscapingAgentsAncestorLinkIsBlocked()
    {
        using var workspace = RouteListTestWorkspace.Create();
        using var outside = TemporaryWorkspace.Create("route-list-external-agents-ancestor");
        outside.WriteText(
            "loader.md",
            "# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) - #Root\n<!-- open-forge:generated-index:end -->\n");
        outside.WriteText("root/_root.md", Route("Outside root", "Root"));
        outside.WriteText("root/child.md", Route("Outside child", "Child"));
        var outsideBefore = outside.SnapshotHashes();
        var agentsPath = System.IO.Path.Combine(workspace.Path, ".agents");
        Directory.Delete(agentsPath);
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(agentsPath, outside.Path);
        var selectedBefore = workspace.SnapshotHashes();
        var request = workspace.Request("root", RouteListDepth.All);

        var first = await RouteListOperation.RunAsync(request, TestContext.Current.CancellationToken);
        var second = await RouteListOperation.RunAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(
            RouteListResultProjectionTestSupport.Create(first),
            RouteListResultProjectionTestSupport.Create(second));
        Assert.Equal(CliSemanticStatus.Blocked, first.Status);
        var finding = Assert.Single(first.Result.Findings);
        Assert.Equal(RouteListFindingCodes.PhysicalEscape, finding.Code);
        Assert.Equal(".agents", finding.Path);
        Assert.Empty(first.Result.Rows);
        Assert.Equal(selectedBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
        Assert.NotNull(new DirectoryInfo(agentsPath).LinkTarget);
    }

    [Fact(DisplayName = "Route-list component containment reports a dangling link deterministically"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task DanglingLinkIsDeterministicallyBlocked()
    {
        using var workspace = CreateRootedWorkspace();
        var linkPath = System.IO.Path.Combine(workspace.Path, ".agents", "root", "dangling.md");
        var targetPath = System.IO.Path.Combine(workspace.Path, "missing-target.md");
        RouteListLinkTestSupport.CreateFileSymbolicLinkOrSkip(linkPath, targetPath);

        var first = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));
        var second = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));

        Assert.Equal(
            RouteListResultProjectionTestSupport.Create(first),
            RouteListResultProjectionTestSupport.Create(second));
        Assert.Equal(CliSemanticStatus.Blocked, first.Status);
        Assert.Contains(first.Result.Findings, finding => finding.Path == ".agents/root/dangling.md");
        Assert.NotNull(new FileInfo(linkPath).LinkTarget);
    }

    [Fact(DisplayName = "Route-list component containment blocks repeated aliases to one physical directory after one canonical traversal"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task RepeatedPhysicalDirectoryIdentityIsBlocked()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.Write("shared-target/index.md", Route("Shared", "Shared"));
        var targetPath = System.IO.Path.Combine(workspace.Path, "shared-target");
        var firstLink = System.IO.Path.Combine(workspace.Path, ".agents", "root", "a");
        var secondLink = System.IO.Path.Combine(workspace.Path, ".agents", "root", "b");
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(firstLink, targetPath);
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(secondLink, targetPath);

        var result = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Single(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.PhysicalCycle);
        Assert.DoesNotContain(result.Result.Rows, row => row.Id == "root/b");
        Assert.NotNull(new DirectoryInfo(firstLink).LinkTarget);
        Assert.NotNull(new DirectoryInfo(secondLink).LinkTarget);
    }

    [Fact(DisplayName = "Route-list component containment stops a cyclic directory alias without repeated traversal"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task CyclicPhysicalIdentityIsBlocked()
    {
        using var workspace = CreateRootedWorkspace();
        var rootPath = System.IO.Path.Combine(workspace.Path, ".agents", "root");
        var cyclePath = System.IO.Path.Combine(rootPath, "cycle");
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(cyclePath, rootPath);

        var result = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Single(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.PhysicalCycle);
        Assert.DoesNotContain(result.Result.Rows, row => row.Id.StartsWith("root/cycle", StringComparison.Ordinal));
        Assert.NotNull(new DirectoryInfo(cyclePath).LinkTarget);
    }

    [Fact(DisplayName = "Route-list component containment reports a deterministic two-node directory-link cycle"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task TwoNodeDirectoryLinkCycleIsBlocked()
    {
        using var workspace = CreateRootedWorkspace();
        var rootPath = System.IO.Path.Combine(workspace.Path, ".agents", "root");
        var firstLink = System.IO.Path.Combine(rootPath, "cycle-a");
        var secondLink = System.IO.Path.Combine(rootPath, "cycle-b");
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(firstLink, "cycle-b");
        RouteListLinkTestSupport.CreateDirectorySymbolicLinkOrSkip(secondLink, "cycle-a");

        var first = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));
        var second = await RunWithoutWritesAsync(workspace, workspace.Request("root", RouteListDepth.All));

        Assert.Equal(
            RouteListResultProjectionTestSupport.Create(first),
            RouteListResultProjectionTestSupport.Create(second));
        Assert.Equal(CliSemanticStatus.Blocked, first.Status);
        Assert.Contains(
            first.Result.Findings,
            finding => finding.Code == RouteListFindingCodes.PhysicalCycle
                && finding.Path is ".agents/root/cycle-a" or ".agents/root/cycle-b");
        Assert.DoesNotContain(first.Result.Rows, row => row.Id.StartsWith("root/cycle-", StringComparison.Ordinal));
        Assert.NotNull(new DirectoryInfo(firstLink).LinkTarget);
        Assert.NotNull(new DirectoryInfo(secondLink).LinkTarget);
    }

    [Fact(DisplayName = "Route-list exact-path case follows active operating-system path semantics and reports canonical identity"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task ExactPathCaseUsesOperatingSystemSemantics()
    {
        using var workspace = RouteListTestWorkspace.Create();
        workspace.WriteLoader("- [Case](Case/_Case.md) - #Case");
        workspace.WriteRoute(".agents/Case/_Case.md", "Case", "Case");

        var result = await RunWithoutWritesAsync(
            workspace,
            workspace.Request(".agents/case/_case.md", RouteListDepth.Bounded(0)));

        if (OperatingSystem.IsWindows())
        {
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal("Case", result.Result.Selection.SourceId);
            Assert.Equal(".agents/Case/_Case.md", result.Result.Selection.SourcePath);
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Invalid, result.Status);
            Assert.Null(result.Result.Selection.SourceId);
            Assert.Equal(".agents/case/_case.md", result.Result.Selection.SourcePath);
        }
    }

    private static RouteListTestWorkspace CreateRootedWorkspace(bool nested = false)
    {
        var workspace = nested ? RouteListTestWorkspace.CreateNested() : RouteListTestWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.WriteRoute(".agents/root/_root.md", "Root", "Root");
        workspace.WriteRoute(".agents/root/child.md", "Child", "Child");
        return workspace;
    }

    private static string Route(string description, string tag)
    {
        return $"---\nopen-forge:\n  description: {description}\n  tags: [{tag}]\n---\n\n# {description}\n";
    }

    private static async Task<RouteListResult> RunWithoutWritesAsync(
        RouteListTestWorkspace workspace,
        RouteListRequest request)
    {
        var before = workspace.SnapshotHashes();
        var result = await RouteListOperation.RunAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(before, workspace.SnapshotHashes());
        return result;
    }
}
