using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Create;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

[Trait("Feature", "route-create"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
public sealed class RouteCreateFrontmatterOutputSnapshotTests
{
    [Fact(DisplayName = "Route Create previews root metadata and new intermediate entrypoints")]
    public async Task RootFormPreview()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-root-output");
        workspace.SeedBase();
        workspace.WriteText(".agents/open-forge.json", "{\"frontmatter\":\"root\"}");
        var request = new RouteCreateRequest(
            workspace: workspace.Workspace,
            fileTarget: "memory/project-alpha/team/overview",
            metadata: workspace.Request().Metadata,
            templateReference: null,
            mode: RouteCreateMode.DryRun);
        var before = workspace.SnapshotHashes();

        var result = await RouteCreateOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        CommandOutputRenderers<RouteCreateResult>.From(RouteCreatePresentation.Rendering).MatchDetails(result, "root-form-preview");
    }
}
