using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteRemoveProcessTests
{
    private const string MainRoot = "handbook/context";
    private const string Loader = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string Category = "notes";
    private const string CategoryDirectory = MainRoot + "/" + Category;
    private const string CategoryEntrypoint = CategoryDirectory + "/" + PublishedTokens.Layout.IndexFile;
    private const string Child = "retired";
    private const string ChildPath = CategoryDirectory + "/" + Child + PublishedTokens.Layout.MarkdownExtension;
    private const string ChildOverwrite = CategoryDirectory + "/" + Child + PublishedTokens.Layout.OverwriteSuffix;
    private const string SkillsEntrypoint = PublishedTokens.Layout.NativeSkillsPath + "/" + PublishedTokens.Layout.IndexFile;
    private const string SkillName = "workshop";
    private const string SkillPath = PublishedTokens.Layout.NativeSkillsPath + "/" + SkillName + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string ResourceName = "sample.bin";
    private const string ResourcePath = CategoryDirectory + "/" + ResourceName;
    private const string PreservedPath = "local.txt";
    private const string DefaultShadow = PublishedTokens.Sources.AgentsRoot + "/" + Category + "/" + Child + PublishedTokens.Layout.MarkdownExtension;
    private const string FixtureDescription = "Retired workspace material";
    private const string FixtureTag = "Guide";
    private const string PreservedBody = "Keep this exact CRLF content.\r\n";

    [Theory(DisplayName = "Published Route Remove previews and removes custom-root and native Skills categories, then verifies persisted absence")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter)]
    [InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", "route-remove"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootRemovalPreservesControlsAndConverges(string frontmatter)
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootRemovalPreservesControlsAndConverges));
        using var locks = PublishedWorkspaceLockStore.Create(nameof(CustomRootRemovalPreservesControlsAndConverges));
        _ = locks.Track(workspace.Path);
        workspace.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},
             "{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}",
             "{{PublishedTokens.Settings.FrontmatterProperty}}":"{{frontmatter}}"}
            """);
        workspace.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries(
            $"- [{Category}]({Category}/{PublishedTokens.Layout.IndexFile}) - #{FixtureTag}\n" +
            $"- [{PublishedTokens.Layout.SkillsDirectory}](../../{SkillsEntrypoint}) - #{FixtureTag}"));
        workspace.WriteText(CategoryEntrypoint, OpenForgeDocumentSeed.Metadata(FixtureDescription, [FixtureTag],
            OpenForgeDocumentSeed.GeneratedEntries($"- [{Child}]({Child}{PublishedTokens.Layout.MarkdownExtension}) - #{FixtureTag}")));
        workspace.WriteText(ChildPath, OpenForgeDocumentSeed.Metadata(FixtureDescription, [FixtureTag], FixtureDescription));
        workspace.WriteText(ChildOverwrite, OpenForgeDocumentSeed.Metadata(FixtureDescription, [FixtureTag], PreservedBody));
        workspace.WriteBytes(ResourcePath, [0, 127, 255]);
        workspace.WriteText(SkillsEntrypoint, OpenForgeDocumentSeed.Metadata(FixtureDescription, [FixtureTag],
            OpenForgeDocumentSeed.GeneratedEntries($"- [{SkillName}]({SkillName}/{PublishedTokens.Layout.NativeSkillFileName}) - #{FixtureTag}")));
        workspace.WriteText(SkillPath, OpenForgeDocumentSeed.Skill(SkillName, FixtureDescription));
        workspace.WriteText(PreservedPath, PreservedBody);
        workspace.WriteText(DefaultShadow, PreservedBody);
        var preserved = new[] { PreservedPath, DefaultShadow }
            .ToDictionary(path => path, path => File.ReadAllBytes(workspace.Combine(path)));
        var target = PublishedExecutableTarget.Discover();
        var reports = new Dictionary<string, string>();
        foreach (var category in new[] { Category, PublishedTokens.Layout.SkillsDirectory })
        {
            var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
                RemoveArguments(category, PublishedTokens.RouteRemove.DryRun), locks.EnvironmentVariables);
            AssertCustomRemoveCompleted(preview);
            var applied = await PublishedProcessTestSupport.RunAsync(target, workspace.Path,
                [PublishedTokens.Route.Command, PublishedTokens.RouteRemove.Command, category, PublishedTokens.RouteRemove.Automatic,
                    PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.FullDetail], locks.EnvironmentVariables);
            Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, applied.ExitCode);
            Assert.Empty(applied.StandardError);
            reports[category] = CommandOutputNormalization.ReplaceDelimitedPath(applied.StandardOutput, workspace.Path, "<workspace>");
        }
        Assert.False(Directory.Exists(workspace.Combine(CategoryDirectory)));
        Assert.False(Directory.Exists(workspace.Combine(PublishedTokens.Layout.NativeSkillsPath)));
        Assert.True(File.Exists(workspace.Combine(Loader)));
        Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
        Assert.False(File.Exists(workspace.Combine(PublishedTokens.Ownership.RelativePath)));
        foreach (var (path, bytes) in preserved) Assert.Equal(bytes, File.ReadAllBytes(workspace.Combine(path)));
        using (var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath))))
        {
            var root = settings.RootElement;
            Assert.Equal(MainRoot, root.GetProperty(PublishedTokens.Settings.MainRootProperty).GetString());
            Assert.Equal(frontmatter, root.GetProperty(PublishedTokens.Settings.FrontmatterProperty).GetString());
            Assert.Equal([Category, PublishedTokens.Layout.SkillsDirectory],
                root.GetProperty(PublishedTokens.Settings.RemovedCategoriesProperty).EnumerateArray().Select(item => item.GetString()));
            Assert.Equal([CategoryDirectory, PublishedTokens.Layout.NativeSkillsPath],
                root.GetProperty(PublishedTokens.Settings.RemovedDirectoriesProperty).EnumerateArray().Select(item => item.GetString()));
        }
        CommandOutputSnapshot.MatchDetailSnapshot(reports,
            testName: nameof(CustomRootRemovalPreservesControlsAndConverges) + "-" + frontmatter);
        foreach (var source in new[] { Category, PublishedTokens.Layout.LiteralReferencePrefix + SkillsEntrypoint })
            AssertCustomRemoveCompleted(await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
                RemoveArguments(source, PublishedTokens.RouteRemove.Automatic), locks.EnvironmentVariables));
    }

    private static string[] RemoveArguments(string source, string policy)
        => [PublishedTokens.Route.Command, PublishedTokens.RouteRemove.Command, source, policy,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];

    private static void AssertCustomRemoveCompleted(ProcessRunResult result)
    {
        Assert.True(result.ExitCode == PublishedTokens.Statuses.CompleteExitCode, result.StandardOutput + result.StandardError);
        Assert.Empty(result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
    }
}
