using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedHelpProcessTests
{
    [Theory, InlineData("", "topics", 0), InlineData("concepts", "concepts", 0), InlineData("unknown", "unknown", 4), InlineData("install", "command", 4)]
    [Trait("Feature", "help"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedHelpWorksOutsideAWorkspaceInTextAndJson(string topic, string situation, int exit)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = TemporaryWorkspace.Create("published-help");
        using var dataHome = TestDataHome.Create("published-help");
        var captures = new Dictionary<string, string>();
        foreach (var format in new[] { "text", "json" })
        {
            var arguments = new List<string> { "help" };
            if (topic.Length > 0) arguments.Add(topic);
            arguments.AddRange(["--format", format]);
            var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
                target, workspace.Path, workspace.SnapshotHashes, arguments, dataHome.EnvironmentVariables());
            Assert.Equal(exit, result.ExitCode);
            if (format == "json" || exit == 0) Assert.Empty(result.StandardError);
            else Assert.Empty(result.StandardOutput);
            var primary = format == "text" && exit != 0 ? result.StandardError : result.StandardOutput;
            captures.Add(format == "json" ? $"{situation}.json.minimal" : $"{situation}.minimal", primary);
            if (format == "json")
            {
                using var json = JsonDocument.Parse(primary);
                Assert.Equal("help", json.RootElement.GetProperty("command").GetString());
                Assert.Equal(exit == 0 ? "completed" : "invalid-input", json.RootElement.GetProperty("status").GetString());
                Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("workspace").ValueKind);
            }
        }
        Assert.Empty(workspace.SnapshotHashes());
        Assert.False(Directory.Exists(dataHome.Path));
        CommandOutputSnapshot.MatchDetailSnapshot(captures, testName: situation);
    }
}
