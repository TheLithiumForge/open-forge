using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed partial class RootRemoveRecoveryConsumerJourneyTests
{
    private const string MainRoot = "handbook/context";
    private const string Loader = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string SkillName = "workshop";
    private const string SkillDirectory = PublishedTokens.Layout.NativeSkillsPath + "/" + SkillName;
    private const string Skill = SkillDirectory + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string TargetName = "sample.bin";
    private const string Target = MainRoot + "/" + TargetName;
    private const string FormerRootTarget = PublishedTokens.Sources.AgentsRoot + "/" + TargetName;
    private const string Description = "Workshop instructions";
    private const string FixtureTag = "Docs";
    private static readonly byte[] PreservedBytes = [0, 255, 128, 13, 10];

    [Theory(DisplayName = "Published Remove preserves fixed controls and custom content while removing native Skills and updating navigation")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter)]
    [InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootRemovalRemainsAbsentAfterIndex(string frontmatter)
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootRemovalRemainsAbsentAfterIndex));
        using var locks = PublishedWorkspaceLockStore.Create(nameof(CustomRootRemovalRemainsAbsentAfterIndex));
        _ = locks.Track(workspace.Path);
        workspace.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},
             "{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}",
             "{{PublishedTokens.Settings.FrontmatterProperty}}":"{{frontmatter}}"}
            """);
        workspace.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries($"- [{SkillName}](../../{Skill}) - #{FixtureTag}"));
        workspace.WriteText(Skill, OpenForgeDocumentSeed.Skill(SkillName, Description, string.Empty));
        workspace.WriteBytes(Target, PreservedBytes);
        workspace.WriteBytes(FormerRootTarget, PreservedBytes);
        var executable = PublishedExecutableTarget.Discover();

        foreach (var path in new[] { SkillDirectory, Target })
        {
            var arguments = RemoveArguments(path);
            AssertMainRootCompleted(await PublishedProcessTestSupport.RunWithoutWritesAsync(executable, workspace.Path, workspace.SnapshotHashes,
                [.. arguments, PublishedTokens.Install.DryRun], locks.EnvironmentVariables));
            AssertMainRootCompleted(await PublishedProcessTestSupport.RunAsync(executable, workspace.Path, arguments, locks.EnvironmentVariables));
        }
        Assert.False(Directory.Exists(workspace.Combine(SkillDirectory)));
        Assert.False(File.Exists(workspace.Combine(Target)));
        Assert.Equal(PreservedBytes, File.ReadAllBytes(workspace.Combine(FormerRootTarget)));
        using (var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath))))
        {
            Assert.Equal(MainRoot, settings.RootElement.GetProperty(PublishedTokens.Settings.MainRootProperty).GetString());
            Assert.Contains(settings.RootElement.GetProperty(PublishedTokens.Settings.RemovedDirectoriesProperty).EnumerateArray(),
                item => item.GetString() == SkillDirectory);
            Assert.Contains(settings.RootElement.GetProperty(PublishedTokens.Settings.RemovedFilesProperty).EnumerateArray(),
                item => item.GetString() == Target);
        }
        var settingsBeforeIndex = File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath));
        var loaderBeforeIndex = File.ReadAllBytes(workspace.Combine(Loader));
        AssertMainRootCompleted(await PublishedProcessTestSupport.RunAsync(executable, workspace.Path,
            [PublishedTokens.Index.Command, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], locks.EnvironmentVariables));
        Assert.Equal(settingsBeforeIndex, File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath)));
        Assert.Equal(loaderBeforeIndex, File.ReadAllBytes(workspace.Combine(Loader)));
        Assert.False(Directory.Exists(workspace.Combine(SkillDirectory)));
        Assert.False(File.Exists(workspace.Combine(Target)));
        Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
        Assert.False(Directory.Exists(workspace.Combine(MainRoot + "/" + PublishedTokens.Layout.SkillsDirectory)));
        CommandOutputSnapshot.MatchDetailSnapshot(new Dictionary<string, string>
        {
            [Loader] = File.ReadAllText(workspace.Combine(Loader)),
        }, testName: nameof(CustomRootRemovalRemainsAbsentAfterIndex) + "-" + frontmatter);
        foreach (var path in new[] { SkillDirectory, Target })
            AssertMainRootCompleted(await PublishedProcessTestSupport.RunWithoutWritesAsync(executable, workspace.Path, workspace.SnapshotHashes,
                RemoveArguments(path), locks.EnvironmentVariables));
    }

    private static string[] RemoveArguments(string target)
        => [PublishedTokens.Remove.Command, target, PublishedTokens.Remove.Automatic,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];

    private static void AssertMainRootCompleted(ProcessRunResult result)
    {
        Assert.True(result.ExitCode == PublishedTokens.Statuses.CompleteExitCode, result.StandardOutput + result.StandardError);
        Assert.Empty(result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, document.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
    }
}
