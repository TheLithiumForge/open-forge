using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteIncludeProcessTests
{
    private const string MainRoot = "docs/forge";
    private const string LoaderPath = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string ForeignPath = "team/guide" + PublishedTokens.Layout.MarkdownExtension;
    private const string SkillsCatalogue = PublishedTokens.Layout.NativeSkillsPath + "/_" + PublishedTokens.Layout.SkillsDirectory + PublishedTokens.Layout.MarkdownExtension;
    private const string Description = "Team guide";

    [Fact(DisplayName = "Published custom Include loads native Skills and literal foreign context and preserves them through Index"), Trait("Feature", "route-include"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootIncludesNativeSkillsAndForeignContext()
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootIncludesNativeSkillsAndForeignContext));
        workspace.WriteText(PublishedTokens.Settings.RelativePath,
            $$"""{"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}"}""");
        workspace.WriteText(LoaderPath, $"# Context\n\n## {PublishedTokens.Metadata.Entries}\n");
        workspace.WriteText(PublishedTokens.Layout.AgentHost, $"Read `{LoaderPath}`.\n");
        workspace.WriteText(ForeignPath, "# Team\r\n\r\nKeep this foreign document.\r\n");
        workspace.WriteText(SkillsCatalogue, $"""
            ---
            {PublishedTokens.Metadata.Description}: Research skills
            {PublishedTokens.Metadata.Tags}: [{PublishedTokens.Metadata.LoadNow}]
            ---
            # Skills

            ## {PublishedTokens.Metadata.Entries}

            {PublishedTokens.Metadata.EmptyEntry}

            """);
        var foreignBytes = File.ReadAllBytes(workspace.Combine(ForeignPath));
        var skillsBytes = File.ReadAllBytes(workspace.Combine(SkillsCatalogue));
        var target = PublishedExecutableTarget.Discover();
        async Task<ProcessRunResult> Run(string[] args)
        {
            var result = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, args);
            Assert.True(result.ExitCode == 0, result.StandardOutput + result.StandardError);
            return result;
        }
        await Run([PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, SkillsCatalogue]);
        await Run([PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, ForeignPath,
            PublishedTokens.RouteInclude.Description, Description, PublishedTokens.RouteInclude.Tag, PublishedTokens.Metadata.LoadNow]);
        var loaderBytes = File.ReadAllBytes(workspace.Combine(LoaderPath));
        await Run([PublishedTokens.Index.Command]);
        Assert.Equal(loaderBytes, File.ReadAllBytes(workspace.Combine(LoaderPath)));
        var context = await Run([PublishedTokens.Context.Command, PublishedTokens.Layout.LiteralReferencePrefix + ForeignPath,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);
        using var json = JsonDocument.Parse(context.StandardOutput);
        var sources = json.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.ContextData.Sources).EnumerateArray().ToArray();
        Assert.Single(sources, source => source.GetProperty(PublishedTokens.Json.Path).GetString() == ForeignPath);
        Assert.Contains(sources, source => source.GetProperty(PublishedTokens.Json.Path).GetString() == SkillsCatalogue);
        using var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath)));
        Assert.Equal(new[] { ForeignPath }, settings.RootElement.GetProperty(PublishedTokens.Settings.RootsProperty).EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(foreignBytes, File.ReadAllBytes(workspace.Combine(ForeignPath)));
        Assert.Equal(skillsBytes, File.ReadAllBytes(workspace.Combine(SkillsCatalogue)));
        Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
    }
}
