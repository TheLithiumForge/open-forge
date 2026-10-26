using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Exclude;

public sealed partial class RouteExcludeApplicationTests
{
    [Theory(DisplayName = "Exclude applies to hand-written rows, missing targets and main-root catalogues"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    [InlineData("guide.md", false)]
    [InlineData("guide.md", true)]
    [InlineData(".agents/patterns/_patterns.md", false)]
    public async Task Apply(string file, bool missing)
    {
        var root = file.StartsWith(".agents/", StringComparison.Ordinal);
        using var workspace = new RouteInclusionWorkspace(root ? "- [Patterns](patterns/_patterns.md) - #Pattern" : "- [Guide](../guide.md)");
        if (root) workspace.AddRoot();
        if (missing) File.Delete(workspace.Files.Combine(file));
        var target = missing ? null : workspace.Bytes(file);
        var result = await new RouteExcludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(new RouteExcludeRequest
        { Workspace = workspace.Workspace, File = file }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.Outcome.Verified);
        Assert.Null(result.Outcome.After);
        if (!missing) Assert.Equal(target, workspace.Bytes(file));
    }
}
