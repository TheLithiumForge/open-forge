using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
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
                ApplyTo = ["src/**"],
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
            TestContext.Current.CancellationToken,
            ["src/Order.cs"]);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal(RouteInspectRouteState.Detached, identity.RouteState);
        Assert.Equal(RouteInspectSourceKind.Entrypoint, identity.Kind);
        var applicability = Assert.IsType<RouteInspectApplicability>(result.Applicability);
        Assert.Equal(RouteInspectApplicabilityState.Matched, applicability.State);
        Assert.Equal([".agents/detached/_detached.md"], applicability.Conditions.Select(condition => condition.Source));
        Assert.Equal(["src/Order.cs"], applicability.MatchingPaths);
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

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect loads an untagged visible source when a supplied file matches its condition")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task MatchingFileConditionLoadsVisibleUntaggedSource()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "LoadNow")]);
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/_root.md",
                Description = "Root",
                Tags = ["Root"],
                Entries =
                [RouteInspectProfileIntegrationWorkspace.Entry("Conditional", "conditional.md")],
            });
        workspace.WriteRoutedMarkdown(
            ".agents/root/conditional.md",
            "Conditional",
            ["Route"],
            "# Conditional\n\nThis source is exposed without a loading tag.\n",
            ["src/**"]);
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync(
            "root/conditional",
            TestContext.Current.CancellationToken,
            ["src/Order.cs"]);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        Assert.True(Assert.IsType<bool>(profile.Reading.TaskStart.Value));
        var reason = Assert.Single(Assert.IsType<RouteInspectAutomaticReadings>(profile.Reading.Automatic.Value).Reasons);
        Assert.Equal(RouteInspectAutomaticReadingKind.MatchingFileCondition, reason.Kind);
        Assert.Equal(
            [RouteInspectAutomaticReadingEvent.WorkingPathMatches],
            reason.Events);
        var applicability = Assert.IsType<RouteInspectApplicability>(result.Applicability);
        Assert.Equal(RouteInspectApplicabilityState.Matched, applicability.State);
        Assert.Equal(["src/Order.cs"], applicability.MatchingPaths);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.TaskStartOverlap,
            workspace,
            ".agents/root/_root.md",
            ".agents/root/conditional.md");
        var topology = Assert.IsType<RouteInspectTopology>(profile.Topology.Value);
        RouteInspectProfileIntegrationAssertions.AssertNotApplicable(topology.Counts);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect requires ancestor conditions to match one same working path")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task AncestorConditionsIntersectOnTheSameWorkingPath()
    {
        using var workspace = CreateConditionedSelectedRoute();
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync(
            "root/selected",
            TestContext.Current.CancellationToken,
            ["src/README.md", "tests/Order.cs"]);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var applicability = Assert.IsType<RouteInspectApplicability>(result.Applicability);
        Assert.Equal(RouteInspectApplicabilityState.Unmatched, applicability.State);
        Assert.Equal(
            [".agents/root/_root.md", ".agents/root/selected/_selected.md"],
            applicability.Conditions.Select(condition => condition.Source));
        Assert.Empty(applicability.MatchingPaths);

        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        Assert.Equal(RouteInspectFactState.Value, profile.Reading.TaskStart.State);
        Assert.False(Assert.IsType<bool>(profile.Reading.TaskStart.Value));
        var automatic = Assert.IsType<RouteInspectAutomaticReadings>(profile.Reading.Automatic.Value);
        Assert.Equal(RouteInspectAutomaticReadingKind.OnDemand, Assert.Single(automatic.Reasons).Kind);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.SelectedClosure,
            workspace,
            ".agents/root/_root.md",
            ".agents/root/selected/_selected.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.LoadNowDescendants,
            workspace);
        var topology = Assert.IsType<RouteInspectTopology>(profile.Topology.Value);
        var counts = Assert.IsType<RouteInspectTopologyCounts>(topology.Counts.Value);
        Assert.Equal(1, counts.DirectRoutedFileCount);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect keeps conditioned loading pending without working paths and retains structural counts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task MissingWorkingPathsLeaveEffectiveLoadingPending()
    {
        using var workspace = CreateConditionedSelectedRoute();
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync(
            "root/selected",
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        var applicability = Assert.IsType<RouteInspectApplicability>(result.Applicability);
        Assert.Equal(RouteInspectApplicabilityState.Pending, applicability.State);
        Assert.Empty(applicability.MatchingPaths);
        Assert.Equal(2, applicability.Conditions.Count);

        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        Assert.Equal(RouteInspectCompleteness.Incomplete, profile.Completeness);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Reading.TaskStart);
        RouteInspectProfileIntegrationAssertions.AssertUnavailable(profile.Reading.Automatic);
        var topology = Assert.IsType<RouteInspectTopology>(profile.Topology.Value);
        var counts = Assert.IsType<RouteInspectTopologyCounts>(topology.Counts.Value);
        Assert.Equal(1, counts.DirectRoutedFileCount);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect overwrite companions inherit only the base source file condition")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task OverwriteCompanionInheritsBaseApplicability()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "LoadNow")]);
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/_root.md",
                Description = "Root",
                Tags = ["Root"],
                Entries = [RouteInspectProfileIntegrationWorkspace.Entry("Item", "item.md")],
            });
        workspace.WriteRoutedMarkdown(
            ".agents/root/item.md",
            "Item",
            ["Route"],
            "# Item\n\nBase item.\n",
            ["src/**"]);
        workspace.Write(".agents/root/item.overwrite.md", "# Replaced item\n");
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync(
            ".agents/root/item.overwrite.md",
            TestContext.Current.CancellationToken,
            ["src/Order.cs"]);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(2, Assert.IsType<RouteInspectIdentity>(result.Identity).PhysicalLayers.Count);
        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        var applicability = Assert.IsType<RouteInspectApplicability>(result.Applicability);
        Assert.Equal(RouteInspectApplicabilityState.Matched, applicability.State);
        Assert.Equal([".agents/root/item.md"], applicability.Conditions.Select(condition => condition.Source));
        Assert.Equal(["src/Order.cs"], applicability.MatchingPaths);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    private static RouteInspectProfileIntegrationWorkspace CreateConditionedSelectedRoute()
    {
        var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "LoadNow")]);
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/_root.md",
                Description = "Root",
                Tags = ["Root"],
                ApplyTo = ["src/**"],
                Entries =
                [RouteInspectProfileIntegrationWorkspace.Entry("Selected", "selected/_selected.md", "LoadNow")],
            });
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/selected/_selected.md",
                Description = "Selected",
                Tags = ["Selected"],
                ApplyTo = ["**/*.cs"],
                Entries =
                [RouteInspectProfileIntegrationWorkspace.Entry("Child", "child.md", "LoadNow")],
            });
        workspace.WriteRoutedMarkdown(
            ".agents/root/selected/child.md",
            "Child",
            ["Route", "LoadNow"],
            "# Child\n\nAutomatically loaded child.\n");
        return workspace;
    }

}
