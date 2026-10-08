using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Init;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Init.Generic;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

[Trait("Feature", "route-init"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
public sealed class RouteInitFrontmatterOutputSnapshotTests
{
    [Fact(DisplayName = "Route Init previews generic scaffolds in root form")]
    public async Task RootFormPreview()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-root-output");
        workspace.WriteText(".agents/open-forge.json", "{\"frontmatter\":\"root\"}");
        var before = workspace.SnapshotHashes();

        var result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            workspace.Request("documents/design", RouteInitMode.DryRun), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        CommandOutputRenderers<RouteInitResult>.From(RouteInitPresentation.Rendering).MatchDetails(result, "root-form-preview");
    }
}
