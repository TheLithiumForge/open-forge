using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteListProcessTests
{
    [Theory, InlineData(PublishedTokens.Sources.AgentsRoot, true), InlineData("knowledge/main", false)]
    [Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task RegisteredOutsideSelectionRetainsAuthoredIdentityWithoutWriting(string mainRoot, bool listed)
    {
        using var workspace = TemporaryWorkspace.Create(nameof(RegisteredOutsideSelectionRetainsAuthoredIdentityWithoutWriting));
        const string directory = "docs/team";
        var anchor = directory + "/" + PublishedTokens.Layout.IndexFile;
        var leaf = directory + "/" + PublishedTokens.Layout.NativeSkillFileName;
        var loader = mainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
        workspace.WriteText(PublishedTokens.Settings.RelativePath,
            $$"""{"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{mainRoot}}","{{PublishedTokens.Settings.OutsideRoutesProperty}}":["{{directory}}"]}""");
        var relative = Path.GetRelativePath(mainRoot, anchor).Replace('\\', '/');
        workspace.WriteText(loader, $"# Loader\n\n## {PublishedTokens.Metadata.Entries}\n\n"
            + (listed ? $"- [Team row]({relative}) - #{PublishedTokens.Metadata.LoadNow}\n" : PublishedTokens.Metadata.EmptyEntry + "\n"));
        workspace.WriteText(anchor, OpenForgeDocumentSeed.Metadata("Authored team", ["Row"],
            $"# Authored outside catalogue\n\n## {PublishedTokens.Metadata.Entries}\n\n"
            + $"- [Outside ordinary skill]({PublishedTokens.Layout.NativeSkillFileName}) - #{PublishedTokens.Metadata.LoadNow}\n"));
        workspace.WriteText(leaf, "# Independent ordinary Markdown\r\n\r\nKeep π bytes.\r\n");
        var target = PublishedExecutableTarget.Discover();
        var run = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
            [PublishedTokens.Route.Command, PublishedTokens.RouteList.Command, PublishedTokens.Layout.LiteralReferencePrefix + anchor,
                PublishedTokens.RouteList.Depth, PublishedTokens.RouteList.AllDepth,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);
        Assert.Equal(listed ? PublishedTokens.Statuses.CompleteExitCode : 2, run.ExitCode);
        Assert.Empty(run.StandardError);
        using var json = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal(listed ? PublishedTokens.Statuses.Complete : PublishedTokens.Statuses.Attention,
            json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        var rows = json.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.RouteListData.Rows).EnumerateArray().ToArray();
        Assert.Equal(new[] { anchor, leaf }.Order(StringComparer.Ordinal), rows.Select(row => row.GetProperty(PublishedTokens.Json.Path).GetString()).Order(StringComparer.Ordinal));
        Assert.All(rows, row =>
        {
            Assert.Equal(directory, row.GetProperty(PublishedTokens.RouteListData.MaintenanceRoot).GetString());
            Assert.Equal(PublishedTokens.Layout.LiteralReferencePrefix + row.GetProperty(PublishedTokens.Json.Path).GetString(),
                row.GetProperty(PublishedTokens.Json.Id).GetString());
        });
    }
}
