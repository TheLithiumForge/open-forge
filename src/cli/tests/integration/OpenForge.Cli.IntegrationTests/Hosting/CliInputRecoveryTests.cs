using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Hosting;

public sealed class CliInputRecoveryTests
{
    [Theory(DisplayName = "Invalid input names its subject and next command without writing files"), Trait("Boundary", "Host"), Trait("Feature", "cli-input"), Trait("Evidence", "Integration")]
    [InlineData("unknown-command")]
    [InlineData("unknown-option")]
    [InlineData("missing-for")]
    [InlineData("missing-workspace")]
    [InlineData("route-id")]
    [InlineData("route-action")]
    [InlineData("context-source")]
    [InlineData("inspect-source")]
    [InlineData("depth")]
    [InlineData("frontmatter")]
    [InlineData("preset")]
    [InlineData("format")]
    [InlineData("detail")]
    [InlineData("control-format")]
    [InlineData("control-frontmatter")]
    [InlineData("direction")]
    [InlineData("require")]
    [InlineData("follow-links")]
    public async Task InvalidInputHasRecovery(string scenario)
    {
        using var workspace = TemporaryWorkspace.Create("input-recovery");
        workspace.CreateFile("AGENTS.md", "# Startup\n");
        workspace.CreateFile(".agents/loader.md", "# Loader\n\n## Entries\n");
        var before = workspace.SnapshotHashes();
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = scenario,
            Arguments = Arguments(scenario, workspace.Path),
            ExitCode = 4,
            ShellDiagnostic = scenario is "unknown-command" or "unknown-option" or "missing-workspace" or "format" or "detail" or "control-format",
        }, snapshotCollector: snapshots);
        Assert.Equal(before, workspace.SnapshotHashes());
        var invalidOption = scenario switch
        {
            "depth" => "--depth",
            "frontmatter" or "control-frontmatter" => "--frontmatter",
            "preset" => "--preset",
            "direction" => "--direction",
            "require" => "--require",
            "follow-links" => "--follow-links",
            _ => null,
        };
        if (invalidOption is not null)
        {
            foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
            {
                var stdout = new StringWriter();
                var stderr = new StringWriter();
                Assert.Equal(4, await CliHost.RunAsync([.. Arguments(scenario, workspace.Path), "--detail", detail], workspace.Path,
                    new CliOutputWriters(stdout, stderr), TestContext.Current.CancellationToken));
                Assert.Empty(stdout.ToString());
                Assert.Equal(1, stderr.ToString().Split($"Invalid {invalidOption} value:", StringSplitOptions.None).Length - 1);
                Assert.Contains("--help", stderr.ToString(), StringComparison.Ordinal);
            }
        }
        if (scenario is "control-format" or "control-frontmatter")
        {
            var stdout = new StringWriter();
            var stderr = new StringWriter();
            Assert.Equal(4, await CliHost.RunAsync(Arguments(scenario, workspace.Path), workspace.Path,
                new CliOutputWriters(stdout, stderr), TestContext.Current.CancellationToken));
            Assert.Empty(stdout.ToString());
            Assert.DoesNotContain('\u001b', stderr.ToString());
            Assert.DoesNotContain('\t', stderr.ToString());
            Assert.Contains(@"\n", stderr.ToString(), StringComparison.Ordinal);
        }
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: $"{nameof(InvalidInputHasRecovery)}_{scenario}");
    }

    [Fact(DisplayName = "Absent startup context has an install preview while JSON retains all limitations"), Trait("Boundary", "Host"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task MissingStartupKeepsStreamsAndJson()
    {
        using var workspace = TemporaryWorkspace.Create("context-startup-missing");
        var before = workspace.SnapshotHashes();
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var args = new[] { "context", "--workspace", workspace.Path };
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        Assert.Equal(3, await CliHost.RunAsync(args, workspace.Path, new CliOutputWriters(stdout, stderr), TestContext.Current.CancellationToken));
        Assert.DoesNotContain("=== AGENTS.md ===", stdout.ToString(), StringComparison.Ordinal);
        stdout.GetStringBuilder().Clear();
        stderr.GetStringBuilder().Clear();
        Assert.Equal(3, await CliHost.RunAsync([.. args, "--format", "json"], workspace.Path, new CliOutputWriters(stdout, stderr), TestContext.Current.CancellationToken));
        using var json = JsonDocument.Parse(stdout.ToString());
        Assert.Equal("incomplete", json.RootElement.GetProperty("status").GetString());
        Assert.True(json.RootElement.GetProperty("limitations").GetArrayLength() >= 3);
        Assert.True(json.RootElement.GetProperty("findings").GetArrayLength() >= 2);
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("counts").GetProperty("sources").ValueKind);
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "startup",
            Arguments = args,
            ExitCode = 3,
        }, snapshotCollector: snapshots);
        Assert.Equal(before, workspace.SnapshotHashes());
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    private static string[] Arguments(string scenario, string workspace) => scenario switch
    {
        "unknown-command" => ["statsu"],
        "unknown-option" => ["status", "--detial", "standard"],
        "missing-for" => ["context", "--workspace", workspace, "--for"],
        "missing-workspace" => ["context", "--workspace"],
        "route-id" => ["install", "--workspace", workspace, "--preset", "custom", "--route", "memroy=add", "--dry-run"],
        "route-action" => ["install", "--workspace", workspace, "--preset", "custom", "--route", "guidance=banana", "--dry-run"],
        "context-source" => ["context", "--workspace", workspace, "memroy"],
        "inspect-source" => ["route", "inspect", "--workspace", workspace, "memroy"],
        "depth" => ["route", "list", "--workspace", workspace, "--depth=-1"],
        "frontmatter" => ["install", "--workspace", workspace, "--frontmatter", "other"],
        "preset" => ["install", "--workspace", workspace, "--preset", "other"],
        "format" => ["status", "--format", "xml"],
        "detail" => ["status", "--detail", "other"],
        "control-format" => ["status", "--format", "xml\n\t\u001b[2J"],
        "control-frontmatter" => ["install", "--workspace", workspace, "--frontmatter", "other\n\u001b[2J"],
        "direction" => ["references", "--workspace", workspace, "--direction", "other", "memory"],
        "require" => ["find", "--workspace", workspace, "--tag", "Core", "--require", "other"],
        "follow-links" => ["context", "--workspace", workspace, "--follow-links", "-1"],
        _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
    };
}
