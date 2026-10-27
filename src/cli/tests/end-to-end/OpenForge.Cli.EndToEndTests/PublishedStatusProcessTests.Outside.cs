using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedStatusProcessTests
{
    [Theory, InlineData(".agents"), InlineData("knowledge/main")]
    [Trait("Feature", "status-outside"), Trait("Evidence", "EndToEnd")]
    public async Task RegisteredUnlistedDirectoryHasIndependentPublishedFacts(string root)
    {
        using var files = TemporaryWorkspace.Create("published-status-outside");
        using var store = PublishedWorkspaceLockStore.Create("published-status-outside-store");
        store.Track(files.Path);
        files.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{root}}","{{PublishedTokens.Settings.OutsideRoutesProperty}}":["docs/team"]}
            """);
        files.WriteText(root + "/" + PublishedTokens.Layout.LoaderFileName,
            GeneratedLoaderDocumentBuilder.Build(string.Empty));
        files.WriteText("docs/team/_team.md", OpenForgeDocumentSeed.Metadata("Independent outside", ["Body"], "# Team\n\nUntouched bytes.\n"));
        var run = await PublishedProcessTestSupport.RunWithoutWritesAsync(PublishedExecutableTarget.Discover(), files.Path,
            files.SnapshotHashes, [PublishedTokens.Status.Command, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], store.EnvironmentVariables);
        Assert.Empty(run.StandardError);
        using var json = JsonDocument.Parse(run.StandardOutput);
        var outside = Assert.Single(json.RootElement.GetProperty(PublishedTokens.Json.Data)
            .GetProperty(PublishedTokens.StatusOutsideRoutes.Collection).EnumerateArray());
        Assert.Equal("docs/team", outside.GetProperty(PublishedTokens.Json.Path).GetString());
        Assert.Equal(PublishedTokens.StatusOutsideRoutes.Available, outside.GetProperty(PublishedTokens.StatusOutsideRoutes.State).GetString());
        Assert.False(outside.GetProperty(PublishedTokens.StatusOutsideRoutes.Listed).GetBoolean());
    }
}
