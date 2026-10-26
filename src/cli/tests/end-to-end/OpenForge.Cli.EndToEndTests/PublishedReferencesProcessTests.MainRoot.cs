using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedReferencesProcessTests
{
    private const string MainRoot = "docs/references";
    private const string Topic = "topic";
    private const string Target = "target";
    private const string Skill = "research";
    private const string TopicPath = MainRoot + "/" + Topic + PublishedTokens.Layout.MarkdownExtension;
    private const string TargetPath = MainRoot + "/" + Target + PublishedTokens.Layout.MarkdownExtension;
    private const string SkillPath = PublishedTokens.Layout.NativeSkillsPath + "/" + Skill + "/" + PublishedTokens.Layout.NativeSkillFileName;
    private const string LoaderPath = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;

    [Fact, Trait("Feature", "references"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootReferencesResolveBothDirectionsAcrossNativeSkills()
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootReferencesResolveBothDirectionsAcrossNativeSkills));
        workspace.WriteText(PublishedTokens.Settings.RelativePath,
            $$"""{"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}"}""");
        workspace.WriteText(LoaderPath, $"# References\n\n## {PublishedTokens.Metadata.Entries}\n\n- [Topic]({Topic}{PublishedTokens.Layout.MarkdownExtension})\n");
        workspace.WriteText(TopicPath, $"# Topic\n\n[Target]({Target}{PublishedTokens.Layout.MarkdownExtension})\n");
        workspace.WriteText(TargetPath, "# Target\n");
        workspace.WriteText(SkillPath, $"# Research\n\n[Topic](../../../{TopicPath})\n");
        var before = Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories)
            .ToDictionary(path => path, File.ReadAllBytes, StringComparer.Ordinal);

        var result = await PublishedProcessTestSupport.RunAsync(PublishedExecutableTarget.Discover(), workspace.Path,
            [PublishedTokens.References.Command, Topic,
                PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.FullDetail,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        var data = json.RootElement.GetProperty(PublishedTokens.Json.Data);
        Assert.Equal(Topic, data.GetProperty(PublishedTokens.ReferencesData.Source).GetProperty(PublishedTokens.Json.Id).GetString());
        Assert.Equal(TopicPath, data.GetProperty(PublishedTokens.ReferencesData.Source).GetProperty(PublishedTokens.Json.Path).GetString());
        var outgoing = Assert.Single(data.GetProperty(PublishedTokens.ReferencesData.Outgoing).EnumerateArray());
        Assert.Equal(TargetPath, outgoing.GetProperty(PublishedTokens.ReferencesData.ResolvedPath).GetString());
        var incoming = data.GetProperty(PublishedTokens.ReferencesData.Incoming).EnumerateArray().ToArray();
        Assert.Equal(new[] { SkillPath }.Order(StringComparer.Ordinal),
            incoming.Select(source => source.GetProperty(PublishedTokens.Json.Path).GetString()).Order(StringComparer.Ordinal));
        var skill = Assert.Single(data.GetProperty(PublishedTokens.ReferencesData.Scanned).EnumerateArray(), source => source.GetProperty(PublishedTokens.Json.Path).GetString() == SkillPath);
        Assert.Equal($"{PublishedTokens.Layout.SkillsDirectory}/{Skill}", skill.GetProperty(PublishedTokens.Json.Id).GetString());
        Assert.Equal(before.Keys.Order(StringComparer.Ordinal), Directory.GetFiles(workspace.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
        foreach (var file in before) Assert.Equal(file.Value, File.ReadAllBytes(file.Key));
    }
}
