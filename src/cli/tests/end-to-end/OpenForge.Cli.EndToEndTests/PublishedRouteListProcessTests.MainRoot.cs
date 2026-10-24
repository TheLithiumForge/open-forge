using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using static OpenForge.Cli.EndToEndTests.Shared.PublishedProcess.PublishedProcessTestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteListProcessTests
{
    private const string MainRoot = "docs/team/context";
    private const string Route = "policies";
    private const string CatalogueName = "_" + Route + PublishedTokens.Layout.MarkdownExtension;
    private const string CataloguePath = MainRoot + "/" + Route + "/" + CatalogueName;
    private const string LoaderPath = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string Leaf = "conduct";
    private const string LeafId = Route + "/" + Leaf;
    private const string LeafPath = MainRoot + "/" + LeafId + PublishedTokens.Layout.MarkdownExtension;
    private const string OverwritePath = MainRoot + "/" + LeafId + PublishedTokens.Layout.OverwriteSuffix;
    private const string SkillsCatalogue = PublishedTokens.Layout.NativeSkillsPath + "/_" + PublishedTokens.Layout.SkillsDirectory + PublishedTokens.Layout.MarkdownExtension;
    private const string Skill = "review";
    private const string SkillId = PublishedTokens.Layout.SkillsDirectory + "/" + Skill;
    private const string SkillPath = PublishedTokens.Layout.NativeSkillsPath + "/" + Skill + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string IncludedPath = "handbook/overview" + PublishedTokens.Layout.MarkdownExtension;
    private const string FixtureTag = "Policy";
    private const string DocumentBody = "# Workspace instructions\r\n\r\nKeep these bytes.\r\n";

    [Theory, InlineData(PublishedTokens.Settings.RootFrontmatter), InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootDiscoveryAndExactSelectionsPreserveNativeSkillsAndBytes(string frontmatter)
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootDiscoveryAndExactSelectionsPreserveNativeSkillsAndBytes));
        workspace.WriteText(PublishedTokens.Settings.RelativePath,
            $$"""{"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}","{{PublishedTokens.Settings.FrontmatterProperty}}":"{{frontmatter}}"}""");
        workspace.WriteText(LoaderPath, $"""
            # Loader

            ## {PublishedTokens.Metadata.Entries}

            - [Policies]({Route}/{CatalogueName}) - #{FixtureTag}
            - [Skills](../../../{SkillsCatalogue}) - #{FixtureTag}
            - [Overview](../../../{IncludedPath}) - #{FixtureTag}
            """);
        var metadata = frontmatter == PublishedTokens.Settings.ScopedFrontmatter
            ? OpenForgeDocumentSeed.MetadataFrontmatter(Route, [FixtureTag])
            : $"---\n{PublishedTokens.Metadata.Description}: {Route}\n{PublishedTokens.Metadata.Tags}: [{FixtureTag}]\n---";
        workspace.WriteText(CataloguePath, metadata + "\n" + DocumentBody);
        workspace.WriteText(LeafPath, metadata + "\n" + DocumentBody);
        workspace.WriteText(OverwritePath, DocumentBody);
        workspace.WriteText(SkillsCatalogue, metadata + "\n" + DocumentBody);
        workspace.WriteText(SkillPath, $"---\n{PublishedTokens.Metadata.SkillName}: {Skill}\n{PublishedTokens.Metadata.Description}: {Skill}\n---\n" + DocumentBody);
        workspace.WriteText(IncludedPath, DocumentBody);
        var before = Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories)
            .ToDictionary(path => path, File.ReadAllBytes, StringComparer.Ordinal);
        var target = PublishedExecutableTarget.Discover();

        var discovery = await RunAsync(target, workspace.Path,
            [PublishedTokens.Route.Command, PublishedTokens.RouteList.Command,
                PublishedTokens.RouteList.Depth, PublishedTokens.RouteList.AllDepth,
                PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.FullDetail,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);

        Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, discovery.ExitCode);
        Assert.Empty(discovery.StandardError);
        using var json = JsonDocument.Parse(discovery.StandardOutput);
        Assert.Equal(PublishedTokens.RouteList.CommandIdentity, json.RootElement.GetProperty(PublishedTokens.Json.Command).GetString());
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        var rows = json.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.RouteListData.Rows).EnumerateArray().ToArray();
        Assert.Equal(new[] { CataloguePath, LeafPath, SkillsCatalogue, SkillPath, IncludedPath }.Order(StringComparer.Ordinal),
            rows.Select(row => row.GetProperty(PublishedTokens.Json.Path).GetString()).Order(StringComparer.Ordinal));
        Assert.Equal(new[] { Route, LeafId, PublishedTokens.Layout.SkillsDirectory, SkillId, PublishedTokens.Layout.LiteralReferencePrefix + IncludedPath }.Order(StringComparer.Ordinal),
            rows.Select(row => row.GetProperty(PublishedTokens.Json.Id).GetString()).Order(StringComparer.Ordinal));
        Assert.True(Assert.Single(rows, row => row.GetProperty(PublishedTokens.Json.Path).GetString() == LeafPath)
            .GetProperty(PublishedTokens.RouteListData.HasOverwrite).GetBoolean());

        foreach (var (reference, path) in new[]
        {
            (LeafId, LeafPath),
            (PublishedTokens.Layout.LiteralReferencePrefix + OverwritePath, LeafPath),
            (SkillId, SkillPath),
            (PublishedTokens.Layout.LiteralReferencePrefix + IncludedPath, IncludedPath),
        })
        {
            var selected = await RunAsync(target, workspace.Path,
                [PublishedTokens.Route.Command, PublishedTokens.RouteList.Command, reference,
                    PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);
            Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, selected.ExitCode);
            Assert.Empty(selected.StandardError);
            using var selection = JsonDocument.Parse(selected.StandardOutput);
            var row = Assert.Single(selection.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.RouteListData.Rows).EnumerateArray());
            Assert.Equal(path, row.GetProperty(PublishedTokens.Json.Path).GetString());
        }

        Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
        Assert.False(Directory.Exists(workspace.Combine(MainRoot + "/" + PublishedTokens.Layout.SkillsDirectory)));
        Assert.Equal(before.Keys.Order(StringComparer.Ordinal), Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
        foreach (var (path, bytes) in before) Assert.Equal(bytes, File.ReadAllBytes(path));
    }
}
