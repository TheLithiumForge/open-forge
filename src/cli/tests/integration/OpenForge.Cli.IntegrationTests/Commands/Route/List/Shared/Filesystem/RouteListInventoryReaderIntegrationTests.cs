using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;

public sealed class RouteListInventoryReaderIntegrationTests
{
    [Fact(DisplayName = "Route-list directory enumeration sorts real entries by ordinal logical identity")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public void DirectoryEnumerationDoesNotTrustOperatingSystemOrder()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        workspace.Write(".agents/order/z.md", "z");
        workspace.Write(".agents/order/a.md", "a");
        workspace.CreateDirectory(".agents/order/middle");

        var result = new RouteListDirectoryEnumerator().Enumerate(
            workspace.Absolute(".agents/order"),
            ".agents/order",
            TestContext.Current.CancellationToken);
        var entries = Assert.IsAssignableFrom<IReadOnlyList<RouteListDirectoryEntry>>(result.Entries);

        Assert.Equal(DirectoryEnumerationState.Complete, result.State);
        Assert.Equal(["a.md", "middle", "z.md"], entries.Select(entry => entry.Name));
        Assert.Equal(
            [".agents/order/a.md", ".agents/order/middle", ".agents/order/z.md"],
            entries.Select(entry => entry.CanonicalLogicalPath));
    }

    [Fact(DisplayName = "Route-list inventory classifies authored source forms and ignores generated descendant authority")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task InventoryClassifiesMetadataOverwriteAmbiguityAndReadFailures()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        workspace.Write(
            ".agents/loader.md",
            """
            # Loader

            ## Entries

            <!-- open-forge:generated-index:start -->
            - [Ghost](ghost/_ghost.md) - #Ghost
            <!-- open-forge:generated-index:end -->
            """);
        workspace.Write(
            ".agents/root/_root.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Canonical root", "Route", "Root"));
        workspace.Write(
            ".agents/root/index.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Compatibility root", "Route", "Compatibility"));
        workspace.Write(
            ".agents/root/leaf.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Exact leaf description", "Leaf", "Route-List")
            + "\n[Stale body link](ghost.md)\n");
        workspace.Write(".agents/root/leaf.overwrite.md", "Local overwrite body.\n");
        workspace.Write(
            ".agents/root/tool/SKILL.md",
            RouteListFilesystemIntegrationWorkspace.SkillMetadata("native-tool", "Exact native description."));
        workspace.Write(".agents/root/orphan.overwrite.md", "Orphan overwrite.\n");
        workspace.Write(".agents/root/invalid.md", [0xC3, 0x28]);
        workspace.Write(".agents/root/missing.md", "# No frontmatter\n");
        workspace.Write(".agents/root/resource.txt", [0xC3, 0x28]);
        var before = workspace.SnapshotHashes();

        var facts = await workspace.ReadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(RouteListInventoryState.Incomplete, facts.State);
        Assert.Equal(
            [
                ".agents/loader.md",
                ".agents/root/_root.md",
                ".agents/root/index.md",
                ".agents/root/invalid.md",
                ".agents/root/leaf.md",
                ".agents/root/missing.md",
                ".agents/root/tool/SKILL.md",
            ],
            facts.Sources.Select(source => source.Source.CanonicalPath));
        Assert.DoesNotContain(facts.Sources, source => source.Source.CanonicalPath.Contains("ghost", StringComparison.Ordinal));
        Assert.DoesNotContain(facts.Sources, source => source.Source.CanonicalPath.EndsWith("resource.txt", StringComparison.Ordinal));
        Assert.DoesNotContain(facts.Sources, source => source.Form == RouteListSourceForm.OverwriteCompanion);

        var loader = facts.Sources.Single(source => source.Form == RouteListSourceForm.Loader);
        Assert.Equal(RouteListSourceKind.Loader, loader.Source.Kind);
        Assert.Equal(RouteListMetadataState.NotApplicable, loader.Metadata.State);
        var canonical = facts.Sources.Single(source => source.Form == RouteListSourceForm.CanonicalEntrypoint);
        var compatibility = facts.Sources.Single(source => source.Form == RouteListSourceForm.IndexEntrypoint);
        Assert.True(canonical.Source.IsRouteAmbiguous);
        Assert.True(compatibility.Source.IsRouteAmbiguous);
        Assert.True(compatibility.Metadata.IsCompatibilityEntrypoint);

        var leaf = facts.Sources.Single(source => source.Source.CanonicalPath == ".agents/root/leaf.md");
        Assert.Equal(RouteListSourceKind.RoutedLeaf, leaf.Source.Kind);
        Assert.Equal("Exact leaf description", leaf.Metadata.Description);
        Assert.Equal(["Leaf", "Route-List"], leaf.Metadata.Tags);
        Assert.True(leaf.Metadata.IsOverwritePresent);
        Assert.Equal(".agents/root/leaf.overwrite.md", leaf.Overwrite?.CanonicalLogicalPath);
        Assert.Same(leaf.Source, facts.Catalogue.FindByPath(".agents/root/leaf.overwrite.md"));

        var skill = facts.Sources.Single(source => source.Form == RouteListSourceForm.Skill);
        Assert.Equal(RouteListSourceKind.RoutedNative, skill.Source.Kind);
        Assert.Equal("Exact native description.", skill.Metadata.Description);
        Assert.Empty(skill.Metadata.Tags);
        Assert.Equal(
            RouteListSourceKind.Unrouted,
            facts.Sources.Single(source => source.Source.CanonicalPath == ".agents/root/invalid.md").Source.Kind);
        Assert.Equal(
            RouteListSourceKind.Unrouted,
            facts.Sources.Single(source => source.Source.CanonicalPath == ".agents/root/missing.md").Source.Kind);
        Assert.Contains(
            facts.Findings,
            finding => finding.Code == RouteListFindingCode.AuthoredForm
                && finding.CanonicalLogicalSubject == ".agents/root/index.md");
        Assert.Contains(
            facts.Findings,
            finding => finding.Code == RouteListFindingCode.AuthoredForm
                && finding.CanonicalLogicalSubject == ".agents/root/orphan.overwrite.md");
        Assert.Contains(
            facts.Findings,
            finding => finding.Code == RouteListFindingCode.ReadUnavailable
                && finding.CanonicalLogicalSubject == ".agents/root/invalid.md");
        Assert.Contains(
            facts.Findings,
            finding => finding.Code == RouteListFindingCode.MetadataMissing
                && finding.CanonicalLogicalSubject == ".agents/root/missing.md");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route-list inventory reports a completely inspected empty .agents root")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task EmptyAgentsDirectoryIsComplete()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        var before = workspace.SnapshotHashes();

        var facts = await workspace.ReadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(RouteListInventoryState.Complete, facts.State);
        Assert.Empty(facts.Sources);
        Assert.Empty(facts.Findings);
        Assert.Empty(facts.PhysicalAliases);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route-list inventory stops at a pre-cancelled root without filesystem evidence")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task PreCancelledInventoryIsInterruptedBeforeRootAccess()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        workspace.Write(
            ".agents/unread.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Would be readable", "Route"));
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var facts = await workspace.ReadAsync(cancellation.Token);

        Assert.Equal(RouteListInventoryState.Interrupted, facts.State);
        Assert.Empty(facts.Sources);
        var finding = Assert.Single(facts.Findings);
        Assert.Equal(RouteListFindingCode.Interrupted, finding.Code);
        Assert.Equal(".agents", finding.CanonicalLogicalSubject);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
