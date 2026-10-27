using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteInspectProcessTests
{
    [Theory, InlineData(PublishedTokens.Sources.AgentsRoot, true), InlineData("knowledge/main", false)]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task RegisteredOutsideOverwriteSelectionUsesTheLiteralBaseWithoutWriting(string mainRoot, bool listed)
    {
        using var workspace = TemporaryWorkspace.Create(nameof(RegisteredOutsideOverwriteSelectionUsesTheLiteralBaseWithoutWriting));
        const string directory = "docs/team";
        var anchor = directory + "/" + PublishedTokens.Layout.IndexFile;
        var leaf = directory + "/ordinary" + PublishedTokens.Layout.MarkdownExtension;
        var overwrite = directory + "/ordinary" + PublishedTokens.Layout.OverwriteSuffix;
        var loader = mainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
        workspace.WriteText(PublishedTokens.Settings.RelativePath,
            $$"""{"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{mainRoot}}","{{PublishedTokens.Settings.OutsideRoutesProperty}}":["{{directory}}"]}""");
        var relative = Path.GetRelativePath(mainRoot, anchor).Replace('\\', '/');
        workspace.WriteText(loader, $"# Loader\n\n## {PublishedTokens.Metadata.Entries}\n\n"
            + (listed ? $"- [Team row]({relative}) - #{PublishedTokens.Metadata.LoadNow}\n" : PublishedTokens.Metadata.EmptyEntry + "\n"));
        workspace.WriteText(anchor, OpenForgeDocumentSeed.Metadata("Authored team", ["Row"],
            $"# Authored outside catalogue\n\n## {PublishedTokens.Metadata.Entries}\n\n"
            + $"- [Outside ordinary file](ordinary{PublishedTokens.Layout.MarkdownExtension}) - #{PublishedTokens.Metadata.LoadNow}\n"));
        workspace.WriteText(leaf, "# Independent ordinary Markdown\r\n\r\nKeep π bytes.\r\n");
        workspace.WriteText(overwrite, "Independent overwrite bytes.\n");
        var target = PublishedExecutableTarget.Discover();
        var run = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
            [PublishedTokens.Route.Command, PublishedTokens.RouteInspect.Command, PublishedTokens.Layout.LiteralReferencePrefix + overwrite,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);
        Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, run.ExitCode);
        Assert.Empty(run.StandardError);
        using var json = JsonDocument.Parse(run.StandardOutput);
        var data = json.RootElement.GetProperty(PublishedTokens.Json.Data);
        Assert.Equal(directory, data.GetProperty(PublishedTokens.RouteInspectData.MaintenanceRoot).GetString());
        Assert.Equal(PublishedTokens.Layout.LiteralReferencePrefix + leaf, data.GetProperty(PublishedTokens.Json.Id).GetString());
        Assert.Equal(leaf, data.GetProperty(PublishedTokens.Json.Path).GetString());
    }
}
