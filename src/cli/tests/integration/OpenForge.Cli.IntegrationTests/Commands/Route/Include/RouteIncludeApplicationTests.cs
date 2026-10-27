using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Settings;
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

public sealed partial class RouteIncludeApplicationTests
{
    private const string GuidePath = "guide.md";
    private const string GuideDescription = "Guide";
    private const string NewGuideDescription = "New guide";
    private const string SourcePattern = "src/**";

    [Theory(DisplayName = "Include then Index keeps the loader and included bytes unchanged, then Exclude preserves the target"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IncludeIndexExclude(bool metadata)
    {
        using var workspace = new RouteInclusionWorkspace();
        var bytes = workspace.Bytes(GuidePath);
        var request = new RouteIncludeRequest
        {
            Workspace = workspace.Workspace,
            File = GuidePath,
            Description = GuideDescription,
            Tags = metadata ? ["LoadNow", "Docs"] : null,
            ApplyTo = metadata ? ApplyToPatternExpressionParser.Parse(SourcePattern).Patterns : null,
        };
        var operation = new RouteIncludeOperation(workspace.Locks.StoreRoot);
        var included = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, included.Status);
        Assert.True(included.Outcome.Verified);
        var loader = workspace.Bytes(SourceLogicalPath.LoaderPath);
        Assert.Equal(bytes, workspace.Bytes(GuidePath));
        var index = await CliHostCapture.RunAsync([PublishedTokens.Index.Command], workspace.Files.Path);
        Assert.Equal(0, index.ExitCode);
        Assert.Equal(loader, workspace.Bytes(SourceLogicalPath.LoaderPath));
        Assert.Equal(bytes, workspace.Bytes(GuidePath));
        var repeated = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.True(repeated.Outcome.NoOp);
        var updated = await operation.ExecuteAsync(request with { Description = NewGuideDescription }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, updated.Status);
        if (metadata) CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(updated.Outcome.After), $"{nameof(IncludeIndexExclude)}-{"updated-row"}.content", testName: nameof(RouteIncludeApplicationTests));
        var excluded = await new RouteExcludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(new RouteExcludeRequest
        { Workspace = workspace.Workspace, File = GuidePath }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, excluded.Status);
        Assert.Empty(SourceLoaderEntriesParser.Parse(WorkspaceLayout.Create(workspace.Workspace, WorkspaceLayout.DefaultMainRoot), workspace.Read(SourceLogicalPath.LoaderPath)).Destinations);
        using var settings = JsonDocument.Parse(workspace.Read(WorkspaceSettingsDefinitions.RelativePath));
        Assert.DoesNotContain(GuidePath, settings.RootElement.GetProperty(PublishedTokens.Settings.RootsProperty).EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(bytes, workspace.Bytes(GuidePath));
    }

    [Fact(DisplayName = "Include registers an existing hand-written row without rewriting the loader"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task HandWrittenRow()
    {
        using var workspace = new RouteInclusionWorkspace("- [Existing](../guide.md) - #Docs");
        var loader = workspace.Bytes(SourceLogicalPath.LoaderPath);
        var result = await new RouteIncludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = GuidePath }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(loader, workspace.Bytes(SourceLogicalPath.LoaderPath));
        Assert.Equal(WorkspaceSettingsDefinitions.RelativePath, Assert.Single(result.Outcome.Effects).Path);
    }
}
