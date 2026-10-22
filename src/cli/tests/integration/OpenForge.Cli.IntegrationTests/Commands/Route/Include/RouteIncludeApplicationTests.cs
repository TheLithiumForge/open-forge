using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Include;

public sealed class RouteIncludeApplicationTests
{
    [Theory(DisplayName = "Include then Index keeps the loader and included bytes unchanged, then Exclude preserves the target"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IncludeIndexExclude(bool metadata)
    {
        using var workspace = new RouteInclusionWorkspace();
        var bytes = workspace.Bytes("guide.md");
        var request = new RouteIncludeRequest
        {
            Workspace = workspace.Workspace,
            File = "guide.md",
            Description = "Guide",
            Tags = metadata ? ["LoadNow", "Docs"] : null,
            ApplyTo = metadata ? ApplyToPatternExpressionParser.Parse("src/**").Patterns : null,
        };
        var operation = new RouteIncludeOperation(workspace.Locks.StoreRoot);
        var included = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, included.Status);
        Assert.True(included.Outcome.Verified);
        var loader = workspace.Bytes(".agents/loader.md");
        Assert.Equal(bytes, workspace.Bytes("guide.md"));
        var index = await CliHostCapture.RunAsync(["index"], workspace.Files.Path);
        Assert.Equal(0, index.ExitCode);
        Assert.Equal(loader, workspace.Bytes(".agents/loader.md"));
        Assert.Equal(bytes, workspace.Bytes("guide.md"));
        var repeated = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.True(repeated.Outcome.NoOp);
        var updated = await operation.ExecuteAsync(request with { Description = "New guide" }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, updated.Status);
        if (metadata) Assert.Equal("- [New guide](../guide.md) - #LoadNow #Docs - applies to \u0060src/**\u0060", updated.Outcome.After);
        var excluded = await new RouteExcludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(new RouteExcludeRequest
        { Workspace = workspace.Workspace, File = "guide.md" }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, excluded.Status);
        Assert.DoesNotContain("../guide.md", workspace.Read(".agents/loader.md"), StringComparison.Ordinal);
        using var settings = JsonDocument.Parse(workspace.Read(".agents/open-forge.json"));
        Assert.DoesNotContain("guide.md", settings.RootElement.GetProperty("roots").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(bytes, workspace.Bytes("guide.md"));
    }

    [Fact(DisplayName = "Include registers an existing hand-written row without rewriting the loader"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task HandWrittenRow()
    {
        using var workspace = new RouteInclusionWorkspace("- [Existing](../guide.md) - #Docs");
        var loader = workspace.Bytes(".agents/loader.md");
        var result = await new RouteIncludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = "guide.md" }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(loader, workspace.Bytes(".agents/loader.md"));
        Assert.Equal(".agents/open-forge.json", Assert.Single(result.Outcome.Effects).Path);
    }
}
