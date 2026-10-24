using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OpenForge.Cli.EndToEndTests.Shared.MainRoot;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

[Trait("Feature", "install-main-root"), Trait("Evidence", "EndToEnd")]
public sealed class PublishedMainRootOfflineStartupProcessTests
{
    private const string MainRoot = "docs/team/context";
    private const string ForeignDirectory = "project";
    private const string ForeignPath = ForeignDirectory + "/handbook" + PublishedTokens.Layout.MarkdownExtension;
    private const string ForeignBody = "# Team handbook\r\n\r\nKeep the authored offline instructions.\r\n";
    private const string Description = "Team handbook";

    [Fact(DisplayName = "Both installed hosts reach the selected loader Skills and included files without the CLI or README")]
    public async Task OfflinePointersRemainSelfContained()
    {
        using var workspace = new PublishedMainRootWorkspace(MainRoot);
        using var baseline = new DefaultLayoutBaseline();
        workspace.OwnTree(ForeignDirectory);
        workspace.Write(ForeignPath, ForeignBody);
        var foreign = workspace.Read(ForeignPath);
        await workspace.Install();
        using var included = await workspace.Run(PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, ForeignPath,
            PublishedTokens.RouteInclude.Description, Description, PublishedTokens.RouteInclude.Tag, PublishedTokens.Metadata.LoadNow);
        var skills = PublishedTokens.Layout.NativeSkillsPath + "/" + PublishedTokens.Layout.SkillsEntrypoint;
        using var skillsIncluded = await workspace.Run(PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, skills);
        foreach (var withoutGuide in new[] { false, true })
        {
            if (withoutGuide) File.Delete(workspace.Combine(MainRoot + "/" + PublishedTokens.Layout.GuideFile));
            foreach (var host in new[] { PublishedTokens.Layout.AgentHost, PublishedTokens.Layout.ClaudeHost })
            {
                var expected = Encoding.UTF8.GetString(baseline.Files[host])
                    .Replace(PublishedTokens.Sources.LoaderPath, workspace.Loader, StringComparison.Ordinal);
                Assert.Equal(Encoding.UTF8.GetBytes(expected), workspace.Read(host));
            }
            var hostLoader = Assert.Single(Markdown.Parse(Encoding.UTF8.GetString(workspace.Read(PublishedTokens.Layout.AgentHost)))
                    .Descendants<CodeInline>().Select(code => code.Content),
                value => value.EndsWith("/" + PublishedTokens.Layout.LoaderFileName, StringComparison.Ordinal));
            Assert.Equal(workspace.Loader, hostLoader);
            var claudeTargets = Encoding.UTF8.GetString(workspace.Read(PublishedTokens.Layout.ClaudeHost)).Split('\n')
                .Where(line => line.StartsWith('@')).Select(line => line[1..].TrimEnd('\r')).ToArray();
            Assert.Equal([PublishedTokens.Layout.AgentHost, workspace.Loader], claudeTargets);
            Assert.All(claudeTargets, path => Assert.True(File.Exists(workspace.Combine(path))));
            var entries = workspace.EntryTargets(hostLoader);
            Assert.Contains(workspace.Combine(skills), entries);
            Assert.Contains(workspace.Combine(ForeignPath), entries);
            Assert.All(entries, path => Assert.True(File.Exists(path), path));
            Assert.Contains(workspace.Combine(PublishedTokens.Layout.CliSkillDirectory + "/" + PublishedTokens.Layout.NativeSkillFileName), workspace.EntryTargets(skills));
            Assert.Equal(foreign, workspace.Read(ForeignPath));
        }
    }
}
