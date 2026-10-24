using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteCreateProcessTests
{
    private const string MainRoot = "handbook/context";
    private const string Loader = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string RouteName = "notes";
    private const string Parent = MainRoot + "/" + RouteName + "/" + PublishedTokens.Layout.IndexFile;
    private const string NestedName = "releases";
    private const string NoteName = "checklist";
    private const string NoteId = RouteName + "/" + NestedName + "/" + NoteName;
    private const string NotePath = MainRoot + "/" + NoteId + PublishedTokens.Layout.MarkdownExtension;
    private const string NestedPath = MainRoot + "/" + RouteName + "/" + NestedName;
    private const string NestedCatalogue = NestedPath + "/_" + NestedName + PublishedTokens.Layout.MarkdownExtension;
    private const string SkillName = "workshop";
    private const string SkillDirectory = PublishedTokens.Layout.NativeSkillsPath + "/" + SkillName;
    private const string Skill = SkillDirectory + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string References = "references";
    private const string ReferenceDirectory = SkillDirectory + "/" + References;
    private const string ReferenceCatalogue = ReferenceDirectory + "/" + PublishedTokens.Layout.UnderscoreReferencesFileName;
    private const string ReferenceId = PublishedTokens.Layout.SkillsDirectory + "/" + SkillName + "/" + References + "/" + NoteName;
    private const string ReferencePath = ReferenceDirectory + "/" + NoteName + PublishedTokens.Layout.MarkdownExtension;
    private const string Description = "Release checklist";
    private const string FixtureTag = "Docs";
    private const string NativeBody = "# Workshop\r\n\r\nKeep native instructions byte for byte.\r\n";

    [Theory(DisplayName = "Published Route Create follows a nested custom root and native Skills through preview, creation and no-op")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter)]
    [InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", "route-create"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootAndNativeSkillCreationConverge(string frontmatter)
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootAndNativeSkillCreationConverge));
        using var lockStore = PublishedWorkspaceLockStore.Create(nameof(CustomRootAndNativeSkillCreationConverge));
        _ = lockStore.Track(workspace.Path);
        workspace.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},
             "{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}",
             "{{PublishedTokens.Settings.FrontmatterProperty}}":"{{frontmatter}}"}
            """);
        workspace.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries(
            $"- [{RouteName}]({RouteName}/{PublishedTokens.Layout.IndexFile}) - #{FixtureTag}\n" +
            $"- [{SkillName}](../../{ReferenceCatalogue}) - #{FixtureTag}"));
        workspace.WriteText(Parent, OpenForgeDocumentSeed.Metadata(Description, [FixtureTag], OpenForgeDocumentSeed.GeneratedEntries(string.Empty)));
        workspace.WriteText(Skill, OpenForgeDocumentSeed.Skill(SkillName, Description, NativeBody));
        workspace.WriteText(ReferenceCatalogue, OpenForgeDocumentSeed.Metadata(Description, [FixtureTag], OpenForgeDocumentSeed.GeneratedEntries(string.Empty)));
        var preserved = new[] { Loader, Skill, PublishedTokens.Settings.RelativePath }
            .ToDictionary(path => path, path => File.ReadAllBytes(workspace.Combine(path)));
        var target = PublishedExecutableTarget.Discover();
        var reports = new Dictionary<string, string>();
        try
        {
            foreach (var sourceId in new[] { NoteId, ReferenceId })
            {
                var arguments = CreateArguments(sourceId);
                AssertCustomCompleted(await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
                    [.. arguments, PublishedTokens.RouteCreate.DryRun], lockStore.EnvironmentVariables));
                reports[sourceId] = AssertCustomCompleted(await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, lockStore.EnvironmentVariables));
            }
            Assert.True(File.Exists(workspace.Combine(NotePath)));
            Assert.True(File.Exists(workspace.Combine(ReferencePath)));
            foreach (var (path, bytes) in preserved) Assert.Equal(bytes, File.ReadAllBytes(workspace.Combine(path)));
            Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
            Assert.False(Directory.Exists(workspace.Combine(MainRoot + "/" + PublishedTokens.Layout.SkillsDirectory)));
            CommandOutputSnapshot.MatchDetailSnapshot(new Dictionary<string, string>
            {
                [NoteId] = reports[NoteId],
                [ReferenceId] = reports[ReferenceId],
                [Parent] = File.ReadAllText(workspace.Combine(Parent)),
                [NestedCatalogue] = File.ReadAllText(workspace.Combine(NestedCatalogue)),
                [NotePath] = File.ReadAllText(workspace.Combine(NotePath)),
                [ReferenceCatalogue] = File.ReadAllText(workspace.Combine(ReferenceCatalogue)),
                [ReferencePath] = File.ReadAllText(workspace.Combine(ReferencePath)),
            }, testName: nameof(CustomRootAndNativeSkillCreationConverge) + "-" + frontmatter);
            foreach (var path in new[] { NotePath, ReferencePath })
                AssertCustomCompleted(await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
                    CreateArguments(PublishedTokens.Layout.LiteralReferencePrefix + path), lockStore.EnvironmentVariables));
        }
        finally
        {
            foreach (var path in new[] { NotePath, NestedCatalogue, ReferencePath })
                if (File.Exists(workspace.Combine(path))) File.Delete(workspace.Combine(path));
            foreach (var path in new[] { NestedPath })
                if (Directory.Exists(workspace.Combine(path))) Directory.Delete(workspace.Combine(path));
        }
    }

    private static string[] CreateArguments(string target)
        => [PublishedTokens.Route.Command, PublishedTokens.RouteCreate.Command, target,
            PublishedTokens.RouteCreate.Description, Description, PublishedTokens.RouteCreate.Tag, FixtureTag,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];

    private static string AssertCustomCompleted(ProcessRunResult result)
    {
        Assert.True(result.ExitCode == 0, result.StandardOutput + result.StandardError);
        Assert.Empty(result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        return json.RootElement.GetProperty(PublishedTokens.Json.Data).GetRawText();
    }
}
