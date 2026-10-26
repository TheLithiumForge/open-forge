using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteInspectProcessTests
{
    private const string MainRoot = "knowledge/nested";
    private const string Route = "practice";
    private const string Topic = "review";
    private const string Skill = "investigate";
    private const string Reference = "evidence";
    private const string Description = "Inspection fixture";
    private const string RoutePath = MainRoot + "/" + Route + "/" + PublishedTokens.Layout.IndexFile;
    private const string TopicPath = MainRoot + "/" + Route + "/" + Topic + PublishedTokens.Layout.MarkdownExtension;
    private const string OverwritePath = MainRoot + "/" + Route + "/" + Topic + PublishedTokens.Layout.OverwriteSuffix;
    private const string LoaderPath = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string SkillsPath = PublishedTokens.Layout.NativeSkillsPath + "/" + PublishedTokens.Layout.IndexFile;
    private const string SkillDirectory = PublishedTokens.Layout.NativeSkillsPath + "/" + Skill;
    private const string SkillPath = SkillDirectory + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string ReferencesPath = SkillDirectory + "/" + PublishedTokens.Layout.UnderscoreReferencesFileName;
    private const string ReferencePath = SkillDirectory + "/" + Reference + PublishedTokens.Layout.MarkdownExtension;

    [Theory(DisplayName = "Published Route Inspect follows a nested root from route selection through overwrites and native Skill references"),
     InlineData(PublishedTokens.Settings.RootFrontmatter), InlineData(PublishedTokens.Settings.ScopedFrontmatter),
     Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootInspectionJourneyPreservesBytes(string frontmatter)
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootInspectionJourneyPreservesBytes));
        workspace.WriteText(PublishedTokens.Settings.RelativePath,
            $$"""{"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}","{{PublishedTokens.Settings.FrontmatterProperty}}":"{{frontmatter}}"}""");
        workspace.WriteText(LoaderPath, $"""
            # Context

            ## Axioms

            - Preserve the evidence.

            ## {PublishedTokens.Metadata.Entries}

            - [Practice]({Route}/{PublishedTokens.Layout.IndexFile}) - #{PublishedTokens.Metadata.LoadNow}
            - [Skills](../../{SkillsPath}) - #{PublishedTokens.Metadata.LoadNow}
            """);
        var metadata = frontmatter == PublishedTokens.Settings.ScopedFrontmatter
            ? OpenForgeDocumentSeed.MetadataFrontmatter(Description, [PublishedTokens.Metadata.LoadNow]) + "\n"
            : $"---\n{PublishedTokens.Metadata.Description}: {Description}\n{PublishedTokens.Metadata.Tags}: [{PublishedTokens.Metadata.LoadNow}]\n---\n";
        workspace.WriteText(RoutePath, metadata + $"""
            # Practice

            ## Axioms

            - Review the current source.

            ## {PublishedTokens.Metadata.Entries}

            - [Review]({Topic}{PublishedTokens.Layout.MarkdownExtension}) - #{PublishedTokens.Metadata.LoadNow}
            """);
        workspace.WriteText(TopicPath, metadata + "# Review\n\nCheck the evidence.\n");
        workspace.WriteText(OverwritePath, "# Review customization\r\n\r\nRetain exact bytes.\r\n");
        workspace.WriteText(SkillsPath, metadata + $"# Skills\n\n## {PublishedTokens.Metadata.Entries}\n\n- [Investigate]({Skill}/{PublishedTokens.Layout.NativeSkillFileName}) - #{PublishedTokens.Metadata.LoadNow}\n");
        workspace.WriteText(SkillPath, $"---\n{PublishedTokens.Metadata.SkillName}: {Skill}\n{PublishedTokens.Metadata.Description}: Investigate evidence\n---\n# Investigate\n");
        workspace.WriteText(ReferencesPath, metadata + $"# References\n\n## {PublishedTokens.Metadata.Entries}\n\n- [Evidence]({Reference}{PublishedTokens.Layout.MarkdownExtension}) - #{PublishedTokens.Metadata.LoadNow}\n");
        workspace.WriteText(ReferencePath, metadata + "# Evidence\n\nRetain the source.\n");
        var before = Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories)
            .ToDictionary(path => path, File.ReadAllBytes, StringComparer.Ordinal);
        var target = PublishedExecutableTarget.Discover();

        foreach (var (operand, id, path, atStart, exitCode, status, capture) in new[]
        {
            (Route, Route, RoutePath, true, 0, PublishedTokens.Statuses.Complete, "route"),
            (PublishedTokens.Layout.LiteralReferencePrefix + OverwritePath, $"{Route}/{Topic}", TopicPath, true, 0, PublishedTokens.Statuses.Complete, "overwrite"),
            (PublishedTokens.Layout.LiteralReferencePrefix + SkillPath, $"{PublishedTokens.Layout.SkillsDirectory}/{Skill}", SkillPath, false, 2, PublishedTokens.Statuses.Attention, "skill"),
            (PublishedTokens.Layout.LiteralReferencePrefix + ReferencePath, $"{PublishedTokens.Layout.SkillsDirectory}/{Skill}/{Reference}", ReferencePath, false, 0, PublishedTokens.Statuses.Complete, "reference"),
        })
        {
            var result = await PublishedProcessTestSupport.RunAsync(target, workspace.Path,
                [PublishedTokens.Route.Command, PublishedTokens.RouteInspect.Command, operand,
                    PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat,
                    PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.FullDetail]);
            Assert.Equal(exitCode, result.ExitCode);
            Assert.Empty(result.StandardError);
            using var json = JsonDocument.Parse(result.StandardOutput);
            Assert.Equal(status, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
            var data = json.RootElement.GetProperty(PublishedTokens.Json.Data);
            Assert.Equal(id, data.GetProperty(PublishedTokens.Json.Id).GetString());
            Assert.Equal(path, data.GetProperty(PublishedTokens.Json.Path).GetString());
            Assert.Equal(atStart, data.GetProperty(PublishedTokens.RouteInspectData.Read).GetProperty(PublishedTokens.RouteInspectData.AtStart).GetBoolean());
            CommandOutputSnapshot.MatchSnapshot(data.GetRawText(), $"{frontmatter}-{capture}.json");
        }

        Assert.Equal(before.Keys.Order(StringComparer.Ordinal), Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
        foreach (var file in before) Assert.Equal(file.Value, File.ReadAllBytes(file.Key));
    }
}
