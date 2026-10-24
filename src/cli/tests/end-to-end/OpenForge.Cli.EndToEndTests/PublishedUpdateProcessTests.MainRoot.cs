using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedUpdateProcessTests
{
    private const string MainRoot = "handbook/context";
    private const string Loader = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string Guide = MainRoot + "/" + PublishedTokens.Layout.GuideFile;
    private const string SkillName = "workshop";
    private const string Skill = PublishedTokens.Layout.NativeSkillsPath + "/" + SkillName + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string OwnerId = "fixture-framework";
    private const string PrivateRoute = "private";
    private const string PrivateCatalogue = MainRoot + "/" + PrivateRoute + "/" + PublishedTokens.Layout.IndexFile;
    private const string Note = MainRoot + "/" + PrivateRoute + "/local" + PublishedTokens.Layout.MarkdownExtension;
    private const string Description = "Local instructions";
    private const string Tag = "Local";
    private const string Body = "# Local instructions\r\n\r\nPreserve authored bytes.\r\n";

    [Theory(DisplayName = "Published Update reconciles a custom root, restores its guide and converges without a default loader")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter)]
    [InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", "update-command"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootUpdateRestoreAndRepeatJourney(string frontmatter)
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootUpdateRestoreAndRepeatJourney));
        using var lockStore = PublishedWorkspaceLockStore.Create(nameof(CustomRootUpdateRestoreAndRepeatJourney));
        _ = lockStore.Track(workspace.Path);
        workspace.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},
             "{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}",
             "{{PublishedTokens.Settings.FrontmatterProperty}}":"{{frontmatter}}"}
            """);
        workspace.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        workspace.WriteText(PrivateCatalogue, OpenForgeDocumentSeed.Metadata(Description, [PublishedTokens.Metadata.LoadNow], OpenForgeDocumentSeed.GeneratedEntries(string.Empty)));
        workspace.WriteText(Note, OpenForgeDocumentSeed.Metadata(Description, [Tag], Body));
        workspace.WriteText(Skill, OpenForgeDocumentSeed.Skill(SkillName, Description, Body));
        workspace.WriteText(PublishedTokens.Ownership.RelativePath, $$"""
            {"{{PublishedTokens.Ownership.Framework}}":{
              "{{PublishedTokens.Ownership.Source}}":{"{{PublishedTokens.Json.Id}}":"{{OwnerId}}"},
              "{{PublishedTokens.Ownership.Paths}}":["{{Loader}}"]}
            }
            """);
        var preserved = new[] { Note, Skill, PublishedTokens.Settings.RelativePath }
            .ToDictionary(path => path, path => File.ReadAllBytes(workspace.Combine(path)));
        var originalFiles = Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories).ToHashSet(StringComparer.Ordinal);
        var originalDirectories = Directory.GetDirectories(workspace.Path, "*", SearchOption.AllDirectories).ToHashSet(StringComparer.Ordinal);
        var target = PublishedExecutableTarget.Discover();
        string[] arguments = [PublishedTokens.Update.Command, PublishedTokens.Update.Automatic,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];
        try
        {
            AssertCustomUpdateCompleted(await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
                [.. arguments, PublishedTokens.Update.DryRun], lockStore.EnvironmentVariables));
            AssertCustomUpdateCompleted(await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, lockStore.EnvironmentVariables));
            var guideBytes = File.ReadAllBytes(workspace.Combine(Guide));
            File.Delete(workspace.Combine(Guide));
            AssertCustomUpdateCompleted(await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, lockStore.EnvironmentVariables));
            Assert.Equal(guideBytes, File.ReadAllBytes(workspace.Combine(Guide)));
            AssertCustomUpdateCompleted(await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
                arguments, lockStore.EnvironmentVariables));
            foreach (var (path, bytes) in preserved) Assert.Equal(bytes, File.ReadAllBytes(workspace.Combine(path)));
            Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
            Assert.False(Directory.Exists(workspace.Combine(MainRoot + "/" + PublishedTokens.Layout.SkillsDirectory)));
            using var ownership = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(PublishedTokens.Ownership.RelativePath)));
            var framework = ownership.RootElement.GetProperty(PublishedTokens.Ownership.Framework);
            Assert.Equal(OwnerId, framework.GetProperty(PublishedTokens.Ownership.Source).GetProperty(PublishedTokens.Json.Id).GetString());
            var paths = framework.GetProperty(PublishedTokens.Ownership.Paths).EnumerateArray().Select(value => value.GetString()).ToArray();
            Assert.Contains(Loader, paths);
            Assert.Contains(Guide, paths);
            Assert.DoesNotContain(PublishedTokens.Sources.LoaderPath, paths);
            Assert.DoesNotContain(Note, paths);
            Assert.DoesNotContain(Skill, paths);
            var context = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
                [PublishedTokens.Context.Command, PublishedTokens.Layout.SkillsDirectory + "/" + SkillName,
                    PublishedTokens.Context.Content, PublishedTokens.Context.Frontmatter,
                    PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], lockStore.EnvironmentVariables);
            AssertCustomUpdateCompleted(context);
            using var contextJson = JsonDocument.Parse(context.StandardOutput);
            var sources = contextJson.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.ContextData.Sources)
                .EnumerateArray().Select(source => source.GetProperty(PublishedTokens.Json.Path).GetString()).ToArray();
            Assert.Contains(Loader, sources);
            Assert.Contains(Skill, sources);
            Assert.DoesNotContain(PrivateCatalogue, sources);
            Assert.DoesNotContain(Note, sources);
        }
        finally
        {
            foreach (var path in Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories).Except(originalFiles)) File.Delete(path);
            foreach (var path in Directory.GetDirectories(workspace.Path, "*", SearchOption.AllDirectories).Except(originalDirectories).OrderByDescending(path => path.Length))
                Directory.Delete(path);
        }
    }

    private static void AssertCustomUpdateCompleted(ProcessRunResult result)
    {
        Assert.True(result.ExitCode == PublishedTokens.Statuses.CompleteExitCode, result.StandardOutput + result.StandardError);
        Assert.Empty(result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
    }
}
