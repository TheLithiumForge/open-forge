using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Exclude;

public sealed class RouteExcludePlanningTests
{
    [Theory(DisplayName = "Exclude previews removal of matching rows including missing and hand-written targets"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task RemovesRows(bool registered, bool missing)
    {
        using var workspace = new RouteInclusionWorkspace("- [Guide](../guide.md)\n- [Again](../guide.md) - #Docs\n- [Other](../other.md)");
        workspace.Files.WriteText("other.md", "other\n");
        if (registered) workspace.Files.ReplaceText(".agents/open-forge.json", "{\"roots\":[\"guide.md\"]}\n");
        if (missing) File.Delete(workspace.Files.Combine("guide.md"));
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteExcludeOperation().ExecuteAsync(new RouteExcludeRequest
        { Workspace = workspace.Workspace, File = "guide.md", DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Null(result.Outcome.After);
        var loader = Assert.Single(result.Outcome.Effects.Where(effect => effect.Path == ".agents/loader.md"));
        Assert.DoesNotContain("../guide.md", loader.After, StringComparison.Ordinal);
        Assert.Contains("- [Other](../other.md)", loader.After, StringComparison.Ordinal);
        Assert.Equal(registered, result.Outcome.RegisteredBefore);
        Assert.False(result.Outcome.RegisteredAfter);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }

    [Fact(DisplayName = "Exclude turns a main-root catalogue off without changing it"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    public async Task MainRoot()
    {
        using var workspace = new RouteInclusionWorkspace("- [Patterns](patterns/_patterns.md) - #Pattern");
        workspace.AddRoot();
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteExcludeOperation().ExecuteAsync(new RouteExcludeRequest
        { Workspace = workspace.Workspace, File = ".agents/patterns/_patterns.md", DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Null(result.Outcome.After);
        Assert.DoesNotContain("patterns/_patterns.md", Assert.Single(result.Outcome.Effects).After, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }

    [Fact(DisplayName = "Exclude with nothing listed or registered returns successful no-change JSON"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    public async Task NothingToDo()
    {
        using var workspace = new RouteInclusionWorkspace();
        var before = workspace.Files.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(["route", "exclude", "missing.md", "--format", "json"], workspace.Files.Path);
        Assert.Equal(0, result.ExitCode);
        using var json = JsonDocument.Parse(result.Output);
        Assert.Equal("completed", json.RootElement.GetProperty("status").GetString());
        Assert.Empty(json.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(workspace.Locks.InfrastructureExists);
    }
}
