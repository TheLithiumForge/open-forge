using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRepairMainRootProcessTests
{
    private const string MainRoot = "handbook/context";
    private const string Category = "notes";
    private const string Loader = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string Catalogue = MainRoot + "/" + Category + "/" + PublishedTokens.Layout.IndexFile;
    private const string Source = MainRoot + "/" + Category + "/checklist" + PublishedTokens.Layout.MarkdownExtension;
    private const string Guide = "guide" + PublishedTokens.Layout.MarkdownExtension;
    private const string SkillDirectory = PublishedTokens.Layout.NativeSkillsPath + "/workshop";
    private const string NativeCatalogue = PublishedTokens.Layout.NativeSkillsPath + "/" + PublishedTokens.Layout.IndexFile;
    private const string Skill = SkillDirectory + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string NativeSource = SkillDirectory + "/checklist" + PublishedTokens.Layout.MarkdownExtension;
    private const string Tag = "Docs";

    [Fact(DisplayName = "Published Repair follows a proven rename under a nested main root")]
    [Trait("Feature", "repair"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootRenamePreservesIdentityAndConverges()
    {
        using var files = new PublishedRepairProcessTests.RenameWorkspace();
        await files.InitializeMainRootAsync(MainRoot);
        var target = PublishedExecutableTarget.Discover();
        async Task<ProcessRunResult> Run(params string[] arguments) => await PublishedProcessTestSupport.RunAsync(
            target, files.Root, arguments, files.EnvironmentVariables);
        AssertCompleted(await Run(PublishedTokens.Index.Command, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat));
        await files.RecordTreeAsync();
        var oldPath = $"{MainRoot}/{Category}/rounding{PublishedTokens.Layout.MarkdownExtension}";
        var newPath = $"{MainRoot}/{Category}/money-rounding{PublishedTokens.Layout.MarkdownExtension}";
        File.Move(files.PathFor(oldPath), files.PathFor(newPath));
        var settings = File.ReadAllBytes(files.PathFor(PublishedTokens.Settings.RelativePath));
        var renamed = File.ReadAllBytes(files.PathFor(newPath));
        var before = files.Snapshot();
        var preview = await Run(PublishedTokens.Repair.Command, PublishedTokens.Repair.Automatic, PublishedTokens.Repair.DryRun);
        Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, preview.ExitCode);
        Assert.Equal(before, files.Snapshot());
        CommandOutputSnapshot.MatchSnapshot(preview.StandardOutput.Replace(files.Root, "<workspace>", StringComparison.Ordinal), "preview");
        AssertCompleted(await Run(PublishedTokens.Repair.Command, PublishedTokens.Repair.Automatic,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat));
        Assert.Equal(settings, File.ReadAllBytes(files.PathFor(PublishedTokens.Settings.RelativePath)));
        Assert.Equal(renamed, File.ReadAllBytes(files.PathFor(newPath)));
        Assert.False(File.Exists(files.PathFor(PublishedTokens.Sources.LoaderPath)));
        AssertCompleted(await Run(PublishedTokens.Doctor.Command, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat,
            PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.FullDetail));
        var repeated = files.Snapshot();
        AssertCompleted(await Run(PublishedTokens.Repair.Command, PublishedTokens.Repair.Automatic,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat));
        Assert.Equal(repeated, files.Snapshot());
    }

    [Theory(DisplayName = "Published Repair repairs custom-root and native Skill links, preserves settings and converges")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter)]
    [InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", "repair"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootAndNativeSkillRepairsConverge(string frontmatter)
    {
        using var files = TemporaryWorkspace.Create(nameof(CustomRootAndNativeSkillRepairsConverge));
        using var store = PublishedWorkspaceLockStore.Create(nameof(CustomRootAndNativeSkillRepairsConverge));
        _ = store.Track(files.Path);
        files.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},
             "{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}",
             "{{PublishedTokens.Settings.FrontmatterProperty}}":"{{frontmatter}}"}
            """);
        files.WriteText(PublishedTokens.Layout.AgentHost, $"# Workspace\n\nRead `{Loader}`.\n");
        files.WriteText(Loader, Navigation(
            $"- [Notes]({Category}/{PublishedTokens.Layout.IndexFile}) - #{Tag}\n" +
            $"- [Skills](../../{NativeCatalogue}) - #{Tag}"));
        files.WriteText(NativeCatalogue, Document("Skills", Navigation(
            $"- [Workshop](workshop/{PublishedTokens.Layout.NativeSkillFileName}) - #{Tag}")));
        files.WriteText(Catalogue, OpenForgeDocumentSeed.Metadata("Notes", [Tag], Navigation(string.Empty)));
        files.WriteText(Source, Document("Checklist", $"# Checklist\n\nRead [Guide](./{Guide}).\n"));
        files.WriteText($"{MainRoot}/{Category}/{Guide}", Document("Guide", "# Guide\n\nKeep the authored guide.\n"));
        files.WriteText(Skill, OpenForgeDocumentSeed.Skill("workshop", "Workshop instructions", "# Workshop\n\nKeep native instructions.\n"));
        files.WriteText(NativeSource, Document("Native checklist", $"# Native checklist\n\nRead [Guide](./{Guide}).\n"));
        files.WriteText($"{SkillDirectory}/{Guide}", Document("Native guide", "# Native guide\n\nKeep native guide bytes.\n"));
        var target = PublishedExecutableTarget.Discover();
        async Task<ProcessRunResult> Run(params string[] arguments) => await PublishedProcessTestSupport.RunAsync(
            target, files.Path, arguments, store.EnvironmentVariables);
        AssertCompleted(await Run(PublishedTokens.Index.Command, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat));
        var preserved = new[] { PublishedTokens.Settings.RelativePath, Skill, $"{MainRoot}/{Category}/{Guide}", $"{SkillDirectory}/{Guide}" }
            .ToDictionary(path => path, path => File.ReadAllBytes(files.Combine(path)));
        string[] repair = [PublishedTokens.Repair.Command, PublishedTokens.Repair.Automatic,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];

        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, files.Path, files.SnapshotHashes,
            [.. repair, PublishedTokens.Repair.DryRun], store.EnvironmentVariables);
        AssertCompleted(preview);
        using (var json = JsonDocument.Parse(preview.StandardOutput))
            Assert.NotEmpty(json.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        AssertCompleted(await Run(repair));
        foreach (var (path, bytes) in preserved) Assert.Equal(bytes, File.ReadAllBytes(files.Combine(path)));
        Assert.False(File.Exists(files.Combine(PublishedTokens.Sources.LoaderPath)));
        Assert.False(Directory.Exists(files.Combine(MainRoot, PublishedTokens.Layout.SkillsDirectory)));
        CommandOutputSnapshot.MatchDetailSnapshot(new Dictionary<string, string>
        {
            [Source] = File.ReadAllText(files.Combine(Source)),
            [NativeSource] = File.ReadAllText(files.Combine(NativeSource)),
            [Loader] = File.ReadAllText(files.Combine(Loader)),
        }, testName: nameof(CustomRootAndNativeSkillRepairsConverge) + "-" + frontmatter);
        AssertCompleted(await Run(PublishedTokens.Doctor.Command, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat,
            PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.FullDetail));
        var repeated = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, files.Path, files.SnapshotHashes, repair, store.EnvironmentVariables);
        AssertCompleted(repeated);
        using var repeatedJson = JsonDocument.Parse(repeated.StandardOutput);
        Assert.Empty(repeatedJson.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
    }

    private static string Navigation(string entries) => OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
    {
        Prefix = "# Notes\n\n## Axioms\n\n- Preserve authored content.",
        Entries = entries,
    });

    private static string Document(string description, string body) => OpenForgeDocumentSeed.Metadata(description, [Tag], body);

    private static void AssertCompleted(ProcessRunResult result)
    {
        Assert.True(result.ExitCode == PublishedTokens.Statuses.CompleteExitCode, result.StandardOutput + result.StandardError);
        Assert.Empty(result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
    }
}
