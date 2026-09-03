using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class RouteMoveSharedFoundationIntegrationTests
{
    [Theory(DisplayName = "Route Markdown catalogue covers explicit real roots exclusions extensions UTF-8 and cancellation"),
        InlineData("complete", (int)RouteMarkdownCatalogueCoverage.Complete),
        InlineData("explicit-exclusion", (int)RouteMarkdownCatalogueCoverage.Complete),
        InlineData("invalid-utf8", (int)RouteMarkdownCatalogueCoverage.Incomplete),
        InlineData("missing-root", (int)RouteMarkdownCatalogueCoverage.Incomplete),
        InlineData("cancelled", (int)RouteMarkdownCatalogueCoverage.Interrupted)]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationBehavior")]
    public async Task CatalogueUsesOneCompleteRealWorkspaceBoundary(
        string scenario,
        int expectedCoverageValue)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create($"move-catalogue-{scenario}");
        if (scenario == "invalid-utf8")
        {
            workspace.SeedScenario(scenario);
        }

        using var cancellation = new CancellationTokenSource();
        if (scenario == "cancelled")
        {
            cancellation.Cancel();
        }

        var request = new RouteMarkdownCatalogueRequest(
            workspace.Workspace,
            scenario == "missing-root" ? ["missing"] : ["."],
            scenario == "explicit-exclusion" ? [".agents"] : [],
            new RouteMarkdownCatalogueFilters([".md"]));

        var catalogue = await new RouteMarkdownCatalogueReader(new PhysicalPathResolver())
            .ReadAsync(request, cancellation.Token);

        Assert.Equal((RouteMarkdownCatalogueCoverage)expectedCoverageValue, catalogue.Coverage);
        if (scenario == "complete")
        {
            Assert.Contains("README.md", catalogue.SelectedPaths);
            Assert.Contains(RouteMoveIntegrationWorkspace.LeafPath, catalogue.SelectedPaths);
            Assert.Equal(
                catalogue.SelectedPaths.Order(StringComparer.Ordinal),
                catalogue.SelectedPaths);
        }
        else if (scenario == "explicit-exclusion")
        {
            Assert.Contains("README.md", catalogue.SelectedPaths);
            Assert.DoesNotContain(catalogue.SelectedPaths, path => path.StartsWith(".agents/", StringComparison.Ordinal));
        }
    }

    [Fact(DisplayName = "Route Markdown catalogue blocks a real Linux filename that has no canonical path representation")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task CatalogueBlocksUnrepresentableLinuxFilesystemName()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        using var workspace = RouteMoveIntegrationWorkspace.Create("move-catalogue-unrepresentable-linux-name");
        const string unsafeName = "unsafe\\name.md";
        workspace.WriteText(unsafeName, "# Unsafe filesystem name\n");
        Assert.True(File.Exists(workspace.Absolute(unsafeName)));

        var catalogue = await new RouteMarkdownCatalogueReader(new PhysicalPathResolver())
            .ReadAsync(
                new RouteMarkdownCatalogueRequest(
                    workspace.Workspace,
                    ["."],
                    [],
                    new RouteMarkdownCatalogueFilters([".md"])),
                TestContext.Current.CancellationToken);

        Assert.Equal(RouteMarkdownCatalogueCoverage.Blocked, catalogue.Coverage);
        var finding = Assert.Single(
            catalogue.Findings,
            value => value.Code == RouteMarkdownCatalogueFindingCode.MarkdownPathUnsafe);
        Assert.Equal(".", finding.Path);
        Assert.DoesNotContain(unsafeName, catalogue.SelectedPaths);
    }

    [Theory(DisplayName = "Lifecycle ownership reads Framework and Extensions from one exact snapshot"),
        InlineData("trusted", (int)LifecycleOwnershipReadState.Trusted, 1),
        InlineData("claimed", (int)LifecycleOwnershipReadState.Trusted, 2),
        InlineData("missing", (int)LifecycleOwnershipReadState.Blocked, 0),
        InlineData("malformed", (int)LifecycleOwnershipReadState.Blocked, 0),
        InlineData("stale", (int)LifecycleOwnershipReadState.Blocked, 0),
        InlineData("incomplete", (int)LifecycleOwnershipReadState.Blocked, 0),
        InlineData("conflicting", (int)LifecycleOwnershipReadState.Blocked, 0),
        InlineData("cancelled", (int)LifecycleOwnershipReadState.Interrupted, 0)]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task OwnershipRequiresOneTrustedCombinedSnapshot(
        string scenario,
        int expectedStateValue,
        int expectedClaims)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create($"move-ownership-{scenario}");
        if (scenario == "claimed")
        {
            workspace.SeedScenario("ownership-claim");
        }
        else if (scenario == "missing")
        {
            workspace.DeleteFile(RouteMoveIntegrationWorkspace.LifecyclePath);
        }
        else if (scenario == "malformed")
        {
            workspace.SeedScenario("ownership-malformed");
        }
        else if (scenario is "stale" or "incomplete" or "conflicting")
        {
            workspace.SeedScenario($"ownership-{scenario}");
        }

        using var cancellation = new CancellationTokenSource();
        if (scenario == "cancelled")
        {
            cancellation.Cancel();
        }

        var before = workspace.SnapshotHashes();
        var result = await new LifecycleOwnershipReader(new PhysicalPathResolver())
            .ReadAsync(workspace.Workspace, cancellation.Token);

        Assert.Equal((LifecycleOwnershipReadState)expectedStateValue, result.Framework.State);
        Assert.Equal((LifecycleOwnershipReadState)expectedStateValue, result.Extensions.State);
        Assert.Equal(expectedClaims, result.Claims.Length);
        Assert.Equal(before, workspace.SnapshotHashes());
        if (scenario == "claimed")
        {
            var claim = Assert.Single(
                result.Claims,
                value => value.Path == RouteMoveIntegrationWorkspace.LeafPath);
            Assert.Equal(RouteMoveIntegrationWorkspace.LeafPath, claim.Path);
            Assert.Equal(LifecycleOwnershipManager.Extension, claim.Manager);
        }
        else if (scenario == "trusted")
        {
            var claim = Assert.Single(result.Claims);
            Assert.Equal(".agents/loader.md", claim.Path);
            Assert.Equal(LifecycleOwnershipManager.Framework, claim.Manager);
        }
    }
}
