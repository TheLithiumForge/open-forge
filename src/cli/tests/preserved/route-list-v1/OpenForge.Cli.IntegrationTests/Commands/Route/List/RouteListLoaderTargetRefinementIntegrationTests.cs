using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

public sealed class RouteListLoaderTargetRefinementIntegrationTests
{
    [Theory(DisplayName = "Route-list Loader rejects malformed percent triplets as incomplete authored declarations"),
     InlineData("root%/_root.md"),
     InlineData("root%2/_root.md"),
     InlineData("root%GG/_root.md"),
     Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task LoaderRejectsMalformedPercentTriplets(string destination)
    {
        using var workspace = CreateWorkspace(destination);

        var result = await RunWithoutWritesAsync(workspace);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, result.Result.Coverage.State);
        var finding = Assert.Single(result.Result.Findings);
        Assert.Equal(RouteListFindingCodes.LoaderEntryMalformed, finding.Code);
        Assert.Equal(destination, finding.Path);
    }

    [Theory(DisplayName = "Route-list Loader blocks decoded controls separators delimiters rooted forms and traversal before lookup"),
     InlineData("root%5C_root.md"),
     InlineData("root/%00source.md"),
     InlineData("root/%1Fsource.md"),
     InlineData("root%3Fquery/_root.md"),
     InlineData("root%23fragment/_root.md"),
     InlineData("root%3Astream/_root.md"),
     InlineData("/root/_root.md"),
     InlineData("file:/root/_root.md"),
     InlineData("C:/root/_root.md"),
     InlineData("root%2F%2F_root.md"),
     InlineData("root/%2E/_root.md"),
     InlineData("root/%2E%2E/_root.md"),
     Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task LoaderBlocksDecodedPathHazards(string destination)
    {
        using var workspace = CreateWorkspace(destination);

        var first = await RunWithoutWritesAsync(workspace);
        var second = await RunWithoutWritesAsync(workspace);

        Assert.Equal(
            RouteListResultProjectionTestSupport.Create(first),
            RouteListResultProjectionTestSupport.Create(second));
        Assert.Equal(CliSemanticStatus.Blocked, first.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, first.Result.Coverage.State);
        var finding = Assert.Single(first.Result.Findings);
        Assert.Equal(RouteListFindingCodes.PhysicalEscape, finding.Code);
        Assert.Equal(destination, finding.Path);
    }

    [Theory(DisplayName = "Route-list Loader accepts literal separators encoded spaces Unicode and exactly one decode"),
     InlineData("root/_root.md", ".agents/root/_root.md", "root"),
     InlineData("root%2F_root.md", ".agents/root/_root.md", "root"),
     InlineData("project%20alpha/_project%20alpha.md", ".agents/project alpha/_project alpha.md", "project alpha"),
     InlineData("%E5%B7%A5%E4%BD%9C/_%E5%B7%A5%E4%BD%9C.md", ".agents/工作/_工作.md", "工作"),
     InlineData("encoded%2520name/_encoded%2520name.md", ".agents/encoded%20name/_encoded%20name.md", "encoded%20name"),
     Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task LoaderAcceptsCanonicalDecodedTargets(
        string destination,
        string canonicalPath,
        string canonicalId)
    {
        using var workspace = RouteListTestWorkspace.Create();
        workspace.WriteLoader($"- [Root]({destination}) - #Root");
        workspace.WriteRoute(canonicalPath, "Loader root", "Root");

        var result = await RunWithoutWritesAsync(workspace);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var row = Assert.Single(result.Result.Rows);
        Assert.Equal(canonicalId, row.Id);
        Assert.Equal(canonicalPath, row.Path);
    }

    private static RouteListTestWorkspace CreateWorkspace(string destination)
    {
        var workspace = RouteListTestWorkspace.Create();
        workspace.WriteLoader($"- [Root]({destination}) - #Root");
        workspace.WriteRoute(".agents/root/_root.md", "Root", "Root");
        return workspace;
    }

    private static async Task<RouteListResult> RunWithoutWritesAsync(RouteListTestWorkspace workspace)
    {
        var before = workspace.SnapshotHashes();
        var result = await RouteListOperation.RunAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);
        Assert.Equal(before, workspace.SnapshotHashes());
        return result;
    }
}
