using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedDoctorProcessTests
{
    [Theory, InlineData(".agents"), InlineData("knowledge/main")]
    [Trait("Feature", "doctor-outside"), Trait("Evidence", "EndToEnd")]
    public async Task MissingRegisteredDirectoryUsesTheExistingPublishedFindingGraph(string root)
    {
        using var files = TemporaryWorkspace.Create("published-doctor-outside");
        using var store = PublishedWorkspaceLockStore.Create("published-doctor-outside-store");
        store.Track(files.Path);
        files.WriteText(PublishedTokens.Layout.AgentHost, "# Independent workspace\n");
        files.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{root}}","{{PublishedTokens.Settings.OutsideRoutesProperty}}":["docs/team"]}
            """);
        files.WriteText(root + "/" + PublishedTokens.Layout.LoaderFileName, GeneratedLoaderDocumentBuilder.Build(string.Empty));
        var run = await PublishedProcessTestSupport.RunWithoutWritesAsync(PublishedExecutableTarget.Discover(), files.Path,
            files.SnapshotHashes, [PublishedTokens.Doctor.Command, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat,
                PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.FullDetail], store.EnvironmentVariables);
        Assert.Equal(2, run.ExitCode);
        Assert.Empty(run.StandardError);
        using var json = JsonDocument.Parse(run.StandardOutput);
        Assert.Single(json.RootElement.GetProperty(PublishedTokens.Json.Findings).EnumerateArray(), finding =>
            finding.GetProperty(PublishedTokens.Json.Code).GetString() == PublishedTokens.Doctor.RootMissingCode
            && finding.GetProperty(PublishedTokens.Json.Subject).GetProperty(PublishedTokens.Json.Path).GetString() == "docs/team");
        Assert.False(json.RootElement.GetProperty(PublishedTokens.Json.Data).TryGetProperty(PublishedTokens.StatusOutsideRoutes.Collection, out _));
    }
}
