using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedContextProcessTests
{
    private const string MainRoot = "docs/context";
    private const string Topic = "topic";
    private const string Route = "topics";
    private const string RoutePath = MainRoot + "/" + Route + "/" + PublishedTokens.Layout.IndexFile;
    private const string SkillsPath = PublishedTokens.Layout.NativeSkillsPath + "/" + PublishedTokens.Layout.IndexFile;
    private const string Skill = "research";
    private const string TopicPath = MainRoot + "/" + Route + "/" + Topic + PublishedTokens.Layout.MarkdownExtension;
    private const string SkillPath = PublishedTokens.Layout.NativeSkillsPath + "/" + Skill + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string LoaderPath = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string GuidePath = MainRoot + "/" + PublishedTokens.Layout.GuideFile;

    [Fact, Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootStartupAndLinkedGuideKeepNativeSkillsAndBytes()
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootStartupAndLinkedGuideKeepNativeSkillsAndBytes));
        workspace.WriteText(PublishedTokens.Settings.RelativePath,
            $$"""{"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}"}""");
        workspace.WriteText(PublishedTokens.Layout.AgentHost, $"Read `{LoaderPath}`.\n");
        workspace.WriteText(LoaderPath, $"""
            # Context

            ## {PublishedTokens.Metadata.Entries}

            - [Topics]({Route}/{PublishedTokens.Layout.IndexFile}) - #{PublishedTokens.Metadata.LoadNow}
            - [Skills](../../{SkillsPath}) - #{PublishedTokens.Metadata.LoadNow}
            """);
        var catalogueMetadata = $"---\n{PublishedTokens.Metadata.Description}: Catalogue\n{PublishedTokens.Metadata.Tags}: [{PublishedTokens.Metadata.LoadNow}]\n---\n";
        workspace.WriteText(RoutePath, catalogueMetadata + $"# Topics\n\n## {PublishedTokens.Metadata.Entries}\n\n- [Topic]({Topic}{PublishedTokens.Layout.MarkdownExtension}) - #{PublishedTokens.Metadata.LoadNow}\n");
        workspace.WriteText(SkillsPath, catalogueMetadata + $"# Skills\n\n## {PublishedTokens.Metadata.Entries}\n\n- [Skill]({Skill}/{PublishedTokens.Layout.NativeSkillFileName}) - #{PublishedTokens.Metadata.LoadNow}\n");
        workspace.WriteText(TopicPath, $"""
            ---
            {PublishedTokens.Metadata.Description}: Topic
            {PublishedTokens.Metadata.Tags}: [{PublishedTokens.Metadata.LoadNow}]
            ---
            # Topic

            [Guide](../{PublishedTokens.Layout.GuideFile})
            """);
        workspace.WriteText(GuidePath, "# Human guide\n");
        workspace.WriteText(SkillPath, $"""
            ---
            {PublishedTokens.Metadata.SkillName}: {Skill}
            {PublishedTokens.Metadata.Description}: Research
            {PublishedTokens.Metadata.Tags}: [{PublishedTokens.Metadata.LoadNow}]
            ---
            # Research
            """);
        var before = Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories)
            .ToDictionary(path => path, File.ReadAllBytes, StringComparer.Ordinal);

        var result = await PublishedProcessTestSupport.RunAsync(PublishedExecutableTarget.Discover(), workspace.Path,
            [PublishedTokens.Context.Command, $"{Route}/{Topic}", $"{PublishedTokens.Layout.SkillsDirectory}/{Skill}", PublishedTokens.Context.Content, PublishedTokens.Context.Frontmatter,
                PublishedTokens.Context.FollowLinks, PublishedTokens.Context.All,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        var sources = json.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.ContextData.Sources).EnumerateArray().ToArray();
        Assert.Equal(new[] { PublishedTokens.Layout.AgentHost, LoaderPath, RoutePath, TopicPath, SkillsPath, SkillPath, GuidePath }.Order(StringComparer.Ordinal),
            sources.Select(source => source.GetProperty(PublishedTokens.Json.Path).GetString()).Order(StringComparer.Ordinal));
        var skill = Assert.Single(sources, source => source.GetProperty(PublishedTokens.Json.Path).GetString() == SkillPath);
        Assert.Equal($"{PublishedTokens.Layout.SkillsDirectory}/{Skill}", skill.GetProperty(PublishedTokens.Json.Id).GetString());
        Assert.Equal(before.Keys.Order(StringComparer.Ordinal), Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
        foreach (var file in before) Assert.Equal(file.Value, File.ReadAllBytes(file.Key));
    }
}
