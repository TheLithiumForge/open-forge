using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectOperationCancellationIntegrationTests
{
    [Fact(DisplayName = "Route inspect translates a pre-cancelled operation to an interrupted typed result without a profile")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task PreCancelledOperationIsInterrupted()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "Root")]);
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/_root.md",
                Description = "Root",
                Tags = ["Root"],
            });
        var before = workspace.Snapshot();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);
        cancellation.Cancel();

        var result = await workspace.InspectAsync("root", cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(RouteInspectSelectionMethod.Unresolved, result.Selection.SelectionMethod);
        Assert.Equal("root", result.Selection.RequestedReference);
        Assert.Null(result.Identity);
        Assert.Null(result.Profile);
        var condition = Assert.Single(result.Conditions);
        Assert.Equal(RouteInspectConditionCode.Interrupted, condition.Code);
        Assert.Equal(CliSemanticStatus.Interrupted, condition.Status);
        RouteInspectProfileIntegrationAssertions.AssertSemanticConditionAndNext(
            result,
            RouteInspectConditionCode.Interrupted);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }
}
