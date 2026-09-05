using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemovePlanningIntegrationTests
{
    [Fact(DisplayName = "Route Remove dry-run plans a leaf pair and performs no persistent effect"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task DryRunPlansLeafPairWithoutWrites()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-leaf-dry-run");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--dry-run", "--json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using var document = JsonDocument.Parse(output.ToString());
        var root = document.RootElement;
        Assert.Equal("route remove", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        var result = root.GetProperty("result");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal("leaf", result.GetProperty("subject").GetProperty("kind").GetString());
        Assert.Contains(
            result.GetProperty("subject").GetProperty("layers").EnumerateArray(),
            layer => layer.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.LeafPath);
        Assert.Contains(
            result.GetProperty("subject").GetProperty("layers").EnumerateArray(),
            layer => layer.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.LeafOverwritePath);
        Assert.Contains(
            result.GetProperty("references").GetProperty("detachments").EnumerateArray(),
            detachment => detachment.GetProperty("visibleLabel").GetString() == "Old guide");
        Assert.Contains(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == RouteRemoveIntegrationWorkspace.LeafPath);
        Assert.Equal("not-created", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Route Remove category dry-run inventories every contained regular item and generated consequence"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task DryRunInventoriesCompleteCategoryAndProjection()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-category-dry-run");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.CategoryId, "--dry-run", "--json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using var document = JsonDocument.Parse(output.ToString());
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("category", result.GetProperty("subject").GetProperty("kind").GetString());
        var items = result.GetProperty("subject").GetProperty("items").EnumerateArray().ToArray();
        Assert.Contains(items, item => item.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.CategoryPath);
        Assert.Contains(items, item => item.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.CategoryChildPath);
        Assert.Contains(items, item => item.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.CategoryNotesPath);
        Assert.Contains(items, item => item.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.CategoryResourcePath);
        Assert.Contains(
            result.GetProperty("generatedNavigation").GetProperty("regions").EnumerateArray(),
            region => region.GetProperty("reasons").EnumerateArray().Any(reason => reason.GetString() == "old-parent"));
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Route Remove projects a Loader-exposed category through the Loader region with exact bounded bytes"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationNavigation")]
    public async Task LoaderProjectionRetainsExactBeforeAndExpectedRegionBytes()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-loader-projection");
        var build = await RouteRemovePlanBuilder.Create().BuildAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.LoaderCategoryId,
                RouteRemoveMode.Apply),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteRemovePlan>(build.Plan);

        var region = Assert.Single(
            plan.Preview.GeneratedNavigation.Regions,
            value => value.Path == RouteRemoveIntegrationWorkspace.LoaderPath);
        Assert.Equal(RouteRemoveGeneratedState.Changed, region.State);
        Assert.Equal([RouteRemoveGeneratedReason.Loader], region.Reasons);

        var edit = Assert.Single(
            plan.Projection.Navigation.DocumentEdits,
            value => value.LogicalPath == workspace.Combine(RouteRemoveIntegrationWorkspace.LoaderPath));
        var beforeBytes = Encoding.UTF8.GetBytes(
            "\n- [Guidance](guidance/_guidance.md) - #Route\n- [Loader topics](loader-topics/_loader-topics.md) - #Route\n");
        var expectedBytes = Encoding.UTF8.GetBytes(
            "\n- [Guidance](guidance/_guidance.md) - #Route\n");
        Assert.Equal(beforeBytes, edit.BeforeBytes);
        Assert.Equal(expectedBytes, edit.ExpectedBytes);
        Assert.Equal(FileExpectation.Hash(beforeBytes), FileExpectation.Hash(edit.BeforeBytes.AsSpan()));
        Assert.Equal(FileExpectation.Hash(expectedBytes), FileExpectation.Hash(edit.ExpectedBytes.AsSpan()));
        Assert.Equal(
            FileExpectation.Hash(Encoding.UTF8.GetBytes(workspace.ReadText(RouteRemoveIntegrationWorkspace.LoaderPath))),
            FileExpectation.Hash(edit.Snapshot.Bytes.AsSpan()));
    }

    [Theory(DisplayName = "Route Remove refuses missing or claimed ownership before effects"),
     InlineData("missing", "blocked", "route-remove.ownership-unavailable"),
     InlineData("framework-claim", "blocked", "route-remove.ownership-claimed")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task OwnershipBoundariesAreWriteFree(
        string scenario,
        string expectedStatus,
        string expectedCode)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create($"route-remove-ownership-{scenario}");
        if (scenario == "missing")
        {
            workspace.RemoveLifecycle();
        }
        else
        {
            workspace.SeedFrameworkClaim(RouteRemoveIntegrationWorkspace.LeafPath);
        }

        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId],
            output,
            error);

        var primary = expectedStatus == "incomplete" ? output.ToString() : error.ToString();
        Assert.Equal(expectedStatus == "incomplete" ? 3 : 5, completion.ExitCode);
        Assert.Equal(expectedStatus, completion.Status switch
        {
            CliSemanticStatus.Incomplete => "incomplete",
            CliSemanticStatus.Blocked => "blocked",
            _ => completion.Status.ToString().ToLowerInvariant(),
        });
        Assert.Contains(expectedCode, primary, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }
}
