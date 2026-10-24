using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedFindProcessTests
{
    private const string MainRoot = "knowledge/nested";
    private const string Topic = "topic";
    private const string Skill = "research";
    private const string Heading = "Searchable";
    private const string ExcludedPath = MainRoot + "/ignored" + PublishedTokens.Layout.MarkdownExtension;
    private const string TopicPath = MainRoot + "/" + Topic + PublishedTokens.Layout.MarkdownExtension;
    private const string OverwritePath = MainRoot + "/" + Topic + PublishedTokens.Layout.OverwriteSuffix;
    private const string SkillPath = PublishedTokens.Layout.NativeSkillsPath + "/" + Skill + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string LoaderPath = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;

    [Fact, Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootSearchProjectsOverwriteAndNativeSkillWithStableIds()
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootSearchProjectsOverwriteAndNativeSkillWithStableIds));
        workspace.WriteText(PublishedTokens.Settings.RelativePath,
            $$"""{"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}"}""");
        workspace.WriteText(LoaderPath, $"# Search\n\n## {PublishedTokens.Metadata.Entries}\n");
        var document = $"""
            ---
            {PublishedTokens.Metadata.SkillName}: {Skill}
            {PublishedTokens.Metadata.Description}: {Heading}
            {PublishedTokens.Metadata.Tags}: []
            ---
            # {Heading}
            """;
        workspace.WriteText(TopicPath, document);
        workspace.WriteText(SkillPath, document);
        workspace.WriteText(ExcludedPath, document);
        workspace.WriteText(OverwritePath, "# Supplement\n");
        var before = Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories)
            .ToDictionary(path => path, File.ReadAllBytes, StringComparer.Ordinal);

        var result = await PublishedProcessTestSupport.RunAsync(PublishedExecutableTarget.Discover(), workspace.Path,
            [PublishedTokens.Find.Command, PublishedTokens.Find.Exclude, PublishedTokens.Layout.LiteralReferencePrefix + ExcludedPath, PublishedTokens.Find.Heading, Heading,
                PublishedTokens.Find.Content, $"{PublishedTokens.Find.Metadata},{PublishedTokens.Find.Body}",
                PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.FullDetail,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        var matches = json.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.FindData.Matches).EnumerateArray().ToArray();
        Assert.Equal(new[] { SkillPath, TopicPath }.Order(StringComparer.Ordinal),
            matches.Select(match => match.GetProperty(PublishedTokens.Json.Path).GetString()).Order(StringComparer.Ordinal));
        Assert.Equal(new[] { $"{PublishedTokens.Layout.SkillsDirectory}/{Skill}", Topic },
            matches.Select(match => match.GetProperty(PublishedTokens.Json.Id).GetString()));
        var topic = Assert.Single(matches, match => match.GetProperty(PublishedTokens.Json.Path).GetString() == TopicPath);
        Assert.Equal(new[] { TopicPath, OverwritePath }, topic.GetProperty(PublishedTokens.FindData.Parts).EnumerateArray()
            .Select(part => part.GetProperty(PublishedTokens.Json.Path).GetString()).Where(path => path is not null));
        Assert.Equal(before.Keys.Order(StringComparer.Ordinal), Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
        foreach (var file in before) Assert.Equal(file.Value, File.ReadAllBytes(file.Key));
    }
}
