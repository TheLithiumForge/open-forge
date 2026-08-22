using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;
using OpenForge.Cli.Core.Commands.Route.Shared.Topology;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Topology;

public sealed class RouteListTopologyIntegrationTests
{
    [Fact(DisplayName = "Route list forms canonical rows from real authored topology instead of generated Entries"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task FormsCanonicalRowsFromRealAuthoredTopology()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(
            workspace,
            "- [Root](root/_root.md) - #Root\n- [Workspace](workspace-defined/_workspace-defined.md) - #Workspace");
        WriteRoute(
            workspace,
            ".agents/root/_root.md",
            "Root route",
            "Root",
            """
            ## Entries

            <!-- open-forge:generated-index:start -->
            - [Fake](fake.md) - #Fake
            - [Zeta](zeta.md) - #Zeta
            - [Alpha](alpha.md) - #Alpha
            <!-- open-forge:generated-index:end -->
            """);
        WriteRoute(workspace, ".agents/root/zeta.md", "Zeta route", "Zeta");
        WriteRoute(workspace, ".agents/root/alpha.md", "Alpha route", "Alpha");
        WriteRoute(workspace, ".agents/root/child/_child.md", "Child route", "Child");
        WriteRoute(workspace, ".agents/root/child/grand.md", "Grand route", "Grand");
        workspace.Write(
            ".agents/root/native/SKILL.md",
            RouteListFilesystemIntegrationWorkspace.SkillMetadata("native", "Native route"));
        WriteRoute(workspace, ".agents/root/unrepresented/leaf.md", "Unrepresented route", "Leaf");
        WriteRoute(workspace, ".agents/workspace-defined/_workspace-defined.md", "Workspace route", "Workspace");
        var before = workspace.SnapshotHashes();

        var result = await RunAsync(workspace, null, RouteListDepth.All);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(
            [
                "root",
                "root/alpha",
                "root/child",
                "root/child/grand",
                "root/native",
                "root/zeta",
                "workspace-defined",
            ],
            result.Rows.Select(row => row.Id));
        Assert.DoesNotContain(result.Rows, row => row.Id == "root/fake");
        Assert.DoesNotContain(result.Rows, row => row.Id == "root/unrepresented/leaf");
        var root = result.Rows[0];
        Assert.Equal(4, root.DirectChildCount);
        Assert.Equal("Root route", root.Description);
        Assert.Equal(["Root"], root.Tags);
        var child = result.Rows.Single(row => row.Id == "root/child");
        Assert.Equal("root", child.ParentId);
        Assert.Equal(1, child.AbsoluteDepth);
        Assert.Equal(1, child.RelativeDepth);
        Assert.Equal(1, child.DirectChildCount);
        var skill = result.Rows.Single(row => row.Id == "root/native");
        Assert.Equal(RouteListSourceProvenance.RoutedNative, skill.Provenance.Source);
        Assert.Empty(skill.Tags);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route list real explicit selection distinguishes nested and detached ancestry"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ExplicitSelectionDistinguishesNestedAndDetachedAncestry()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        WriteRoute(workspace, ".agents/root/child/_child.md", "Child route", "Child");
        WriteRoute(workspace, ".agents/root/child/grand.md", "Grand route", "Grand");
        WriteRoute(workspace, ".agents/detached/_detached.md", "Detached route", "Detached");
        WriteRoute(workspace, ".agents/detached/leaf.md", "Detached leaf", "Leaf");

        var defaultResult = await RunAsync(workspace, null, RouteListDepth.All);
        var nestedResult = await RunAsync(workspace, "root/child", RouteListDepth.All);
        var detachedResult = await RunAsync(workspace, "detached", RouteListDepth.All);

        Assert.DoesNotContain(defaultResult.Rows, row => row.Id == "detached");
        Assert.Equal(["root/child", "root/child/grand"], nestedResult.Rows.Select(row => row.Id));
        Assert.Equal("root", nestedResult.Rows[0].ParentId);
        Assert.Equal(1, nestedResult.Rows[0].AbsoluteDepth);
        Assert.Equal(0, nestedResult.Rows[0].RelativeDepth);
        Assert.Equal(RouteListSelectionProvenance.ExplicitRoot, nestedResult.Rows[0].Provenance.Selection);
        Assert.Equal(["detached", "detached/leaf"], detachedResult.Rows.Select(row => row.Id));
        Assert.All(detachedResult.Rows, row => Assert.Null(row.AbsoluteDepth));
        Assert.Equal(RouteListSelectionProvenance.DetachedRoot, detachedResult.Rows[0].Provenance.Selection);
    }

    [Theory(DisplayName = "Route list real inventory preserves exact finite and all depth boundaries"), InlineData(0, 1), InlineData(1, 2), InlineData(2, 3), InlineData(-1, 4), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task PreservesExactDepthBoundaries(int depthValue, int expectedRows)
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        WriteRoute(workspace, ".agents/root/child/_child.md", "Child route", "Child");
        WriteRoute(workspace, ".agents/root/child/grand/_grand.md", "Grand route", "Grand");
        WriteRoute(workspace, ".agents/root/child/grand/deep.md", "Deep route", "Deep");
        var depth = depthValue < 0
            ? RouteListDepth.All
            : RouteListDepth.Finite(depthValue);

        var result = await RunAsync(workspace, "root", depth);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(expectedRows, result.Rows.Count);
        Assert.All(result.Rows, row => Assert.True(depthValue < 0 || row.RelativeDepth <= depthValue));
        Assert.Equal(depth.MachineValue, result.EffectiveDepth?.MachineValue);
    }

    [Fact(DisplayName = "Route list real malformed metadata retains safe rows and honest incomplete coverage"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task MalformedMetadataRetainsSafeRows()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        workspace.Write(
            ".agents/root/malformed.md",
            "---\nopen-forge: [\n---\n\n# Malformed\n");
        workspace.Write(".agents/root/orphan.overwrite.md", "Workspace adjustment\n");

        var first = await RunAsync(workspace, "root", RouteListDepth.All);
        var second = await RunAsync(workspace, "root", RouteListDepth.All);

        Assert.Equal(CliSemanticStatus.Incomplete, first.Status);
        Assert.Equal(["root"], first.Rows.Select(row => row.Id));
        Assert.Null(first.Rows[0].DirectChildCount);
        Assert.Equal(0, first.EffectiveDepth?.Value);
        Assert.Contains(first.Findings, finding => finding.Code == RouteListFindingCode.MetadataMalformed);
        Assert.Contains(first.Findings, finding => finding.Code == RouteListFindingCode.AuthoredForm);
        Assert.Equal(
            [RouteListFindingCode.MetadataMalformed, RouteListFindingCode.AuthoredForm],
            first.Findings.Select(finding => finding.Code));
        Assert.Equal(
            first.Findings.Select(FindingKey),
            second.Findings.Select(FindingKey));
        Assert.Equal(
            first.Rows.Select(RowKey),
            second.Rows.Select(RowKey));
    }

    [Fact(DisplayName = "Route list real partial Loader selection retains safe roots without complete coverage"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task PartialLoaderSelectionRetainsSafeRoots()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(
            workspace,
            "- [Root](root/_root.md) - #Root\n- [Broken](broken/_broken.md - #Broken");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");

        var result = await RunAsync(workspace, null, RouteListDepth.All);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(["root"], result.Rows.Select(row => row.Id));
        Assert.Equal(1, result.Coverage.SelectedRootCount);
        Assert.Null(result.EffectiveDepth);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.LoaderMalformed);
        Assert.NotNull(result.Next);
    }

    [Fact(DisplayName = "Route list real duplicate entrypoints block the ambiguous branch and retain its safe parent"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task DuplicateEntrypointsBlockAmbiguousBranch()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        WriteRoute(workspace, ".agents/root/ambiguous/_ambiguous.md", "Canonical child", "Child");
        WriteRoute(workspace, ".agents/root/ambiguous/index.md", "Compatibility child", "Child");

        var result = await RunAsync(workspace, "root", RouteListDepth.All);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(["root"], result.Rows.Select(row => row.Id));
        Assert.Null(result.Rows[0].DirectChildCount);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.RouteAmbiguous);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.AuthoredForm);
        Assert.Equal(RouteListCoverageState.Blocked, result.Coverage.State);
    }

    [Fact(DisplayName = "Route list finite depth retains a child entrypoint when its overwrite read is incomplete"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ChildEntrypointOverwriteFailureUsesLogicalDepth()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        WriteRoute(workspace, ".agents/root/child/_child.md", "Child route", "Child");
        workspace.Write(".agents/root/child/_child.overwrite.md", [0xFF, 0xFE]);

        var result = await RunAsync(workspace, "root", RouteListDepth.Finite(1));

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(["root", "root/child"], result.Rows.Select(row => row.Id));
        Assert.True(result.Rows[1].Provenance.HasOverwrite);
        Assert.Null(result.Rows[0].DirectChildCount);
        Assert.Equal(0, result.EffectiveDepth?.Value);
        Assert.Contains(result.Findings, finding =>
            finding.Code == RouteListFindingCode.ReadUnavailable
            && finding.Subject == ".agents/root/child/_child.overwrite.md");
    }

    [Fact(DisplayName = "Route list accepts a completely inspected empty Loader root set"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task EmptyLoaderRootSetIsComplete()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, string.Empty);

        var result = await RunAsync(workspace, null, RouteListDepth.Default);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Rows);
        Assert.Empty(result.Findings);
        Assert.Equal(0, result.Coverage.SelectedRootCount);
        Assert.Equal(RouteListCoverageState.Complete, result.Coverage.State);
    }

    private static async ValueTask<RouteListResult> RunAsync(
        RouteListFilesystemIntegrationWorkspace workspace,
        string? sourceReference,
        RouteListDepth depth)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var inventory = await workspace.ReadAsync(cancellationToken);
        var resolver = new RouteListSelectionResolver(new PhysicalPathResolver());
        var request = new RouteListRequest(workspace.Workspace, sourceReference, depth);
        var selection = await resolver.ResolveAsync(request, inventory.Catalogue, cancellationToken);
        RouteListSelectionResolution? loaderSelection = null;
        if (sourceReference is not null && selection.State == RouteListSelectionResolutionState.Resolved)
        {
            loaderSelection = await resolver.ResolveAsync(
                new RouteListRequest(workspace.Workspace, null, depth),
                inventory.Catalogue,
                cancellationToken);
        }

        var input = new RouteListTopologyInput(request, inventory, selection, loaderSelection);
        var topology = new RouteTopologyBuilder().Build(
            input.Inventory.Sources
                .Where(source => source.Kind is (
                    RouteListSourceKind.Entrypoint
                    or RouteListSourceKind.RoutedLeaf
                    or RouteListSourceKind.RoutedNative))
                .Select(source => source.Source),
            input.LoaderRootPaths);
        var selected = new RouteListTopologySelector().Select(input, topology, cancellationToken);
        var coverage = new RouteListCoverageBuilder().Build(input, selected);
        return new RouteListResultBuilder().Build(input, selected, coverage);
    }

    private static void WriteLoader(
        RouteListFilesystemIntegrationWorkspace workspace,
        string entries)
    {
        workspace.Write(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build(entries));
    }

    private static void WriteRoute(
        RouteListFilesystemIntegrationWorkspace workspace,
        string path,
        string description,
        string tag,
        string body = "")
    {
        var metadata = RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata(description, tag);
        workspace.Write(
            path,
            body.Length == 0 ? metadata : $"{metadata}\n{body}");
    }

    private static string FindingKey(RouteListFinding finding)
    {
        return $"{finding.MachineCode}|{finding.Status}|{finding.Subject}|{finding.Cause}";
    }

    private static string RowKey(RouteListRow row)
    {
        return $"{row.Id}|{row.Path}|{row.ParentPath}|{row.AbsoluteDepth}|{row.RelativeDepth}";
    }
}
