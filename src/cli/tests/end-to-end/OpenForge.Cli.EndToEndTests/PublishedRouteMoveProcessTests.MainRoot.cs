using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteMoveProcessTests
{
    private const string MainRoot = "handbook/context";
    private const string Loader = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string RouteName = "notes";
    private const string Archive = "archive";
    private const string Topic = "topic";
    private const string Leaf = "guide";
    private const string Child = "child";
    private const string Resource = "image.bin";
    private const string SourceId = RouteName + "/" + Leaf;
    private const string Source = MainRoot + "/" + SourceId + PublishedTokens.Layout.MarkdownExtension;
    private const string Overwrite = MainRoot + "/" + SourceId + PublishedTokens.Layout.OverwriteSuffix;
    private const string Parent = MainRoot + "/" + RouteName + "/" + PublishedTokens.Layout.IndexFile;
    private const string ArchiveParent = MainRoot + "/" + Archive + "/" + PublishedTokens.Layout.IndexFile;
    private const string CategoryId = RouteName + "/" + Topic;
    private const string CategoryDirectory = MainRoot + "/" + CategoryId;
    private const string Category = CategoryDirectory + "/" + PublishedTokens.Layout.IndexFile;
    private const string MovedCategoryDirectory = MainRoot + "/" + Archive + "/" + Topic;
    private const string MovedCategory = MovedCategoryDirectory + "/" + PublishedTokens.Layout.IndexFile;
    private const string SkillName = "workshop";
    private const string SkillDirectory = PublishedTokens.Layout.NativeSkillsPath + "/" + SkillName;
    private const string Skill = SkillDirectory + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string ReferencesFolder = "references";
    private const string SkillsRootCatalogue = PublishedTokens.Layout.NativeSkillsPath + "/" + PublishedTokens.Layout.IndexFile;
    private const string SkillParent = SkillDirectory + "/" + PublishedTokens.Layout.IndexFile;
    private const string SkillReferences = SkillDirectory + "/" + ReferencesFolder;
    private const string SkillCatalogue = SkillReferences + "/" + PublishedTokens.Layout.UnderscoreReferencesFileName;
    private const string DestinationId = PublishedTokens.Layout.SkillsDirectory + "/" + SkillName + "/" + ReferencesFolder + "/" + Leaf;
    private const string Destination = SkillReferences + "/" + Leaf + PublishedTokens.Layout.MarkdownExtension;
    private const string DestinationOverwrite = SkillReferences + "/" + Leaf + PublishedTokens.Layout.OverwriteSuffix;
    private const string Description = "Move fixture";
    private const string FixtureTag = "Docs";
    private const string FrameworkId = "fixture";
    private const string AuthoredBody = "# Guide\r\n\r\nPreserve authored bytes.\r\n";
    private const string OverwriteBody = "Local guidance.\r\n";
    private const string SkillBody = "# Workshop\r\n\r\nNative instructions stay here.\r\n";
    private const string OldRootSentinel = PublishedTokens.Sources.AgentsRoot + "/" + RouteName + "/" + Leaf + PublishedTokens.Layout.MarkdownExtension;
    private static readonly byte[] ResourceBytes = [0, 1, 2, 255];

    [Theory(DisplayName = "Published Route Move carries a custom-root leaf into native Skills and moves a complete category")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter)]
    [InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootMovesPreserveContentAndRewriteNavigation(string frontmatter)
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootMovesPreserveContentAndRewriteNavigation));
        using var lockStore = PublishedWorkspaceLockStore.Create(nameof(CustomRootMovesPreserveContentAndRewriteNavigation));
        _ = lockStore.Track(workspace.Path);
        SeedCustomRoot(workspace, frontmatter);
        var preserved = new[] { Skill, OldRootSentinel, PublishedTokens.Settings.RelativePath, PublishedTokens.Ownership.RelativePath }
            .ToDictionary(path => path, path => File.ReadAllBytes(workspace.Combine(path)));
        var sourceBytes = File.ReadAllBytes(workspace.Combine(Source));
        var overwriteBytes = File.ReadAllBytes(workspace.Combine(Overwrite));
        var childBytes = File.ReadAllBytes(workspace.Combine(CategoryDirectory + "/" + Child + PublishedTokens.Layout.MarkdownExtension));
        var target = PublishedExecutableTarget.Discover();
        var reports = new Dictionary<string, string>();
        try
        {
            foreach (var (source, destination) in new[]
                { (SourceId, DestinationId), (CategoryId, PublishedTokens.Layout.LiteralReferencePrefix + MovedCategory) })
            {
                var arguments = MoveArguments(source, destination);
                AssertMoveCompleted(await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
                    [.. arguments, PublishedTokens.RouteMove.DryRun], lockStore.EnvironmentVariables));
                reports[source] = AssertMoveCompleted(await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, lockStore.EnvironmentVariables));
            }
            Assert.False(File.Exists(workspace.Combine(Source)));
            Assert.False(File.Exists(workspace.Combine(Overwrite)));
            Assert.False(Directory.Exists(workspace.Combine(CategoryDirectory)));
            Assert.Equal(sourceBytes, File.ReadAllBytes(workspace.Combine(Destination)));
            Assert.Equal(overwriteBytes, File.ReadAllBytes(workspace.Combine(DestinationOverwrite)));
            Assert.Equal(childBytes, File.ReadAllBytes(workspace.Combine(MovedCategoryDirectory + "/" + Child + PublishedTokens.Layout.MarkdownExtension)));
            Assert.Equal(ResourceBytes, File.ReadAllBytes(workspace.Combine(MovedCategoryDirectory + "/" + Resource)));
            foreach (var (path, bytes) in preserved) Assert.Equal(bytes, File.ReadAllBytes(workspace.Combine(path)));
            Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
            Assert.False(Directory.Exists(workspace.Combine(MainRoot + "/" + PublishedTokens.Layout.SkillsDirectory)));
            foreach (var path in new[] { Loader, Parent, ArchiveParent, SkillsRootCatalogue, SkillParent, SkillCatalogue, MovedCategory, PublishedTokens.Layout.GuideFile })
                reports[path] = File.ReadAllText(workspace.Combine(path));
            CommandOutputSnapshot.MatchDetailSnapshot(reports,
                testName: nameof(CustomRootMovesPreserveContentAndRewriteNavigation) + "-" + frontmatter);
        }
        finally
        {
            foreach (var path in new[] { Destination, DestinationOverwrite, MovedCategory,
                MovedCategoryDirectory + "/" + Child + PublishedTokens.Layout.MarkdownExtension, MovedCategoryDirectory + "/" + Resource })
                if (File.Exists(workspace.Combine(path))) File.Delete(workspace.Combine(path));
            if (Directory.Exists(workspace.Combine(MovedCategoryDirectory))) Directory.Delete(workspace.Combine(MovedCategoryDirectory));
        }
    }

    private static void SeedCustomRoot(TemporaryWorkspace workspace, string frontmatter)
    {
        workspace.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},
             "{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}",
             "{{PublishedTokens.Settings.FrontmatterProperty}}":"{{frontmatter}}"}
            """);
        workspace.WriteText(PublishedTokens.Ownership.RelativePath, $$$"""
            {"{{{PublishedTokens.Ownership.Framework}}}":{"{{{PublishedTokens.Ownership.Source}}}":{"{{{PublishedTokens.Ownership.Id}}}":"{{{FrameworkId}}}"} } }
            """);
        workspace.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries(
            $"- [{RouteName}]({RouteName}/{PublishedTokens.Layout.IndexFile}) - #{FixtureTag}\n" +
            $"- [{Archive}]({Archive}/{PublishedTokens.Layout.IndexFile}) - #{FixtureTag}\n" +
            $"- [{SkillName}](../../{SkillsRootCatalogue}) - #{FixtureTag}"));
        workspace.WriteText(Parent, MoveDocument(OpenForgeDocumentSeed.GeneratedEntries(
            $"- [{Leaf}]({Leaf}{PublishedTokens.Layout.MarkdownExtension}) - #{FixtureTag}\n" +
            $"- [{Topic}]({Topic}/{PublishedTokens.Layout.IndexFile}) - #{FixtureTag}"), frontmatter));
        workspace.WriteText(ArchiveParent, MoveDocument(OpenForgeDocumentSeed.GeneratedEntries(string.Empty), frontmatter));
        workspace.WriteText(Source, MoveDocument(AuthoredBody, frontmatter));
        workspace.WriteText(Overwrite, OverwriteBody);
        workspace.WriteText(Category, MoveDocument(OpenForgeDocumentSeed.GeneratedEntries(
            $"- [{Child}]({Child}{PublishedTokens.Layout.MarkdownExtension}) - #{FixtureTag}"), frontmatter));
        workspace.WriteText(CategoryDirectory + "/" + Child + PublishedTokens.Layout.MarkdownExtension, MoveDocument(AuthoredBody, frontmatter));
        workspace.CreateFile(CategoryDirectory + "/" + Resource, ResourceBytes);
        workspace.WriteText(Skill, OpenForgeDocumentSeed.Skill(SkillName, Description, SkillBody));
        workspace.WriteText(SkillsRootCatalogue, MoveDocument(OpenForgeDocumentSeed.GeneratedEntries($"- [{SkillName}]({SkillName}/{PublishedTokens.Layout.IndexFile}) - #{FixtureTag}"), frontmatter));
        workspace.WriteText(SkillParent, MoveDocument(OpenForgeDocumentSeed.GeneratedEntries($"- [{ReferencesFolder}]({ReferencesFolder}/{PublishedTokens.Layout.UnderscoreReferencesFileName}) - #{FixtureTag}"), frontmatter));
        workspace.WriteText(SkillCatalogue, MoveDocument(OpenForgeDocumentSeed.GeneratedEntries(string.Empty), frontmatter));
        workspace.WriteText(OldRootSentinel, AuthoredBody);
        workspace.WriteText(PublishedTokens.Layout.GuideFile, $"[{Leaf}]({Source})\n[{Topic}]({Category})\n");
    }

    private static string MoveDocument(string body, string frontmatter)
        => frontmatter == PublishedTokens.Settings.RootFrontmatter
            ? $"---\n{PublishedTokens.Metadata.Description}: {Description}\n{PublishedTokens.Metadata.Tags}: [{FixtureTag}]\n---\n{body}"
            : OpenForgeDocumentSeed.Metadata(Description, [FixtureTag], body);

    private static string[] MoveArguments(string source, string destination)
        => [PublishedTokens.Route.Command, PublishedTokens.RouteMove.Command, source, destination,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];

    private static string AssertMoveCompleted(ProcessRunResult result)
    {
        Assert.True(result.ExitCode == PublishedTokens.Statuses.CompleteExitCode, result.StandardOutput + result.StandardError);
        Assert.Empty(result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        return json.RootElement.GetProperty(PublishedTokens.Json.Data).GetRawText();
    }
}
