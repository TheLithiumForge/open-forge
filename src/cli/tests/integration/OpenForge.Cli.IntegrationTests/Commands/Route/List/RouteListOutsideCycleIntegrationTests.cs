using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

[Trait("Boundary", "Host"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
public sealed class RouteListOutsideCycleIntegrationTests
{
    private const string Outside = "docs/team";
    private const string Anchor = Outside + "/_team.md";
    private const string Other = Outside + "/other.md";
    private const string FirstTag = "First";
    private const string LaterTag = "Later";
    private const string FirstDescription = "First authored edge";
    private const string BackDescription = "Authored return edge";

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(RouteListOutsideWorkspace.CustomRoot, false)]
    [InlineData(RouteListOutsideWorkspace.CustomRoot, true)]
    public async Task UnlistedCyclesCompleteWithoutChangingAuthoredPolicy(string mainRoot, bool twoCatalogues)
    {
        using var files = TemporaryWorkspace.Create("route-list-outside-cycle");
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, mainRoot).WithOutsideRoutes([Outside]);
        files.WriteText(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{Outside}}"]}
            """);
        files.WriteText(SourceLogicalPath.WorkspaceEntryPath, "# Independent cycle workspace\n");
        files.WriteText(layout.LoaderPath, GeneratedLoaderDocumentBuilder.Build(string.Empty));
        var destination = Path.GetFileName(twoCatalogues ? Other : Anchor);
        var firstRows = $"- [{FirstDescription}]({destination}) - #{SourceLoadingClosureResolver.LoadNowTag} #{FirstTag} - applies to `src/**`\n"
            + $"- [Later authored edge](./{destination}) - #{SourceLoadingClosureResolver.KeepInMindTag} #{LaterTag} - applies to `tests/**`\n";
        files.WriteText(Anchor, Catalogue("Inventory metadata", firstRows));
        if (twoCatalogues)
        {
            files.WriteText(Other, Catalogue("Other inventory metadata",
                $"- [{BackDescription}]({Path.GetFileName(Anchor)}) - #{SourceLoadingClosureResolver.LoadNowTag}\n"));
        }

        var before = files.SnapshotHashes(includeRuntime: true);
        var directories = Directory.GetDirectories(files.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
        foreach (var selected in twoCatalogues ? new[] { Anchor, Other } : [Anchor])
            foreach (var depth in new[] { RouteListDepth.Finite(0), RouteListDepth.Finite(1), RouteListDepth.All })
            {
                var reference = SourceWorkspaceRelativePath.LiteralReferencePrefix + selected;
                var request = new RouteListRequest(workspace, reference, depth);
                var resolver = new RouteListSourceResolver(new RouteListInventoryReader(),
                    new RouteListSelectionResolver(new PhysicalPathResolver()), new SourceRouteFactsResolver());
                var source = await resolver.ResolveAsync(request, TestContext.Current.CancellationToken);
                var authoredTarget = twoCatalogues ? Other : Anchor;
                var first = Assert.Single(source.IncludedSources, item => item.Base.CanonicalPath == authoredTarget);
                Assert.Equal(Anchor, first.IncludedParentPath);
                Assert.Equal(FirstDescription, first.IncludedEntry!.Description);
                Assert.Equal(new[] { SourceLoadingClosureResolver.LoadNowTag, FirstTag }, first.IncludedEntry.Tags);
                var repeated = source.IncludedRows.Where(row => row.ParentPath == Anchor && row.Path == authoredTarget).ToArray();
                Assert.Equal(2, repeated.Length);
                Assert.Equal(new[] { SourceLoadingClosureResolver.LoadNowTag, FirstTag }, repeated[0].Entry.Tags);
                Assert.Equal(new[] { SourceLoadingClosureResolver.KeepInMindTag, LaterTag }, repeated[1].Entry.Tags);
                Assert.Equal(new[] { "src/**" }, repeated[0].Entry.ApplyTo.Select(pattern => pattern.Text));
                Assert.Equal(new[] { "tests/**" }, repeated[1].Entry.ApplyTo.Select(pattern => pattern.Text));
                Assert.Equal(new[] { destination, "./" + destination }, repeated.Select(row => row.Entry.Destination));
                var roots = await resolver.ResolveLoaderRootAsync(request, source, TestContext.Current.CancellationToken);
                var result = new RouteListResultComposer(new RouteListTopologySelector(), new RouteListCoverageBuilder(), new RouteListResultBuilder())
                    .Build(request, source, roots, TestContext.Current.CancellationToken);
                var includesChild = twoCatalogues && (depth.Kind == RouteListDepthKind.All || depth.FiniteValue > 0);
                var expected = includesChild ? new[] { selected, selected == Anchor ? Other : Anchor } : [selected];
                var expectedParent = selected == Anchor && twoCatalogues ? Other : Anchor;

                Assert.Equal(CliSemanticStatus.Complete, result.Status);
                Assert.Equal(expected, result.Rows.Select(row => row.Path));
                Assert.Equal(RouteListSelectionProvenance.DetachedRoot, result.Rows[0].Provenance.Selection);
                Assert.Equal(0, result.Rows[0].RelativeDepth);
                Assert.Equal(expectedParent, result.Rows[0].ParentPath);
                Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + expectedParent, result.Rows[0].ParentId);
                Assert.Equal(1, result.Rows[0].DirectChildCount);
                Assert.All(result.Rows, row =>
                {
                    Assert.Null(row.AbsoluteDepth);
                    Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + row.Path, row.Id);
                    Assert.Equal(Outside, row.MaintenanceRoot);
                    Assert.Equal(row.Path == authoredTarget ? FirstDescription : BackDescription, row.Description);
                });
                Assert.Equal(Anchor, first.IncludedParentPath);
                if (twoCatalogues)
                {
                    Assert.Equal(Other, Assert.Single(source.IncludedSources, item => item.Base.CanonicalPath == Anchor).IncludedParentPath);
                }
                if (includesChild)
                {
                    Assert.Equal(selected, result.Rows[1].ParentPath);
                    Assert.Equal(RouteListSelectionProvenance.Descendant, result.Rows[1].Provenance.Selection);
                    Assert.Equal(1, result.Rows[1].RelativeDepth);
                    Assert.Equal(1, result.Rows[1].DirectChildCount);
                }

                var run = await CliHostCapture.RunAsync([RouteDefinitions.RouteGroup.Name, RouteListDefinitions.ListCommand.Name, reference,
                RouteListDefinitions.Depth.Name, depth.MachineValue, CliSyntaxDefinitions.Format.Name,
                CliPresentationDefinitions.JsonFormat, CliSyntaxDefinitions.Detail.Name, CliPresentationDefinitions.Full], files.Path);
                var complete = CliStatusDefinitions.Read(CliSemanticStatus.Complete);
                Assert.Equal(complete.Disposition.ExitCode, run.ExitCode);
                Assert.Empty(run.Error);
                using var json = JsonDocument.Parse(run.Output);
                Assert.Equal(complete.MachineName, json.RootElement.GetProperty(CliJsonFields.Status).GetString());
                Assert.Empty(json.RootElement.GetProperty(CliJsonFields.Effects).EnumerateArray());
                var rows = json.RootElement.GetProperty(CliJsonFields.Data).GetProperty(PublishedTokens.RouteListData.Rows).EnumerateArray().ToArray();
                Assert.Equal(expected, rows.Select(row => row.GetProperty(CliJsonFields.Path).GetString()));
                Assert.All(rows, row => Assert.Equal(JsonValueKind.Null, row.GetProperty(PublishedTokens.RouteListData.AbsoluteDepth).ValueKind));
                Assert.Equal(before, files.SnapshotHashes(includeRuntime: true));
            }

        var bare = await RouteListOperationFactory.Create()(new RouteListRequest(workspace, null, RouteListDepth.All),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, bare.Status);
        Assert.Empty(bare.Rows);
        Assert.Equal(before, files.SnapshotHashes(includeRuntime: true));
        Assert.Equal(directories, Directory.GetDirectories(files.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot)]
    [InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task ALeafRetainsItsParentWhenOnlyAnAncestorCycles(string mainRoot)
    {
        using var files = TemporaryWorkspace.Create("route-list-cycle-descendant");
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, mainRoot).WithOutsideRoutes([Outside]);
        files.WriteText(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{Outside}}"]}
            """);
        files.WriteText(SourceLogicalPath.WorkspaceEntryPath, "# Independent cycle descendant workspace\n");
        files.WriteText(layout.LoaderPath, GeneratedLoaderDocumentBuilder.Build(string.Empty));
        files.WriteText(Anchor, Catalogue("Inventory anchor",
            $"- [Self]({Path.GetFileName(Anchor)}) - #{SourceLoadingClosureResolver.LoadNowTag}\n"
                + $"- [{FirstDescription}]({Path.GetFileName(Other)}) - #{SourceLoadingClosureResolver.LoadNowTag} #{FirstTag}\n"));
        files.WriteText(Other, OpenForgeDocumentSeed.Metadata("Inventory leaf", ["Inventory"], "# Independent leaf\n"));
        var before = files.SnapshotHashes(includeRuntime: true);

        var result = await RouteListOperationFactory.Create()(new RouteListRequest(workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + Other, RouteListDepth.All), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var row = Assert.Single(result.Rows);
        Assert.Equal(Other, row.Path);
        Assert.Equal(Anchor, row.ParentPath);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + Anchor, row.ParentId);
        Assert.Null(row.AbsoluteDepth);
        Assert.Equal(RouteListSelectionProvenance.DetachedRoot, row.Provenance.Selection);
        Assert.Equal(FirstDescription, row.Description);
        Assert.Equal(new[] { SourceLoadingClosureResolver.LoadNowTag, FirstTag }, row.Tags);
        Assert.Equal(before, files.SnapshotHashes(includeRuntime: true));
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task BranchedCycleKeepsSelectedParentFactsSeparateFromDepthFirstSiblingOrder(string mainRoot)
    {
        using var files = TemporaryWorkspace.Create("route-list-branched-cycle");
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, mainRoot).WithOutsideRoutes([Outside]);
        files.WriteText(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{Outside}}"]}
            """);
        files.WriteText(SourceLogicalPath.WorkspaceEntryPath, "# Independent branched cycle workspace\n");
        files.WriteText(layout.LoaderPath, GeneratedLoaderDocumentBuilder.Build(string.Empty));
        const string earlier = Outside + "/a.md";
        const string later = Outside + "/b.md";
        const string grandchild = Outside + "/a-child.md";
        files.WriteText(Anchor, Catalogue("Inventory parent",
            $"- [Selected edge]({Path.GetFileName(Other)}) - #{SourceLoadingClosureResolver.LoadNowTag}\n"
            + $"- [Later sibling]({Path.GetFileName(later)}) - #{FirstTag}\n"
            + $"- [Earlier sibling]({Path.GetFileName(earlier)}) - #{FirstTag}\n"));
        files.WriteText(Other, Catalogue("Inventory selected catalogue",
            $"- [Parent return]({Path.GetFileName(Anchor)}) - #{SourceLoadingClosureResolver.LoadNowTag}\n"));
        files.WriteText(earlier, Catalogue("Inventory earlier catalogue",
            $"- [Grandchild]({Path.GetFileName(grandchild)}) - #{FirstTag}\n"));
        files.WriteText(later, OpenForgeDocumentSeed.Metadata("Inventory later leaf", ["Inventory"], "# Later leaf\n"));
        files.WriteText(grandchild, OpenForgeDocumentSeed.Metadata("Inventory grandchild", ["Inventory"], "# Grandchild\n"));
        var before = files.SnapshotHashes(includeRuntime: true);
        var directories = Directory.GetDirectories(files.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
        var reference = SourceWorkspaceRelativePath.LiteralReferencePrefix + Other;
        foreach (var depth in new[] { RouteListDepth.Finite(0), RouteListDepth.Finite(1), RouteListDepth.Finite(2), RouteListDepth.All })
        {
            var result = await RouteListOperationFactory.Create()(new RouteListRequest(workspace, reference, depth), TestContext.Current.CancellationToken);
            var expected = depth.Kind == RouteListDepthKind.All ? new[] { Other, Anchor, earlier, grandchild, later }
                : depth.FiniteValue == 0 ? [Other] : depth.FiniteValue == 1 ? [Other, Anchor] : [Other, Anchor, earlier, later];
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(expected, result.Rows.Select(row => row.Path));
            Assert.Equal(Anchor, result.Rows[0].ParentPath);
            Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + Anchor, result.Rows[0].ParentId);
            Assert.Equal(1, result.Rows[0].DirectChildCount);
            Assert.Equal(RouteListSelectionProvenance.DetachedRoot, result.Rows[0].Provenance.Selection);
            var parents = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [Other] = Anchor,
                [Anchor] = Other,
                [earlier] = Anchor,
                [later] = Anchor,
                [grandchild] = earlier,
            };
            foreach (var row in result.Rows)
            {
                Assert.Equal(parents[row.Path], row.ParentPath);
                Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + parents[row.Path], row.ParentId);
                Assert.Null(row.AbsoluteDepth);
                Assert.Equal(Outside, row.MaintenanceRoot);
                if (row.Path == Anchor) Assert.Equal(3, row.DirectChildCount);
                if (row.Path == earlier) Assert.Equal(1, row.DirectChildCount);
            }
            var run = await CliHostCapture.RunAsync([RouteDefinitions.RouteGroup.Name, RouteListDefinitions.ListCommand.Name,
                reference, RouteListDefinitions.Depth.Name, depth.MachineValue, CliSyntaxDefinitions.Format.Name,
                CliPresentationDefinitions.JsonFormat, CliSyntaxDefinitions.Detail.Name, CliPresentationDefinitions.Full], files.Path);
            Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Complete).Disposition.ExitCode, run.ExitCode);
            Assert.Empty(run.Error);
            using var json = JsonDocument.Parse(run.Output);
            var rows = json.RootElement.GetProperty(CliJsonFields.Data).GetProperty(PublishedTokens.RouteListData.Rows).EnumerateArray().ToArray();
            Assert.Equal(expected, rows.Select(row => row.GetProperty(CliJsonFields.Path).GetString()));
            Assert.Equal(before, files.SnapshotHashes(includeRuntime: true));
        }
        Assert.Equal(directories, Directory.GetDirectories(files.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
    }

    private static string Catalogue(string description, string rows)
        => OpenForgeDocumentSeed.Metadata(description, ["Inventory"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Prefix = "# Independent catalogue\n\nKeep these authored bytes.",
            Entries = rows,
            LineEnding = "\n",
        }));
}
