using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMovePlanningProjectionTests
{
    [Fact(DisplayName = "Route Move unresolved exact source path retains base-path selection origin")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationProjection"), Trait("Boundary", "OS")]
    public async Task UnresolvedExactSourcePathRetainsSelectionOrigin()
    {
        using var temporary = TemporaryWorkspace.Create("route-move-missing-source");
        _ = temporary.CreateDirectory(".agents");
        var workspace = new CliWorkspace(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new RouteMoveRequest(
            workspace,
            ".agents/guidance/consumed.md",
            ".agents/archive/consumed.md",
            RouteMoveMode.DryRun);

        var result = await RouteMoveOperationFactory.Create()
            .ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(RouteMoveSourceSelection.BasePath, result.Source.SelectedBy);
        Assert.Null(result.Source.Id);
        Assert.Null(result.Source.Path);
        Assert.Null(result.Source.Form);
    }
}
