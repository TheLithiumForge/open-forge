using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Route.Shared.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class RouteMoveSharedFoundationIntegrationTests
{
    [Trait("Boundary", "OS")]
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

    [Trait("Boundary", "OS")]
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

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route ownership reads current path and region claims from one exact lock snapshot"),
        InlineData("trusted", true, 0),
        InlineData("claimed", true, 1),
        InlineData("missing", false, 0),
        InlineData("malformed", false, 0),
        InlineData("old-metadata", true, 0),
        InlineData("unknown", false, 0),
        InlineData("conflicting", true, 2)]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task OwnershipUsesOneCurrentSnapshot(string scenario, bool established, int expectedClaims)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create($"move-ownership-{scenario}");
        if (scenario == "claimed") workspace.SeedScenario("ownership-claim");
        else if (scenario == "missing") workspace.DeleteFile(RouteMoveIntegrationWorkspace.OwnershipPath);
        else if (scenario != "trusted") workspace.SeedScenario($"ownership-{scenario}");
        var before = workspace.SnapshotHashes();
        var result = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, CancellationToken.None);
        Assert.Equal(established, RouteOwnershipEvidence.IsEstablished(result));
        Assert.Equal(expectedClaims, RouteOwnershipEvidence.Claims(result).Count());
        Assert.Equal(before, workspace.SnapshotHashes());
        if (scenario == "claimed")
        {
            var claim = Assert.Single(RouteOwnershipEvidence.Claims(result));
            Assert.Equal(RouteMoveIntegrationWorkspace.LeafPath, claim.Path);
            Assert.Equal(OwnedPathManager.Extension, claim.Manager);
        }
    }
}
