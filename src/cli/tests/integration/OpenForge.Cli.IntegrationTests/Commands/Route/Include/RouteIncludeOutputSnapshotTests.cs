using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Include;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Include;

public sealed class RouteIncludeOutputSnapshotTests
{
    [Fact(DisplayName = "RouteInclude previews preserve text and JSON at every detail level"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task Preview()
    {
        using var workspace = new RouteInclusionWorkspace();
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        {
            Workspace = workspace.Workspace,
            File = "guide.md",
            DryRun = true,
            Description = "Guide",
            Tags = ["LoadNow", "Docs"],
        }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        CommandOutputRenderers<RouteIncludeResult>.From(RouteIncludePresentation.Rendering,
            normalizeText: text => text.Replace("\\r\\n", "\\n", StringComparison.Ordinal),
            normalizeJson: text => text.Replace("\\r\\n", "\\n", StringComparison.Ordinal)).MatchDetails(result, "preview");
    }
}
