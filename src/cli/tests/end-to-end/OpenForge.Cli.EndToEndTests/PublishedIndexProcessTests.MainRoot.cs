using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedIndexProcessTests
{
    private const string MainRoot = "handbook/forge";
    private const string MainLoader = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string GuideDirectory = "guides";
    private const string GuideCatalogue = GuideDirectory + "/_guides.md";
    private const string MainCatalogue = MainRoot + "/" + GuideCatalogue;
    private const string NoteFile = "note.md";
    private const string MainNote = MainRoot + "/" + GuideDirectory + "/" + NoteFile;
    private const string SkillName = "workshop";
    private const string SkillsCatalogueName = "_skills.md";
    private const string SkillsCatalogue = PublishedTokens.Layout.NativeSkillsPath + "/" + SkillsCatalogueName;
    private const string SkillDirectory = PublishedTokens.Layout.NativeSkillsPath + "/" + SkillName;
    private const string NativeSkill = SkillDirectory + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string ReferenceDirectory = SkillDirectory + "/references";
    private const string ReferenceCatalogue = ReferenceDirectory + "/" + PublishedTokens.Layout.UnderscoreReferencesFileName;
    private const string NestedDirectory = ReferenceDirectory + "/nested";
    private const string NestedCatalogue = NestedDirectory + "/_nested.md";
    private const string ReferenceNote = NestedDirectory + "/" + NoteFile;
    private const string GuideDescription = "Workspace guides";
    private const string SkillDescription = "Workshop procedures";
    private const string ReferenceDescription = "Workshop references";
    private const string NestedDescription = "Nested procedures";
    private const string FixtureTag = "Docs";
    private const string StaleEntries = "- stale";
    private const string NoteBody = "# Release checklist\r\n\r\nKeep the authored body.\r\n";
    private const string NativeBody = "# Workshop\r\n\r\nNative instructions stay exact.\r\n";
    private const string NativeRelativeRoot = "../../" + PublishedTokens.Layout.NativeSkillsPath;

    [Theory(DisplayName = "Published Index maintains a custom root and native catalogues without reactivating unlisted Skills")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter)]
    [InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", "index-command"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootMaintainsNativeCataloguesAndPreservesUnlistedSkills(string frontmatter)
    {
        using var workspace = TemporaryWorkspace.Create("published-index-main-root");
        using var lockStore = PublishedWorkspaceLockStore.Create("published-index-main-root-lock");
        _ = lockStore.Track(workspace.Path);
        workspace.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {
              "{{PublishedTokens.Settings.SchemaVersionProperty}}": {{PublishedTokens.Settings.SchemaVersion}},
              "{{PublishedTokens.Settings.MainRootProperty}}": "{{MainRoot}}",
              "{{PublishedTokens.Settings.FrontmatterProperty}}": "{{frontmatter}}"
            }
            """);
        var guideRow = $"- [{GuideDescription}]({GuideCatalogue}) - #{FixtureTag}";
        var skillRow = $"- [{SkillDescription}]({NativeRelativeRoot}/{SkillsCatalogueName}) - #{FixtureTag}";
        workspace.WriteText(MainLoader, OpenForgeDocumentSeed.GeneratedEntries(guideRow + "\n" + skillRow));
        workspace.WriteText(MainCatalogue, Catalogue(GuideDescription));
        workspace.WriteText(MainNote, NoteBody);
        workspace.WriteText(SkillsCatalogue, OpenForgeDocumentSeed.SkillEntrypoint([SkillName]));
        workspace.WriteText(NativeSkill, OpenForgeDocumentSeed.Skill(SkillName, SkillDescription, NativeBody));
        workspace.WriteText(ReferenceCatalogue, Catalogue(ReferenceDescription));
        workspace.WriteText(NestedCatalogue, Catalogue(NestedDescription));
        workspace.WriteText(ReferenceNote, OpenForgeDocumentSeed.Metadata(NestedDescription, [FixtureTag], NoteBody));
        var preserved = new[] { PublishedTokens.Settings.RelativePath, NativeSkill, ReferenceNote }
            .ToDictionary(path => path, path => File.ReadAllBytes(workspace.Combine(path)));
        var target = PublishedExecutableTarget.Discover();
        string[] arguments = [PublishedTokens.Index.Command, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];

        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
            [.. arguments, PublishedTokens.Index.DryRun], lockStore.EnvironmentVariables);
        AssertCompleted(preview);
        lockStore.AssertNoInfrastructure();
        AssertCompleted(await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, lockStore.EnvironmentVariables));
        foreach (var (path, bytes) in preserved) Assert.Equal(bytes, File.ReadAllBytes(workspace.Combine(path)));
        Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
        Assert.False(Directory.Exists(workspace.Combine(MainRoot + "/" + Path.GetFileName(PublishedTokens.Layout.NativeSkillsPath))));
        CommandOutputSnapshot.MatchDetailSnapshot(new Dictionary<string, string>
        {
            ["loader"] = File.ReadAllText(workspace.Combine(MainLoader)),
            ["guides"] = File.ReadAllText(workspace.Combine(MainCatalogue)),
            ["completed-note"] = File.ReadAllText(workspace.Combine(MainNote)),
            ["references"] = File.ReadAllText(workspace.Combine(ReferenceCatalogue)),
            ["nested"] = File.ReadAllText(workspace.Combine(NestedCatalogue)),
        }, testName: nameof(CustomRootMaintainsNativeCataloguesAndPreservesUnlistedSkills) + "-" + frontmatter);
        AssertCompleted(await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
            arguments, lockStore.EnvironmentVariables));

        workspace.ReplaceText(MainLoader, OpenForgeDocumentSeed.GeneratedEntries(guideRow));
        workspace.ReplaceText(ReferenceCatalogue, Catalogue(ReferenceDescription));
        var unlisted = new[] { MainLoader, NativeSkill }
            .ToDictionary(path => path, path => File.ReadAllBytes(workspace.Combine(path)));
        AssertCompleted(await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, lockStore.EnvironmentVariables));
        AssertCompleted(await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
            arguments, lockStore.EnvironmentVariables));
        foreach (var (path, bytes) in unlisted) Assert.Equal(bytes, File.ReadAllBytes(workspace.Combine(path)));
    }

    private static string Catalogue(string description)
        => OpenForgeDocumentSeed.Metadata(description, [FixtureTag], OpenForgeDocumentSeed.GeneratedEntries(StaleEntries));

    private static void AssertCompleted(ProcessRunResult result)
    {
        Assert.True(result.ExitCode == 0, result.StandardOutput + result.StandardError);
        Assert.Empty(result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
    }
}
