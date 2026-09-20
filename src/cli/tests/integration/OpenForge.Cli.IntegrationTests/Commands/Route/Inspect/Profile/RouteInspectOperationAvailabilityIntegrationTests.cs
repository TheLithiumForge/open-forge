using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectOperationAvailabilityIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect keeps own bytes and local Axioms when a visible LoadNow body is unreadable")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task UnreadableDescendantDoesNotCollapseIndependentFacts()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "LoadNow", "Root")]);
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/_root.md",
                Description = "Root",
                Tags = ["Root"],
                Axioms = "- Root local Axioms remain readable.",
                Entries =
                [RouteInspectProfileIntegrationWorkspace.Entry("Unreadable", "unreadable.md", "LoadNow")],
            });
        workspace.Write(".agents/root/unreadable.md", [0xC3, 0x28]);
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync("root", TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        Assert.Equal(RouteInspectCompleteness.Incomplete, profile.Completeness);
        Assert.Equal(RouteInspectSafety.Safe, profile.Safety);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.OwnSource,
            workspace,
            ".agents/root/_root.md");
        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        Assert.Equal(RouteInspectAxiomsLocalState.Substantive, axioms.Local.Value);
        Assert.Equal(RouteInspectFactState.Value, profile.Topology.State);
        var topology = Assert.IsType<RouteInspectTopology>(profile.Topology.Value);
        var counts = Assert.IsType<RouteInspectTopologyCounts>(topology.Counts.Value);
        Assert.Equal(1, counts.DirectRoutedFileCount);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Measurements.SelectedClosure);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Measurements.TaskStartOverlap);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Measurements.SelectionAddition);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Measurements.LoadNowDescendants);
        RouteInspectProfileIntegrationAssertions.AssertSemanticConditionAndNext(
            result,
            RouteInspectConditionCode.UnavailableFact);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect keeps local Axioms and topology when source metadata is unavailable instead of treating the source as on demand")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task MissingMetadataLeavesLoadingFactsUnavailable()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "LoadNow", "Root")]);
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/_root.md",
                Description = "Root",
                Tags = ["Root"],
                Axioms = "- Root Axioms establish the selected route.",
            });
        var selectedBody = RouteInspectProfileIntegrationWorkspace.BuildBody(
            "Selected",
            "- Local Axioms are readable without frontmatter metadata.",
            [RouteInspectProfileIntegrationWorkspace.Entry("Load", "load.md", "LoadNow")]);
        workspace.Write(".agents/root/selected/_selected.md", selectedBody);
        workspace.WriteRoutedMarkdown(
            ".agents/root/selected/load.md",
            "Load",
            ["LoadNow"],
            "# Load\n\nA readable child.\n");
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync(
            "root/selected",
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        Assert.Equal(RouteInspectCompleteness.Incomplete, profile.Completeness);
        Assert.Equal(RouteInspectSafety.Safe, profile.Safety);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.OwnSource,
            workspace,
            ".agents/root/selected/_selected.md");
        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        var inherited = Assert.IsType<RouteInspectAxiomsSources>(axioms.Inherited.Value);
        Assert.Equal(["root"], inherited.SourceIds);
        Assert.Equal(RouteInspectAxiomsLocalState.Substantive, axioms.Local.Value);
        Assert.Equal(RouteInspectFactState.Value, profile.Topology.State);
        Assert.Equal(
            ["root", "selected"],
            Assert.IsType<RouteInspectTopology>(profile.Topology.Value).RouteChain);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Reading.TaskStart);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Reading.Automatic);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Reading.Later);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.SelectedClosure,
            workspace,
            ".agents/root/_root.md",
            ".agents/root/selected/_selected.md",
            ".agents/root/selected/load.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.TaskStartOverlap,
            workspace,
            ".agents/root/_root.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.SelectionAddition,
            workspace,
            ".agents/root/selected/_selected.md",
            ".agents/root/selected/load.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.LoadNowDescendants,
            workspace,
            ".agents/root/selected/load.md");
        RouteInspectProfileIntegrationAssertions.AssertSemanticConditionAndNext(
            result,
            RouteInspectConditionCode.UnavailableFact);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect keeps own bytes, local Axioms, and topology when generated Entries are unavailable")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task MissingEntriesLeaveLoadingMeasurementsUnavailable()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "LoadNow", "Root")]);
        workspace.Write(
            ".agents/root/_root.md",
            RouteInspectProfileIntegrationWorkspace.OpenForgeMetadata("Root", "Root")
            + "# Root\n\n"
            + "## Axioms\n\n"
            + "- Root Axioms remain substantive.\n");
        workspace.WriteRoutedMarkdown(
            ".agents/root/load.md",
            "Load",
            ["LoadNow"],
            "# Load\n\nA source hidden by the unavailable Entries section.\n");
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync("root", TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.OwnSource,
            workspace,
            ".agents/root/_root.md");
        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        Assert.Equal(RouteInspectAxiomsLocalState.Substantive, axioms.Local.Value);
        Assert.Equal(RouteInspectFactState.Value, profile.Topology.State);
        var topology = Assert.IsType<RouteInspectTopology>(profile.Topology.Value);
        var counts = Assert.IsType<RouteInspectTopologyCounts>(topology.Counts.Value);
        Assert.Equal(1, counts.DirectRoutedFileCount);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Measurements.SelectedClosure);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Measurements.TaskStartOverlap);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Measurements.SelectionAddition);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Measurements.LoadNowDescendants);
        RouteInspectProfileIntegrationAssertions.AssertSemanticConditionAndNext(
            result,
            RouteInspectConditionCode.UnavailableFact);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }
}
