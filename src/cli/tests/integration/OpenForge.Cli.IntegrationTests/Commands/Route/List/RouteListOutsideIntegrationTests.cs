using System.Text.Json;
using System.Text;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Route.List.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

[Trait("Boundary", "Host"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
public sealed class RouteListOutsideIntegrationTests
{
    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(RouteListOutsideWorkspace.CustomRoot, false), InlineData(RouteListOutsideWorkspace.CustomRoot, true)]
    public async Task UnreadableUnexposedChildRetainsConfirmedIncompleteCoverage(string root, bool explicitAnchor)
    {
        using var fixture = new RouteListOutsideWorkspace(root, inventoryOnly: true);
        byte[] invalid = [0xc3, 0x28];
        fixture.Inner.ReplaceBytes(RouteListOutsideWorkspace.Independent, invalid);
        var before = fixture.Inner.SnapshotHashes(includeRuntime: true);
        var directories = Directory.GetDirectories(fixture.Inner.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
        var reference = explicitAnchor ? SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor : null;
        var request = new RouteListRequest(fixture.Workspace, reference, RouteListDepth.All);
        var inventory = await ReadInventoryAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Incomplete, inventory.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, inventory.Coverage.State);
        var result = await RouteListOperationFactory.Create()(request, TestContext.Current.CancellationToken);
        var arguments = new List<string> { RouteDefinitions.RouteGroup.Name, RouteListDefinitions.ListCommand.Name };
        if (reference is not null) arguments.Add(reference);
        arguments.AddRange([RouteListDefinitions.Depth.Name, RouteListDefinitions.AllDepth,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat,
            CliSyntaxDefinitions.Detail.Name, CliPresentationDefinitions.Full]);
        var run = await CliHostCapture.RunAsync(arguments.ToArray(), fixture.Inner.Path);
        using var json = JsonDocument.Parse(run.Output);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, result.Coverage.State);
        Assert.Equal(inventory.Coverage.Evidence, result.Coverage.Evidence);
        Assert.All(inventory.Coverage.UnresolvedBoundaries, boundary => Assert.Contains(boundary, result.Coverage.UnresolvedBoundaries));
        Assert.NotEmpty(result.Coverage.UnresolvedBoundaries);
        var expected = explicitAnchor ? new[] { fixture.Anchor } : new[] { fixture.PrimaryAnchor, fixture.Anchor }.Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(expected, result.Rows.Select(row => row.Path));
        Assert.Equal(expected.Length, result.Coverage.ConfirmedRowCount);
        Assert.Equal(explicitAnchor ? 1 : 2, result.Coverage.SelectedRootCount);
        var anchor = Assert.Single(result.Rows, row => row.Path == fixture.Anchor);
        Assert.Null(anchor.DirectChildCount);
        Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, anchor.MaintenanceRoot);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.ReadUnavailable
            && finding.Status == CliSemanticStatus.Incomplete && finding.Subject == RouteListOutsideWorkspace.Independent);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == RouteListFindingCode.OperationFailed);

        var status = CliStatusDefinitions.Read(CliSemanticStatus.Incomplete);
        Assert.Equal(status.Disposition.ExitCode, run.ExitCode);
        Assert.Equal(status.MachineName, json.RootElement.GetProperty(CliJsonFields.Status).GetString());
        Assert.Empty(json.RootElement.GetProperty(CliJsonFields.Effects).EnumerateArray());
        var rows = json.RootElement.GetProperty(CliJsonFields.Data)
            .GetProperty(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteListData.Rows))).EnumerateArray().ToArray();
        Assert.Equal(expected, rows.Select(row => row.GetProperty(CliJsonFields.Path).GetString()));
        Assert.Contains(json.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray(), finding =>
            finding.GetProperty(CliJsonFields.Code).GetString() == RouteListDefinitions.ReadFindingCode(RouteListFindingCode.ReadUnavailable)
            && finding.GetProperty(CliJsonFields.Subject).GetProperty(CliJsonFields.Path).GetString() == RouteListOutsideWorkspace.Independent);
        Assert.Equal(invalid, File.ReadAllBytes(fixture.Inner.Combine(RouteListOutsideWorkspace.Independent)));
        Assert.Equal(before, fixture.Inner.SnapshotHashes(includeRuntime: true));
        Assert.Equal(directories, Directory.GetDirectories(fixture.Inner.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
    }

    private static async ValueTask<RouteListResult> ReadInventoryAsync(RouteListRequest request, CancellationToken token)
    {
        var resolver = new RouteListSourceResolver(new RouteListInventoryReader(),
            new RouteListSelectionResolver(new PhysicalPathResolver()), new SourceRouteFactsResolver());
        var source = await resolver.ResolveAsync(request, token);
        var roots = await resolver.ResolveLoaderRootAsync(request, source, token);
        var input = new RouteListTopologyInput(request, source.Inventory, source.Selection, roots);
        var topology = new RouteListTopologySelector().SelectSources(input, source.RouteFacts, token);
        var coverage = new RouteListCoverageBuilder().Build(input, topology);
        return new RouteListResultBuilder().Build(input, topology, coverage);
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(RouteListOutsideWorkspace.CustomRoot, false), InlineData(RouteListOutsideWorkspace.CustomRoot, true)]
    public async Task ExactInventoryLeafDoesNotRequireAnExposingRow(string root, bool listed)
    {
        using var fixture = new RouteListOutsideWorkspace(root, listed, inventoryOnly: true);
        var before = fixture.Inner.SnapshotHashes();
        foreach (var reference in new[] { RouteListOutsideWorkspace.Independent, RouteListOutsideWorkspace.IndependentOverwrite })
        {
            var selected = await RouteListOperationFactory.Create()(new RouteListRequest(fixture.Workspace,
                SourceWorkspaceRelativePath.LiteralReferencePrefix + reference, RouteListDepth.All), TestContext.Current.CancellationToken);
            Assert.Equal(before, fixture.Inner.SnapshotHashes());
            Assert.Equal(CliSemanticStatus.Complete, selected.Status);
            var row = Assert.Single(selected.Rows);
            AssertInventoryLeaf(row, fixture.Anchor, listed, relativeDepth: 0);
            Assert.Equal(listed ? RouteListSelectionProvenance.ExplicitRoot : RouteListSelectionProvenance.DetachedRoot, row.Provenance.Selection);
            await AssertPublicInventoryAsync(fixture, reference, [RouteListOutsideWorkspace.Independent]);
        }
        AssertInventoryBytes(fixture);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(RouteListOutsideWorkspace.CustomRoot, false), InlineData(RouteListOutsideWorkspace.CustomRoot, true)]
    public async Task AnchorSelectionRetainsUnexposedPhysicalDescendantsAndTruthfulCounts(string root, bool listed)
    {
        using var fixture = new RouteListOutsideWorkspace(root, listed, inventoryOnly: true);
        var before = fixture.Inner.SnapshotHashes();
        foreach (var depth in new[] { RouteListDepth.Finite(0), RouteListDepth.Finite(1), RouteListDepth.All })
        {
            var tree = await RouteListOperationFactory.Create()(new RouteListRequest(fixture.Workspace,
                SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor, depth), TestContext.Current.CancellationToken);
            Assert.Equal(before, fixture.Inner.SnapshotHashes());
            Assert.Equal(CliSemanticStatus.Complete, tree.Status);
            var anchor = Assert.Single(tree.Rows, row => row.Path == fixture.Anchor);
            Assert.Equal(1, anchor.DirectChildCount);
            Assert.Equal(RouteListRowKind.Entrypoint, anchor.Kind);
            Assert.Equal(listed ? 0 : (int?)null, anchor.AbsoluteDepth);
            Assert.Equal(0, anchor.RelativeDepth);
            Assert.Null(anchor.ParentPath);
            Assert.Equal(listed ? "Team authored" : "Authored team", anchor.Description);
            Assert.Equal(listed ? [SourceLoadingClosureResolver.LoadNowTag] : new[] { "Authored" }, anchor.Tags);
            var expected = depth.Kind == RouteListDepthKind.Finite && depth.FiniteValue == 0
                ? new[] { fixture.Anchor } : [fixture.Anchor, RouteListOutsideWorkspace.Independent];
            Assert.Equal(expected, tree.Rows.Select(row => row.Path));
            if (expected.Length > 1)
            {
                var leaf = tree.Rows[1];
                AssertInventoryLeaf(leaf, fixture.Anchor, listed, relativeDepth: 1);
                Assert.Equal(RouteListSelectionProvenance.Descendant, leaf.Provenance.Selection);
            }
            Assert.Equal(expected.Length, tree.Coverage.ConfirmedRowCount);
        }
        await AssertPublicInventoryAsync(fixture, fixture.Anchor, [fixture.Anchor, RouteListOutsideWorkspace.Independent]);
        var roots = await RouteListOperationFactory.Create()(new RouteListRequest(fixture.Workspace, null, RouteListDepth.All), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, roots.Status);
        Assert.Equal(listed, roots.Rows.Any(row => row.Path == fixture.Anchor));
        Assert.Equal(listed, roots.Rows.Any(row => row.Path == RouteListOutsideWorkspace.Independent));
        Assert.Contains(roots.Rows, row => row.Path == fixture.PrimaryAnchor);
        AssertInventoryBytes(fixture);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    private static void AssertInventoryLeaf(RouteListRow row, string anchor, bool listed, int relativeDepth)
    {
        Assert.Equal(RouteListOutsideWorkspace.Independent, row.Path);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + row.Path, row.Id);
        Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, row.MaintenanceRoot);
        Assert.Equal(anchor, row.ParentPath);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + anchor, row.ParentId);
        Assert.Equal(listed ? 1 : (int?)null, row.AbsoluteDepth);
        Assert.Equal(relativeDepth, row.RelativeDepth);
        Assert.Equal(RouteListRowKind.RoutedLeaf, row.Kind);
        Assert.Equal("Independent authored leaf", row.Description);
        Assert.Equal(["Body"], row.Tags);
        Assert.Null(row.DirectChildCount);
        Assert.Equal(RouteListSourceProvenance.AuthoredLeaf, row.Provenance.Source);
        Assert.True(row.Provenance.HasOverwrite);
    }

    private static void AssertInventoryBytes(RouteListOutsideWorkspace fixture)
    {
        Assert.Equal(Encoding.UTF8.GetBytes(RouteListOutsideWorkspace.IndependentDocument),
            File.ReadAllBytes(fixture.Inner.Combine(RouteListOutsideWorkspace.Independent)));
        Assert.Equal(Encoding.UTF8.GetBytes(RouteListOutsideWorkspace.IndependentOverwriteDocument),
            File.ReadAllBytes(fixture.Inner.Combine(RouteListOutsideWorkspace.IndependentOverwrite)));
    }

    private static async Task AssertPublicInventoryAsync(RouteListOutsideWorkspace fixture, string reference, string[] paths)
    {
        var run = await CliHostCapture.RunAsync([RouteDefinitions.RouteGroup.Name, RouteListDefinitions.ListCommand.Name,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + reference, RouteListDefinitions.Depth.Name, RouteListDefinitions.AllDepth,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat, CliSyntaxDefinitions.Detail.Name, CliPresentationDefinitions.Full], fixture.Inner.Path);
        Assert.Equal(0, run.ExitCode);
        using var json = JsonDocument.Parse(run.Output);
        Assert.Empty(json.RootElement.GetProperty(CliJsonFields.Effects).EnumerateArray());
        var rows = json.RootElement.GetProperty(CliJsonFields.Data)
            .GetProperty(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteListData.Rows))).EnumerateArray().ToArray();
        Assert.Equal(paths, rows.Select(row => row.GetProperty(CliJsonFields.Path).GetString()));
        Assert.All(rows, row => Assert.Equal(RouteListOutsideWorkspace.DirectoryPath,
            row.GetProperty(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteListDataRow.MaintenanceRoot))).GetString()));
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, true)]
    [InlineData(RouteListOutsideWorkspace.CustomRoot, false, true)]
    [InlineData(RouteListOutsideWorkspace.CustomRoot, true, false)]
    public async Task ExactRegisteredSelectionRetainsCoordinatesAndAuthoredRows(string root, bool listed, bool compatibility)
    {
        using var fixture = new RouteListOutsideWorkspace(root, listed, compatibility: compatibility);
        var before = fixture.Inner.SnapshotHashes();
        var result = await Compose(new RouteListRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor, RouteListDepth.All), TestContext.Current.CancellationToken);
        Assert.True(result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention,
            string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.Equal(new[] { fixture.Anchor, RouteListOutsideWorkspace.Child, RouteListOutsideWorkspace.Skill }.Order(StringComparer.Ordinal),
            result.Rows.Select(row => row.Path).Order(StringComparer.Ordinal));
        Assert.All(result.Rows, row =>
        {
            Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + row.Path, row.Id);
            Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, row.MaintenanceRoot);
        });
        var child = Assert.Single(result.Rows, row => row.Path == RouteListOutsideWorkspace.Child);
        Assert.Equal("First child", child.Description);
        Assert.Equal(new[] { "LoadNow", "Row" }, child.Tags);
        Assert.True(child.Provenance.HasOverwrite);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    private static async ValueTask<RouteListResult> Compose(RouteListRequest request, CancellationToken token)
    {
        var resolver = new RouteListSourceResolver(new RouteListInventoryReader(),
            new RouteListSelectionResolver(new PhysicalPathResolver()), new SourceRouteFactsResolver());
        var source = await resolver.ResolveAsync(request, token);
        var roots = await resolver.ResolveLoaderRootAsync(request, source, token);
        return new RouteListResultComposer(new RouteListTopologySelector(), new RouteListCoverageBuilder(), new RouteListResultBuilder())
            .Build(request, source, roots, token);
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task UnlistedInventoryDoesNotBecomeAnOperandFreeRoot(string root)
    {
        using var fixture = new RouteListOutsideWorkspace(root, listed: false);
        var result = await RouteListOperationFactory.Create()(new RouteListRequest(fixture.Workspace, null, RouteListDepth.All),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.DoesNotContain(result.Rows, row => row.MaintenanceRoot is not null);
        var leaf = await RouteListOperationFactory.Create()(new RouteListRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + RouteListOutsideWorkspace.Overwrite, RouteListDepth.All),
            TestContext.Current.CancellationToken);
        Assert.Equal(RouteListOutsideWorkspace.Child, Assert.Single(leaf.Rows).Path);
        Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, leaf.Rows[0].MaintenanceRoot);
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public async Task RegistrationCoordinateIsOptionalAtEveryJsonDetail(bool registered)
    {
        using var fixture = new RouteListOutsideWorkspace(registered: registered, fileIncluded: true);
        var before = fixture.Inner.SnapshotHashes();
        foreach (var detail in new[] { CliPresentationDefinitions.Minimal, CliPresentationDefinitions.Standard,
            CliPresentationDefinitions.Full, CliPresentationDefinitions.Debug })
        {
            var run = await CliHostCapture.RunAsync([RouteDefinitions.RouteGroup.Name, RouteListDefinitions.ListCommand.Name,
                SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor, RouteListDefinitions.Depth.Name, RouteListDefinitions.AllDepth,
                CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat, CliSyntaxDefinitions.Detail.Name,
                detail], fixture.Inner.Path);
            Assert.Equal(0, run.ExitCode);
            using var report = JsonDocument.Parse(run.Output);
            var rows = report.RootElement.GetProperty(CliJsonFields.Data).GetProperty(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteListData.Rows))).EnumerateArray();
            Assert.All(rows, row =>
            {
                var present = row.TryGetProperty(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteListDataRow.MaintenanceRoot)), out var value);
                Assert.Equal(registered, present);
                if (present) Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, value.GetString());
            });
        }
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }
}
