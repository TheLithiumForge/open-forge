using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class RouteMoveRevalidationIntegrationTests
{
    [Theory(DisplayName = "Route Move revalidation reobserves every volatile complete-plan fact"),
        InlineData("exact", (int)RouteMovePlanRevalidationState.Exact),
        InlineData("source-bytes", (int)RouteMovePlanRevalidationState.Changed),
        InlineData("reference-bytes", (int)RouteMovePlanRevalidationState.Changed),
        InlineData("generated-bytes", (int)RouteMovePlanRevalidationState.Changed),
        InlineData("ownership", (int)RouteMovePlanRevalidationState.Changed),
        InlineData("unrelated-ownership", (int)RouteMovePlanRevalidationState.Changed),
        InlineData("destination", (int)RouteMovePlanRevalidationState.Changed),
        InlineData("cancelled", (int)RouteMovePlanRevalidationState.Interrupted)]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task RevalidationRejectsEveryStaleObservation(
        string scenario,
        int expectedStateValue)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create($"move-revalidation-{scenario}");
        var builder = workspace.CreatePlanBuilder();
        var build = await builder.BuildAsync(
            workspace.Request(mode: OpenForge.Cli.Core.Commands.Route.Move.Models.Request.RouteMoveMode.Apply),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(build.Plan);
        await using var lease = await workspace.AcquireLeaseAsync(Guid.NewGuid());
        ApplyMutation(workspace, scenario);
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        if (scenario == "cancelled")
        {
            cancellation.Cancel();
        }

        var result = await new RouteMovePlanRevalidator(builder)
            .RevalidateAsync(plan, lease, cancellation.Token);

        Assert.Equal((RouteMovePlanRevalidationState)expectedStateValue, result.State);
        Assert.Equal(before, workspace.SnapshotHashes());
        if (scenario == "exact")
        {
            Assert.Null(result.Cause);
        }
        else
        {
            Assert.False(string.IsNullOrWhiteSpace(result.Cause));
        }
    }

    private static void ApplyMutation(
        RouteMoveIntegrationWorkspace workspace,
        string scenario)
    {
        switch (scenario)
        {
            case "exact":
            case "cancelled":
                return;
            case "source-bytes":
                workspace.WriteText(
                    RouteMoveIntegrationWorkspace.LeafPath,
                    workspace.ReadText(RouteMoveIntegrationWorkspace.LeafPath) + "Concurrent source edit.\n");
                return;
            case "reference-bytes":
                workspace.WriteText(
                    "README.md",
                    workspace.ReadText("README.md") + "Concurrent reference-source edit.\n");
                return;
            case "generated-bytes":
                workspace.WriteText(
                    ".agents/guidance/_guidance.md",
                    workspace.ReadText(".agents/guidance/_guidance.md") + "Concurrent generated-source edit.\n");
                return;
            case "ownership":
                workspace.SeedScenario("ownership-claim");
                return;
            case "unrelated-ownership":
                workspace.SeedScenario("unrelated-ownership-claim");
                return;
            case "destination":
                workspace.WriteText(
                    RouteMoveIntegrationWorkspace.LeafDestination,
                    "Concurrent destination occupant.\n");
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown revalidation scenario.");
        }
    }
}
