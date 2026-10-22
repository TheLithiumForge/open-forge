using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Exclude;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Exclude;

public sealed class RouteExcludeOutputSnapshotTests
{
    [Fact(DisplayName = "RouteExclude previews preserve text and JSON at every detail level"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    public async Task Preview()
    {
        using var workspace = new RouteInclusionWorkspace("- [Guide](../guide.md) - #Docs");
        var result = await new RouteExcludeOperation().ExecuteAsync(new RouteExcludeRequest
        {
            Workspace = workspace.Workspace, File = "guide.md", DryRun = true,
        }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        CommandOutputRenderers<RouteExcludeResult>.From(RouteExcludePresentation.Rendering,
            normalizeText: text => text.Replace("\\r\\n", "\\n", StringComparison.Ordinal),
            normalizeJson: text => text.Replace("\\r\\n", "\\n", StringComparison.Ordinal)).MatchDetails(result, "preview");
    }
}
