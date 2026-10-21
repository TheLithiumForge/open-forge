using System.Text;
using System.Text.Json;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Help;

public sealed class HelpIntegrationTests
{
    [Theory, InlineData("", "topics", 0), InlineData("concepts", "concepts", 0), InlineData("unknown", "unknown", 4), InlineData("install", "command", 4)]
    [Trait("Feature", "help"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task TextAndJsonSnapshotsKeepTheSameResultAtEveryDetail(string topic, string situation, int exit)
    {
        using var workspace = TemporaryWorkspace.Create("help");
        var before = workspace.SnapshotHashes();
        var captures = new Dictionary<string, string>();
        foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
        {
            foreach (var format in new[] { "text", "json" })
            {
                var arguments = new List<string> { "help" };
                if (topic.Length > 0) arguments.Add(topic);
                arguments.AddRange(["--detail", detail, "--format", format, "--workspace", workspace.Combine("missing")]);
                var (actualExit, stdout, stderr) = await RunAsync(workspace, arguments);
                Assert.Equal(exit, actualExit);
                if (format == "json" || exit == 0) Assert.Empty(stderr);
                else Assert.Empty(stdout);
                var primary = format == "text" && exit != 0 ? stderr : stdout;
                var key = format == "json" ? $"{situation}.json.{detail}" : $"{situation}.{detail}";
                captures.Add(key, primary);
                if (format == "json")
                {
                    using var json = JsonDocument.Parse(primary);
                    Assert.Equal(exit == 0 ? "completed" : "invalid-input", json.RootElement.GetProperty("status").GetString());
                    Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("workspace").ValueKind);
                    Assert.Empty(json.RootElement.GetProperty("effects").EnumerateArray());
                    if (topic == "install")
                        Assert.Equal("open-forge install --help", json.RootElement.GetProperty("next").GetProperty("command").GetString());
                    else if (exit != 0)
                        Assert.Equal("open-forge help --help", json.RootElement.GetProperty("next").GetProperty("command").GetString());
                }
            }
        }
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Combine("missing")));
        CommandOutputSnapshot.MatchDetailSnapshot(captures, testName: situation);
    }

    [Fact, Trait("Feature", "help"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task ConceptsPreservesEmbeddedConceptsAndTheGuideFileFormatsWithoutAWorkspace()
    {
        using var workspace = TemporaryWorkspace.Create("help-parity");
        var (exit, stdout, stderr) = await RunAsync(workspace, ["help", "concepts"]);
        Assert.Equal(0, exit);
        Assert.Empty(stderr);
        Assert.Empty(workspace.SnapshotHashes());
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .Single(candidate => candidate.GetName().Name == "OpenForge.Cli.OutputText");
        using var stream = assembly.GetManifestResourceStream("OpenForge.Cli.OutputText.Help/concepts.md");
        Assert.NotNull(stream);
        using var conceptsBytes = new MemoryStream();
        await stream.CopyToAsync(conceptsBytes, TestContext.Current.CancellationToken);
        var printed = Encoding.UTF8.GetBytes(stdout);
        var concepts = conceptsBytes.ToArray();
        Assert.Equal(concepts, printed[..concepts.Length]);

        var payload = EmbeddedFrameworkPayloadReader.Read();
        Assert.NotNull(payload.Payload);
        var asset = payload.Payload.Find(".agents/README.md");
        Assert.NotNull(asset);
        var guide = Encoding.UTF8.GetString(asset.Bytes.AsSpan());
        var section = guide[(guide.IndexOf("\n## Writing workspace files\n", StringComparison.Ordinal) + 1)..];
        Assert.EndsWith(section, stdout, StringComparison.Ordinal);
        Assert.Contains("\n### Lines under Entries\n", stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("## Use a document you already have", stdout, StringComparison.Ordinal);
    }

    [Fact, Trait("Feature", "help"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task LocalGuideAndMalformedWorkspaceFilesDoNotChangeBuiltInConcepts()
    {
        using var workspace = TemporaryWorkspace.Create("help-local");
        var original = await RunAsync(workspace, ["help", "concepts"]);
        workspace.WriteText(".agents/README.md", "# Local guide\n\n## Writing workspace files\n\nLocal rules.\n");
        workspace.WriteText(".agents/open-forge.json", "invalid JSON");
        workspace.WriteText(".agents/loader.md", "Invalid workspace loader");
        var before = workspace.SnapshotHashes();

        var actual = await RunAsync(workspace, ["help", "concepts"]);

        Assert.Equal(original, actual);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<(int Exit, string Stdout, string Stderr)> RunAsync(TemporaryWorkspace workspace, IReadOnlyList<string> arguments)
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var application = CliCompositionRoot.Create(new CliProcessIdentity("open-forge", "test"));
        var completion = await application.RunAsync([.. arguments], new CliProcessEnvironment(workspace.Path),
            new CliOutputWriters(stdout, stderr), TestContext.Current.CancellationToken);
        return (completion.ExitCode, stdout.ToString(), stderr.ToString());
    }
}
