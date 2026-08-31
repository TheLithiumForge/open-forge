using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Generic;

public sealed class GenericRouteInitResidualIntegrationTests
{
    [Fact(DisplayName = "Generic Route Init retains parent-first directories after cancellation during later effects"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task LaterCancellationLeavesVerifiedDirectoryResiduals()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-directory-residual");
        var routeTarget = string.Join(
            "/",
            Enumerable.Range(0, 96).Select(index => $"s{index:D3}"));
        var firstDescendant = workspace.Absolute(".agents/s000");
        var finalPath = $".agents/{routeTarget}/_s095.md";
        using var cancellation = new CancellationTokenSource();
        using var monitorStop = new CancellationTokenSource();
        var monitor = Task.Run(
            () =>
            {
                while (!monitorStop.IsCancellationRequested)
                {
                    if (Directory.Exists(firstDescendant))
                    {
                        cancellation.Cancel();
                        return;
                    }

                    Thread.Yield();
                }
            },
            TestContext.Current.CancellationToken);

        RouteInitResult? result = null;
        try
        {
            result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
                .ExecuteAsync(
                    workspace.Request(routeTarget),
                    cancellation.Token);
        }
        finally
        {
            monitorStop.Cancel();
            await monitor;
        }

        Assert.NotNull(result);
        Assert.Equal(CliSemanticStatus.Interrupted, result!.Status);
        Assert.Contains(
            result.Effects,
            effect => effect.Kind == RouteInitEffectKind.Directory
                && effect.Residual == RouteInitEffectResidual.Retained);
        Assert.True(Directory.Exists(workspace.Absolute(".agents")));
        Assert.True(Directory.Exists(firstDescendant));
        Assert.False(workspace.Exists(finalPath));
    }
}
