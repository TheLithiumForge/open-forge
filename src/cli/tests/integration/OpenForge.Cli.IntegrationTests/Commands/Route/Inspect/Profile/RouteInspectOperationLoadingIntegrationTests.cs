using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectOperationLoadingIntegrationTests
{
    [Fact(DisplayName = "Route inspect separates startup overlap and selection addition while keeping reordered and stale Entries out of narrow LoadNow descendants")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task StartupAndSelectedClosuresRespectAuthoredTopologyAndVisibleEntries()
    {
        using var workspace = CreateLoadingWorkspace();
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync(
            "root/selected",
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal("root/selected", identity.Id);
        Assert.Equal(RouteInspectRouteState.Routed, identity.RouteState);
        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);

        Assert.Equal(RouteInspectFactState.Value, profile.Reading.TaskStart.State);
        Assert.False(Assert.IsType<bool>(profile.Reading.TaskStart.Value));
        var automatic = Assert.IsType<RouteInspectAutomaticReadings>(profile.Reading.Automatic.Value);
        var automaticReason = Assert.Single(automatic.Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.EntrypointKeepInMind, automaticReason.Kind);
        Assert.Equal(
            [
                RouteInspectAutomaticReadingEvent.RouteSelected,
                RouteInspectAutomaticReadingEvent.ScopeSelected,
            ],
            automaticReason.Events);
        var later = Assert.IsType<RouteInspectLaterReading>(profile.Reading.Later.Value);
        Assert.True(later.MayBeReadAgain);
        Assert.Equal(
            [
                RouteInspectLaterReadOccasion.ContextRestoration,
                RouteInspectLaterReadOccasion.Handoff,
                RouteInspectLaterReadOccasion.Closeout,
                RouteInspectLaterReadOccasion.FollowupTransition,
            ],
            later.Occasions);

        var topology = Assert.IsType<RouteInspectTopology>(profile.Topology.Value);
        Assert.Equal("root", topology.RootRoute);
        Assert.Equal(["root", "selected"], topology.RouteChain);
        Assert.Equal("root", topology.ParentId);
        Assert.Equal(2, topology.Depth);
        var counts = Assert.IsType<RouteInspectTopologyCounts>(topology.Counts.Value);
        Assert.Equal(3, counts.DirectRoutedFileCount);
        Assert.Equal(0, counts.DirectEntrypointCount);
        Assert.Equal(3, counts.DescendantRoutedFileCount);
        Assert.Equal(0, counts.DescendantEntrypointCount);

        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        var inherited = Assert.IsType<RouteInspectAxiomsSources>(axioms.Inherited.Value);
        Assert.Equal(["loader", "root"], inherited.SourceIds);
        Assert.Equal(RouteInspectAxiomsLocalState.Substantive, axioms.Local.Value);

        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.OwnSource,
            workspace,
            ".agents/root/selected/_selected.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.SelectedClosure,
            workspace,
            ".agents/root/_root.md",
            ".agents/root/baseline.md",
            ".agents/root/selected/_selected.md",
            ".agents/root/selected/load-now-child.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.TaskStartOverlap,
            workspace,
            ".agents/root/_root.md",
            ".agents/root/baseline.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.SelectionAddition,
            workspace,
            ".agents/root/selected/_selected.md",
            ".agents/root/selected/load-now-child.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.LoadNowDescendants,
            workspace,
            ".agents/root/selected/load-now-child.md");

        Assert.DoesNotContain(
            result.Observations,
            observation => observation.Subject is "global" or "other");
        Assert.Empty(result.Conditions);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "Route inspect preserves global file continuity, selected-chain ancestor continuity, and on-demand exclusion without following ordinary links")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task ContinuityAndOnDemandReasonsRemainIndependent()
    {
        using var workspace = CreateLoadingWorkspace();
        var before = workspace.Snapshot();

        var global = await workspace.InspectAsync(
            "root/global",
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, global.Status);
        var globalProfile = Assert.IsType<RouteInspectProfile>(global.Profile);
        Assert.True(Assert.IsType<bool>(globalProfile.Reading.TaskStart.Value));
        var globalReason = Assert.Single(
            Assert.IsType<RouteInspectAutomaticReadings>(globalProfile.Reading.Automatic.Value).Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.RoutedFileKeepInMind, globalReason.Kind);
        Assert.Equal(
            [
                RouteInspectAutomaticReadingEvent.TaskReview,
                RouteInspectAutomaticReadingEvent.LaterReview,
            ],
            globalReason.Events);
        var globalAxioms = Assert.IsType<RouteInspectAxiomsProfile>(globalProfile.Axioms.Value);
        var globalInherited = Assert.IsType<RouteInspectAxiomsSources>(globalAxioms.Inherited.Value);
        Assert.Equal(["loader", "root"], globalInherited.SourceIds);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            globalProfile.Measurements.OwnSource,
            workspace,
            ".agents/root/global.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            globalProfile.Measurements.SelectedClosure,
            workspace,
            ".agents/root/_root.md",
            ".agents/root/baseline.md",
            ".agents/root/global.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            globalProfile.Measurements.TaskStartOverlap,
            workspace,
            ".agents/root/_root.md",
            ".agents/root/baseline.md",
            ".agents/root/global.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            globalProfile.Measurements.SelectionAddition,
            workspace);
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(globalProfile.Measurements.LoadNowDescendants);
        Assert.Equal(RouteInspectFactState.Value, globalProfile.Topology.State);
        Assert.Equal(
            RouteInspectFactState.NotApplicable,
            Assert.IsType<RouteInspectTopology>(globalProfile.Topology.Value).Counts.State);
        Assert.Equal(RouteInspectAxiomsLocalState.NotApplicable, globalAxioms.Local.Value);

        var ancestor = await workspace.InspectAsync(
            "root/other",
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, ancestor.Status);
        var ancestorProfile = Assert.IsType<RouteInspectProfile>(ancestor.Profile);
        Assert.True(Assert.IsType<bool>(ancestorProfile.Reading.TaskStart.Value));
        var ancestorReason = Assert.Single(
            Assert.IsType<RouteInspectAutomaticReadings>(ancestorProfile.Reading.Automatic.Value).Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.EntrypointKeepInMind, ancestorReason.Kind);
        Assert.Contains(RouteInspectAutomaticReadingEvent.AncestorRequired, ancestorReason.Events);
        Assert.Contains(RouteInspectAutomaticReadingEvent.RouteSelected, ancestorReason.Events);
        Assert.Contains(RouteInspectAutomaticReadingEvent.ScopeSelected, ancestorReason.Events);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            ancestorProfile.Measurements.LoadNowDescendants,
            workspace,
            ".agents/root/other/other-child.md");

        var onDemand = await workspace.InspectAsync(
            "root/selected/on-demand-child",
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, onDemand.Status);
        var onDemandProfile = Assert.IsType<RouteInspectProfile>(onDemand.Profile);
        Assert.False(Assert.IsType<bool>(onDemandProfile.Reading.TaskStart.Value));
        var onDemandReason = Assert.Single(
            Assert.IsType<RouteInspectAutomaticReadings>(onDemandProfile.Reading.Automatic.Value).Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.OnDemand, onDemandReason.Kind);
        Assert.Equal(
            [RouteInspectAutomaticReadingEvent.RouteSelected],
            onDemandReason.Events);
        Assert.Equal(RouteInspectFactState.NotApplicable, onDemandProfile.Measurements.LoadNowDescendants.State);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            onDemandProfile.Measurements.SelectionAddition,
            workspace,
            ".agents/root/selected/_selected.md",
            ".agents/root/selected/load-now-child.md",
            ".agents/root/selected/on-demand-child.md");
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    private static RouteInspectProfileIntegrationWorkspace CreateLoadingWorkspace()
    {
        var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "LoadNow", "Root")],
            "- Loader rules support continuity recovery.");
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/_root.md",
                Description = "Root",
                Tags = ["Root"],
                Axioms = "- Root rules are inherited by selected child routes.",
                Entries =
                [
                    RouteInspectProfileIntegrationWorkspace.Entry("Baseline", "baseline.md", "LoadNow"),
                    RouteInspectProfileIntegrationWorkspace.Entry(
                        "Forged detached source",
                        "../detached/_detached.md",
                        "LoadNow"),
                    RouteInspectProfileIntegrationWorkspace.Entry("Stale source", "ghost.md", "LoadNow"),
                ],
            });
        workspace.WriteRoutedMarkdown(
            ".agents/root/baseline.md",
            "Baseline",
            ["LoadNow"],
            "# Baseline\n\nThis is the only visible root descendant.\n");
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/selected/_selected.md",
                Description = "Selected",
                Tags = ["Selected", "KeepInMind"],
                Axioms = "- Selected rules are local to the selected route.",
                Entries =
                [
                    RouteInspectProfileIntegrationWorkspace.Entry("On demand child", "on-demand-child.md", "Route"),
                    RouteInspectProfileIntegrationWorkspace.Entry(
                        "Wrong topology target",
                        "../other/_other.md",
                        "LoadNow"),
                    RouteInspectProfileIntegrationWorkspace.Entry("Load now child", "load-now-child.md", "LoadNow"),
                    RouteInspectProfileIntegrationWorkspace.Entry("Linked only", "linked-only.md", "Route"),
                    RouteInspectProfileIntegrationWorkspace.Entry("Stale child", "ghost.md", "LoadNow"),
                ],
            });
        workspace.WriteRoutedMarkdown(
            ".agents/root/selected/on-demand-child.md",
            "On demand child",
            ["Route"],
            "# On demand child\n\nIt is selected explicitly, not loaded automatically.\n");
        workspace.WriteRoutedMarkdown(
            ".agents/root/selected/load-now-child.md",
            "Load now child",
            ["LoadNow"],
            "# Load now child\n\nIt is the only valid direct LoadNow descendant.\n");
        workspace.WriteRoutedMarkdown(
            ".agents/root/selected/linked-only.md",
            "Linked only",
            ["Route"],
            "# Linked only\n\nIt is reachable only through an ordinary Markdown link.\n");
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/other/_other.md",
                Description = "Other",
                Tags = ["Other", "KeepInMind"],
                Axioms = "- Other is discovered only as the required ancestor of a continuity file.",
                Entries =
                [RouteInspectProfileIntegrationWorkspace.Entry("Other child", "other-child.md", "LoadNow")],
            });
        workspace.WriteRoutedMarkdown(
            ".agents/root/other/other-child.md",
            "Other child",
            ["LoadNow"],
            "# Other child\n\nThis follows the required continuity ancestor.\n");
        workspace.WriteRoutedMarkdown(
            ".agents/root/global.md",
            "Global continuity",
            ["KeepInMind"],
            "# Global continuity\n\n[Linked only](selected/linked-only.md)\n");
        workspace.WriteRoutedMarkdown(
            ".agents/root/other/global-inside.md",
            "Nested global continuity",
            ["KeepInMind"],
            "# Nested global continuity\n\n[Linked only](../selected/linked-only.md)\n");
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/detached/_detached.md",
                Description = "Detached",
                Tags = ["Detached"],
            });
        return workspace;
    }
}
