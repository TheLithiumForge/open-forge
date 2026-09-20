using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemoveRevalidationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove refuses a held workspace lock before observing effects"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task ContendedWorkspaceLockIsWriteFree()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-lock-race");
        await using var lease = await workspace.AcquireLeaseAsync(Guid.NewGuid());
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--automatic"],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains(
            "Cannot remove guidance/old guide: Another Open Forge command holds the workspace lock. Nothing was changed.",
            error.ToString(),
            StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove revalidates generated navigation before applying a stale plan"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task ChangedGeneratedRegionBlocksApplicationWithoutWrites()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-generated-race");
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.LeafId,
                RouteRemoveMode.Apply),
            TestContext.Current.CancellationToken);
        var original = Assert.IsType<RouteRemovePlan>(build.Plan);
        await using var lease = await workspace.AcquireLeaseAsync(Guid.NewGuid());

        workspace.WriteText(
            RouteRemoveIntegrationWorkspace.ParentPath,
            workspace.ReadText(RouteRemoveIntegrationWorkspace.ParentPath)
                .Replace(
                    "- [Old guide](old%20guide.md) - #Guide",
                    "- [Changed guide](old%20guide.md) - #Guide",
                    StringComparison.Ordinal));
        var beforeRevalidation = workspace.SnapshotHashes();

        var revalidation = await RouteRemoveOperationFactory.CreatePlanRevalidator().RevalidateAsync(
            original,
            lease,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteRemovePlanRevalidationState.Changed, revalidation.State);
        Assert.False(string.IsNullOrWhiteSpace(revalidation.Cause));
        Assert.Equal(beforeRevalidation, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove revalidates the live target instead of reusing an old snapshot"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task ChangedSubjectBytesBlockApplicationWithoutWrites()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-subject-race");
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.LeafId,
                RouteRemoveMode.Apply),
            TestContext.Current.CancellationToken);
        var original = Assert.IsType<RouteRemovePlan>(build.Plan);
        await using var lease = await workspace.AcquireLeaseAsync(Guid.NewGuid());

        workspace.WriteText(
            RouteRemoveIntegrationWorkspace.LeafPath,
            workspace.ReadText(RouteRemoveIntegrationWorkspace.LeafPath)
                + "\nConcurrent subject edit.\n");
        var beforeRevalidation = workspace.SnapshotHashes();

        var revalidation = await RouteRemoveOperationFactory.CreatePlanRevalidator().RevalidateAsync(
            original,
            lease,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteRemovePlanRevalidationState.Changed, revalidation.State);
        Assert.False(string.IsNullOrWhiteSpace(revalidation.Cause));
        Assert.Equal(beforeRevalidation, workspace.SnapshotHashes());
    }
}
