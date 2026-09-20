using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectOperationApplicabilityIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect reports detached local topology and LoadNow descendants without inventing Loader-rooted facts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task DetachedEntrypointRetainsLocalFactsOnly()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader([]);
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/detached/_detached.md",
                Description = "Detached",
                Tags = ["Detached"],
                Axioms = "- Detached local Axioms remain applicable.",
                Entries =
                [RouteInspectProfileIntegrationWorkspace.Entry("Local load", "local.md", "LoadNow")],
            });
        workspace.WriteRoutedMarkdown(
            ".agents/detached/local.md",
            "Local load",
            ["LoadNow"],
            "# Local load\n\nA detached local descendant.\n");
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync(
            ".agents/detached/_detached.md",
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal(RouteInspectRouteState.Detached, identity.RouteState);
        Assert.Equal(RouteInspectSourceKind.Entrypoint, identity.Kind);
        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        Assert.Equal(RouteInspectCompleteness.Complete, profile.Completeness);
        Assert.Equal(RouteInspectSafety.Safe, profile.Safety);

        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Reading.TaskStart);
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Reading.Automatic);
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Reading.Later);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.OwnSource,
            workspace,
            ".agents/detached/_detached.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.SelectedClosure,
            workspace,
            ".agents/detached/_detached.md",
            ".agents/detached/local.md");
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Measurements.TaskStartOverlap);
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Measurements.SelectionAddition);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.LoadNowDescendants,
            workspace,
            ".agents/detached/local.md");

        var topology = Assert.IsType<RouteInspectTopology>(profile.Topology.Value);
        var counts = Assert.IsType<RouteInspectTopologyCounts>(topology.Counts.Value);
        Assert.Equal("detached", topology.RootRoute);
        Assert.Equal(["detached"], topology.RouteChain);
        Assert.Null(topology.ParentId);
        Assert.Equal(1, topology.Depth);
        Assert.Equal(1, counts.DirectRoutedFileCount);
        Assert.Equal(0, counts.DirectEntrypointCount);
        Assert.Equal(1, counts.DescendantRoutedFileCount);
        Assert.Equal(0, counts.DescendantEntrypointCount);

        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        Assert.Equal(RouteInspectFactState.NotApplicable, axioms.Inherited.State);
        Assert.Equal(RouteInspectAxiomsLocalState.Substantive, axioms.Local.Value);
        Assert.Empty(result.Conditions);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect reports a known unrouted source with own bytes while all route-dependent facts remain not applicable")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task KnownUnroutedSourceRetainsOwnMeasurementOnly()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader([]);
        workspace.WriteRoutedMarkdown(
            ".agents/flat.md",
            "Flat",
            ["Flat"],
            "# Flat\n\n## Axioms\n\n- This ordinary heading is not an active route Axiom.\n");
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync(
            ".agents/flat.md",
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal(RouteInspectRouteState.NotRouted, identity.RouteState);
        Assert.Equal(RouteInspectSourceKind.Markdown, identity.Kind);
        Assert.Equal(RouteInspectSourceForm.Markdown, identity.Form);
        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        Assert.Equal(RouteInspectCompleteness.Complete, profile.Completeness);
        Assert.Equal(RouteInspectSafety.Safe, profile.Safety);

        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Reading.TaskStart);
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Reading.Automatic);
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Reading.Later);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.OwnSource,
            workspace,
            ".agents/flat.md");
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Measurements.SelectedClosure);
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Measurements.TaskStartOverlap);
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Measurements.SelectionAddition);
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Measurements.LoadNowDescendants);
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(profile.Topology);
        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        Assert.Equal(RouteInspectFactState.NotApplicable, axioms.Inherited.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, axioms.Local.State);
        Assert.Empty(result.Conditions);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }
}
