using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport.Contracts;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context.Shared.Graph;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Context.Shared.Graph;

[Trait("Boundary", "Host"), Trait("Feature", ContextDefinitions.CommandIdentity), Trait("Evidence", "Integration")]
public sealed class ContextOutsideGraphIntegrationTests
{
    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(ContextOutsideWorkspace.CustomRoot)]
    public async Task RegistrationPreservesRowsFirstParentAndOnePhysicalSource(string mainRoot)
    {
        using var workspace = new ContextOutsideWorkspace(mainRoot, alias: true);
        var builder = new ContextGraphBuilder();
        var initial = await builder.BuildAsync(workspace.Layout.Workspace, TestContext.Current.CancellationToken);
        var bytes = workspace.Files.SnapshotHashes(includeRuntime: true);
        var before = await Read(workspace, ContextDefinitions.For.Name, "src/App.cs");
        Assert.Equal(bytes, workspace.Files.SnapshotHashes(includeRuntime: true));
        workspace.Register();
        bytes = workspace.Files.SnapshotHashes(includeRuntime: true);
        var registered = await builder.BuildAsync(workspace.Layout.Workspace, TestContext.Current.CancellationToken);
        var after = await Read(workspace, ContextDefinitions.For.Name, "src/App.cs");

        var expected = new[] { SourceLogicalPath.WorkspaceEntryPath, workspace.Layout.LoaderPath,
            workspace.Layout.SourcePath(WorkspaceRoutePath.CanonicalEntrypointPath("rules")), ContextOutsideWorkspace.Skills, ContextOutsideWorkspace.Anchor, ContextOutsideWorkspace.Leaf,
            ContextOutsideWorkspace.Nested, ContextOutsideWorkspace.Deep };
        Assert.Equal(expected, Paths(before));
        Assert.Equal(expected, Paths(after));
        Assert.Equal(before.GetRawText(), after.GetRawText());
        foreach (var graph in new[] { initial, registered })
        {
            Assert.Equal(new[] { "First outside", "Later outside", "First rule", "Later rule", "Nested", "Deep", "Other parent" },
                graph.Catalogue.IncludedRows.Select(row => row.Entry.Description));
            var leaf = Assert.Single(graph.Sources, source => source.CanonicalPath == ContextOutsideWorkspace.Leaf);
            Assert.Same(leaf, graph.FindByPath(ContextOutsideWorkspace.Alias));
            Assert.Equal(ContextOutsideWorkspace.Anchor, leaf.LinkedParentPath);
            Assert.Equal(ContextOutsideWorkspace.LeafCondition, Assert.Single(leaf.LinkedApplyTo.Patterns).Text);
            Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + ContextOutsideWorkspace.Leaf,
                graph.Catalogue.FindByPath(ContextOutsideWorkspace.Leaf)?.Identity.AutomaticId);
            Assert.Equal(2, graph.Catalogue.IncludedRows.Count(row => row.ParentPath == ContextOutsideWorkspace.Anchor
                && graph.FindByPath(row.Path) == leaf));
            Assert.Equal(1, graph.Sources.Count(source => source.Layers[0].PhysicalPath == leaf.Layers[0].PhysicalPath));
        }
        Assert.Equal(initial.Catalogue.IncludedRows.Select(row => (row.Path, row.ParentPath, row.Entry.Description, row.Entry.Destination)),
            registered.Catalogue.IncludedRows.Select(row => (row.Path, row.ParentPath, row.Entry.Description, row.Entry.Destination)));
        Assert.Equal(ContextOutsideWorkspace.LeafBody, registered.FindByPath(ContextOutsideWorkspace.Leaf)?.Layers[0].Text);
        Assert.Equal(bytes, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(ContextOutsideWorkspace.CustomRoot)]
    public async Task RegistrationDoesNotActivateUnlistedContentAndExactSelectionStaysExplicit(string mainRoot)
    {
        using var workspace = new ContextOutsideWorkspace(mainRoot, listed: false);
        var before = await Read(workspace);
        workspace.Register();
        var bytes = workspace.Files.SnapshotHashes(includeRuntime: true);
        var after = await Read(workspace);
        var expected = new[] { SourceLogicalPath.WorkspaceEntryPath, workspace.Layout.LoaderPath,
            workspace.Layout.SourcePath(WorkspaceRoutePath.CanonicalEntrypointPath("rules")), ContextOutsideWorkspace.Skills };
        Assert.Equal(expected, Paths(before));
        Assert.Equal(expected, Paths(after));
        var explicitRead = await Read(workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + ContextOutsideWorkspace.Unlisted);
        Assert.Equal(expected.Append(ContextOutsideWorkspace.Unlisted), Paths(explicitRead));
        var catalogue = await Read(workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + ContextOutsideWorkspace.Anchor, ContextDefinitions.For.Name, "src/App.cs");
        Assert.Equal(expected.Concat([ContextOutsideWorkspace.Anchor, ContextOutsideWorkspace.Leaf, ContextOutsideWorkspace.Nested, ContextOutsideWorkspace.Deep]), Paths(catalogue));
        Assert.Equal(bytes, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(ContextOutsideWorkspace.CustomRoot)]
    public async Task WorkingPathsAndLinkedSourcesRetainIncludedPolicy(string mainRoot)
    {
        using var workspace = new ContextOutsideWorkspace(mainRoot);
        var matching = await Read(workspace, ContextDefinitions.For.Name, "src/App.cs", ContextDefinitions.FollowLinks.Name, "1");
        var nonmatching = await Read(workspace, ContextDefinitions.For.Name, "tests/App.cs");
        var pending = await Read(workspace, expectedExit: 3);
        workspace.Register();
        var bytes = workspace.Files.SnapshotHashes(includeRuntime: true);
        Assert.Equal(Paths(matching), Paths(await Read(workspace, ContextDefinitions.For.Name, "src/App.cs", ContextDefinitions.FollowLinks.Name, "1")));
        Assert.Equal(nonmatching.GetRawText(), (await Read(workspace, ContextDefinitions.For.Name, "tests/App.cs")).GetRawText());
        Assert.Equal(pending.GetRawText(), (await Read(workspace, expectedExit: 3)).GetRawText());
        Assert.Contains(ContextOutsideWorkspace.Unlisted, Paths(matching));
        Assert.DoesNotContain(ContextOutsideWorkspace.Anchor, Paths(nonmatching));
        var selected = await Read(workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + ContextOutsideWorkspace.Leaf,
            ContextDefinitions.For.Name, "tests/App.cs");
        Assert.Contains(ContextOutsideWorkspace.Leaf, Paths(selected));
        Assert.DoesNotContain(ContextOutsideWorkspace.Deep, Paths(selected));
        Assert.Equal(bytes, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(ContextOutsideWorkspace.CustomRoot)]
    public async Task LinkedLeafRetainsItsIncludedCatalogueAncestorsBeforeAndAfterRegistration(string mainRoot)
    {
        using var workspace = new ContextOutsideWorkspace(mainRoot);
        var primary = workspace.Layout.SourcePath(WorkspaceRoutePath.CanonicalEntrypointPath("rules"));
        var prefix = string.Concat(Enumerable.Repeat("../", mainRoot.Split('/').Length));
        var skillsLink = workspace.Layout.IsDefault
            ? WorkspaceLayoutNames.SkillsDirectoryName + "/" + Path.GetFileName(ContextOutsideWorkspace.Skills)
            : prefix + ContextOutsideWorkspace.Skills;
        workspace.Files.ReplaceText(workspace.Layout.LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(string.Join('\n',
            ContextOutsideWorkspace.Row("Primary", "rules/" + Path.GetFileName(primary)),
            ContextOutsideWorkspace.Row("Skills", skillsLink),
            $"- [On demand]({prefix}{ContextOutsideWorkspace.Anchor})")));
        workspace.Files.ReplaceText(primary, OpenForgeDocumentSeed.Metadata("Primary", [SourceLoadingClosureResolver.LoadNowTag],
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Prefix = $"# Primary\n\n[Deep](../{prefix}{ContextOutsideWorkspace.Deep})",
                Entries = string.Empty,
            })));
        var before = await Read(workspace, ContextDefinitions.For.Name, "src/App.cs", ContextDefinitions.FollowLinks.Name, "1");
        workspace.Register();
        var after = await Read(workspace, ContextDefinitions.For.Name, "src/App.cs", ContextDefinitions.FollowLinks.Name, "1");

        var expected = new[] { SourceLogicalPath.WorkspaceEntryPath, workspace.Layout.LoaderPath, primary,
            ContextOutsideWorkspace.Skills, ContextOutsideWorkspace.Anchor, ContextOutsideWorkspace.Nested, ContextOutsideWorkspace.Deep };
        Assert.Equal(expected, Paths(before));
        Assert.Equal(expected, Paths(after));
        Assert.Equal(before.GetRawText(), after.GetRawText());
    }

    private static Task<JsonElement> Read(ContextOutsideWorkspace workspace, params string[] arguments)
        => Read(workspace, arguments, 0);

    private static Task<JsonElement> Read(ContextOutsideWorkspace workspace, int expectedExit)
        => Read(workspace, [], expectedExit);

    private static async Task<JsonElement> Read(ContextOutsideWorkspace workspace, string[] arguments, int expectedExit)
    {
        var preserved = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await CliHostCapture.RunAsync([ContextDefinitions.CommandIdentity, .. arguments, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat, CliSyntaxDefinitions.Detail.Name, CliPresentationDefinitions.Full], workspace.Files.Path);
        Assert.True(result.ExitCode == expectedExit, result.Output + result.Error);
        Assert.Empty(result.Error);
        Assert.Equal(preserved, workspace.Files.SnapshotHashes(includeRuntime: true));
        Assert.False(System.IO.Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Layout.Workspace)));
        using var json = JsonDocument.Parse(result.Output);
        return json.RootElement.GetProperty(PublishedTokens.Json.Data).Clone();
    }

    private static string?[] Paths(JsonElement data)
        => data.GetProperty(PublishedTokens.ContextData.Sources).EnumerateArray().Select(source => source.GetProperty(PublishedTokens.Json.Path).GetString()).ToArray();
}
