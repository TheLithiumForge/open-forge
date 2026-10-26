using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteExcludeProcessTests
{
    private const string MainRoot = "knowledge/project";
    private const string LoaderPath = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string ForeignPath = "handbook/team" + PublishedTokens.Layout.MarkdownExtension;
    private const string SkillsCatalogue = PublishedTokens.Layout.NativeSkillsPath + "/_" + PublishedTokens.Layout.SkillsDirectory + PublishedTokens.Layout.MarkdownExtension;

    [Fact(DisplayName = "Published custom Exclude removes native and foreign rows while Index leaves them excluded"), Trait("Feature", "route-exclude"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootExclusionSurvivesIndex()
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootExclusionSurvivesIndex));
        workspace.WriteText(PublishedTokens.Settings.RelativePath,
            $$"""{"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}","{{PublishedTokens.Settings.RootsProperty}}":["{{ForeignPath}}"]}""");
        workspace.WriteText(LoaderPath, $"""
            # Context

            ## {PublishedTokens.Metadata.Entries}

            - [Skills](../../{SkillsCatalogue}) - #{PublishedTokens.Metadata.LoadNow}
            - [Handbook](../../{ForeignPath}) - #{PublishedTokens.Metadata.LoadNow}
            """);
        workspace.WriteText(ForeignPath, "# Handbook\r\n\r\nKeep original bytes.\r\n");
        workspace.WriteText(SkillsCatalogue, $"""
            ---
            {PublishedTokens.Metadata.Description}: Skills
            {PublishedTokens.Metadata.Tags}: [{PublishedTokens.Metadata.LoadNow}]
            ---
            # Skills

            ## {PublishedTokens.Metadata.Entries}

            {PublishedTokens.Metadata.EmptyEntry}

            """);
        var before = new[] { ForeignPath, SkillsCatalogue }.ToDictionary(path => path, path => File.ReadAllBytes(workspace.Combine(path)));
        var target = PublishedExecutableTarget.Discover();
        foreach (var path in before.Keys)
        {
            string[] args = [PublishedTokens.Route.Command, PublishedTokens.RouteExclude.Command, path,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];
            var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
                [.. args, PublishedTokens.RouteExclude.DryRun]);
            Assert.Equal(0, preview.ExitCode);
            var result = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, args);
            Assert.True(result.ExitCode == 0, result.StandardOutput + result.StandardError);
            using var json = JsonDocument.Parse(result.StandardOutput);
            Assert.Null(json.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.Json.After).GetString());
        }
        var excludedLoader = File.ReadAllBytes(workspace.Combine(LoaderPath));
        var index = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, [PublishedTokens.Index.Command]);
        Assert.True(index.ExitCode == 0, index.StandardOutput + index.StandardError);
        Assert.Equal(excludedLoader, File.ReadAllBytes(workspace.Combine(LoaderPath)));
        using var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath)));
        Assert.Empty(settings.RootElement.GetProperty(PublishedTokens.Settings.RootsProperty).EnumerateArray());
        foreach (var file in before) Assert.Equal(file.Value, File.ReadAllBytes(workspace.Combine(file.Key)));
        Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
    }
}
