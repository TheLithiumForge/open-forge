using System.Text;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectOperationCompleteIntegrationTests
{
    [Fact(DisplayName = "Route inspect forms a complete rooted profile with route topology, loading facts, measurements, and Axioms provenance")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task CompleteRootedOperationFormsEveryProfileFact()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [
                RouteInspectProfileIntegrationWorkspace.Entry(
                    "Root",
                    "root/_root.md",
                    "LoadNow",
                    "Root"),
            ],
            "- Loader rules are inherited before route-local rules.");
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/_root.md",
                Description = "Root",
                Tags = ["Root", "LoadNow", "KeepInMind"],
                Axioms = "- Root rules are substantive and remain local to the root route.",
                Entries =
                [
                    RouteInspectProfileIntegrationWorkspace.Entry("On demand", "on-demand.md", "Route"),
                    RouteInspectProfileIntegrationWorkspace.Entry("Load", "load.md", "LoadNow"),
                    RouteInspectProfileIntegrationWorkspace.Entry("Child", "child/_child.md", "LoadNow"),
                    RouteInspectProfileIntegrationWorkspace.Entry(
                        "Stale detached declaration",
                        "../detached/_detached.md",
                        "LoadNow"),
                    RouteInspectProfileIntegrationWorkspace.Entry("Stale source", "stale.md", "LoadNow"),
                ],
            });
        workspace.WriteRoutedMarkdown(
            ".agents/root/on-demand.md",
            "On demand",
            ["Route"],
            "# On demand\n\nThis source remains outside the automatic closure.\n");
        workspace.WriteRoutedMarkdown(
            ".agents/root/load.md",
            "Load",
            ["LoadNow"],
            "# Load\n\nThis source is exposed by the root entrypoint.\n");
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/child/_child.md",
                Description = "Child",
                Tags = ["Child", "LoadNow"],
                Axioms = "- Child rules are inherited by the nested route.",
                Entries =
                [
                    RouteInspectProfileIntegrationWorkspace.Entry("Grand load", "grand-load.md", "LoadNow"),
                    RouteInspectProfileIntegrationWorkspace.Entry("Grand on demand", "grand-on-demand.md", "Route"),
                    RouteInspectProfileIntegrationWorkspace.Entry("Grand", "grand/_grand.md", "Route"),
                ],
            });
        workspace.WriteRoutedMarkdown(
            ".agents/root/child/grand-load.md",
            "Grand load",
            ["LoadNow"],
            "# Grand load\n\nA visible nested load.\n");
        workspace.WriteRoutedMarkdown(
            ".agents/root/child/grand-on-demand.md",
            "Grand on demand",
            ["Route"],
            "# Grand on demand\n\nAn on-demand nested source.\n");
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/child/grand/_grand.md",
                Description = "Grand",
                Tags = ["Grand"],
                Entries =
                [RouteInspectProfileIntegrationWorkspace.Entry("Grand leaf", "grand-leaf.md", "Route")],
            });
        workspace.WriteRoutedMarkdown(
            ".agents/root/child/grand/grand-leaf.md",
            "Grand leaf",
            ["Route"],
            "# Grand leaf\n\nA structural descendant that is not automatically read.\n");
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/detached/_detached.md",
                Description = "Detached",
                Tags = ["Detached"],
            });

        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync("root", TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteInspectSelectionMethod.AutomaticId, result.Selection.SelectionMethod);
        Assert.Equal("root", result.Selection.RequestedReference);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal("root", identity.Id);
        Assert.Equal(".agents/root/_root.md", identity.CanonicalWorkspaceRelativePath);
        Assert.Equal(RouteInspectSourceKind.Entrypoint, identity.Kind);
        Assert.Equal(RouteInspectSourceForm.CanonicalEntrypoint, identity.Form);
        Assert.Equal(RouteInspectRouteState.Routed, identity.RouteState);
        Assert.Equal(
            [".agents/root/_root.md"],
            identity.PhysicalLayers.Select(layer => layer.WorkspaceRelativePath));

        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        Assert.Equal(RouteInspectCompleteness.Complete, profile.Completeness);
        Assert.Equal(RouteInspectSafety.Safe, profile.Safety);

        Assert.Equal(RouteInspectFactState.Value, profile.Reading.TaskStart.State);
        Assert.True(Assert.IsType<bool>(profile.Reading.TaskStart.Value));
        var automatic = Assert.IsType<RouteInspectAutomaticReadings>(profile.Reading.Automatic.Value);
        Assert.Equal(
            [
                RouteInspectAutomaticReadingKind.ParentLoadNow,
                RouteInspectAutomaticReadingKind.EntrypointKeepInMind,
            ],
            automatic.Reasons.Select(reason => reason.Kind));
        var parentReason = automatic.Reasons[0];
        Assert.Equal("loader", parentReason.RelatedSourceId);
        Assert.Equal(
            [RouteInspectAutomaticReadingEvent.ExposingParentRead],
            parentReason.Events);
        var continuityReason = automatic.Reasons[1];
        Assert.Contains(RouteInspectAutomaticReadingEvent.TaskStartVisible, continuityReason.Events);
        Assert.Contains(RouteInspectAutomaticReadingEvent.RouteSelected, continuityReason.Events);
        Assert.Contains(RouteInspectAutomaticReadingEvent.ScopeSelected, continuityReason.Events);
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
        Assert.Equal(["root"], topology.RouteChain);
        Assert.Null(topology.ParentId);
        Assert.Equal(1, topology.Depth);
        var counts = Assert.IsType<RouteInspectTopologyCounts>(topology.Counts.Value);
        Assert.Equal(2, counts.DirectRoutedFileCount);
        Assert.Equal(1, counts.DirectEntrypointCount);
        Assert.Equal(5, counts.DescendantRoutedFileCount);
        Assert.Equal(2, counts.DescendantEntrypointCount);

        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        var inherited = Assert.IsType<RouteInspectAxiomsSources>(axioms.Inherited.Value);
        Assert.Equal(["loader"], inherited.SourceIds);
        Assert.Equal(RouteInspectAxiomsLocalState.Substantive, axioms.Local.Value);

        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.OwnSource,
            workspace,
            ".agents/root/_root.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.SelectedClosure,
            workspace,
            ".agents/root/_root.md",
            ".agents/root/load.md",
            ".agents/root/child/_child.md",
            ".agents/root/child/grand-load.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.TaskStartOverlap,
            workspace,
            ".agents/root/_root.md",
            ".agents/root/load.md",
            ".agents/root/child/_child.md",
            ".agents/root/child/grand-load.md");
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.SelectionAddition,
            workspace);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.LoadNowDescendants,
            workspace,
            ".agents/root/load.md",
            ".agents/root/child/_child.md",
            ".agents/root/child/grand-load.md");

        Assert.Empty(result.Observations);
        Assert.Empty(result.Conditions);
        Assert.Null(result.Next);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "Route inspect round-trips BOM, CRLF, and non-ASCII UTF-8 while measuring a valid base-overwrite pair and zero descendants")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task StrictUtf8AndOverwriteMeasurementsRemainExact()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "LoadNow", "Root")]);

        var encoding = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true);
        const string basePath = ".agents/root/_root.md";
        const string overwritePath = ".agents/root/_root.overwrite.md";
        var baseBytes = encoding.GetBytes(
            "---\r\n"
            + "open-forge:\r\n"
            + "  description: Root résumé\r\n"
            + "  tags: [Root]\r\n"
            + "---\r\n"
            + "# Root π\r\n"
            + "\r\n"
            + "## Axioms\r\n"
            + "\r\n"
            + "- Preserve Ω and 東京.\r\n"
            + "\r\n"
            + "## Entries\r\n"
            + "\r\n"
            + "<!-- open-forge:generated-index:start -->\r\n"
            + "\r\n"
            + "- none - No entries - #Empty\r\n"
            + "\r\n"
            + "<!-- open-forge:generated-index:end -->\r\n");
        var bomEncoding = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: true,
            throwOnInvalidBytes: true);
        var overwriteBytes = bomEncoding.GetPreamble()
            .Concat(encoding.GetBytes("overwrite\r\nΔ 😀"))
            .ToArray();
        workspace.Write(basePath, baseBytes);
        workspace.Write(overwritePath, overwriteBytes);
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync("root", TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal(
            [basePath, overwritePath],
            identity.PhysicalLayers.Select(layer => layer.WorkspaceRelativePath));
        Assert.Equal(
            [RouteInspectLayerRole.Base, RouteInspectLayerRole.Overwrite],
            identity.PhysicalLayers.Select(layer => layer.Role));
        Assert.Equal(
            [workspace.Absolute(basePath), workspace.Absolute(overwritePath)],
            identity.PhysicalLayers.Select(layer => layer.PhysicalPath));
        Assert.Contains(
            result.Observations,
            observation => observation.Code == RouteInspectObservationCode.ValidOverwrite);

        var strict = new UTF8Encoding(false, true);
        Assert.Equal(baseBytes, File.ReadAllBytes(workspace.Absolute(basePath)));
        Assert.Equal(overwriteBytes, File.ReadAllBytes(workspace.Absolute(overwritePath)));
        Assert.Equal([0xEF, 0xBB, 0xBF], overwriteBytes[..3]);
        var decodedOverwrite = strict.GetString(overwriteBytes);
        Assert.Contains('\uFEFF', decodedOverwrite);
        Assert.Equal(overwriteBytes, strict.GetBytes(decodedOverwrite));

        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        Assert.Equal(RouteInspectAxiomsLocalState.Substantive, axioms.Local.Value);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.OwnSource,
            workspace,
            basePath,
            overwritePath);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.SelectedClosure,
            workspace,
            basePath,
            overwritePath);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.TaskStartOverlap,
            workspace,
            basePath,
            overwritePath);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.SelectionAddition,
            workspace);
        RouteInspectProfileIntegrationAssertions.AssertMeasurement(
            profile.Measurements.LoadNowDescendants,
            workspace);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }
}
