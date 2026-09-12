using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Inspect;

public sealed class LibraryInspectCompositionTests
{
    [Theory(DisplayName = "Library Inspect terminal modes require no ID or workspace and remain text only"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData("--help")]
    [InlineData("--version")]
    public static async Task TerminalBypass(string terminal)
    {
        using var fixture = new LibraryReadWorkspace();
        var before = fixture.Snapshot();
        var capture = await CliHostCapture.RunAsync(
            ["library", "inspect", terminal, "--json", "--workspace", fixture.Files.Combine("does-not-exist")], fixture.Path);

        Assert.Equal(0, capture.ExitCode);
        Assert.Empty(capture.Error);
        Assert.NotEmpty(capture.Output);
        Assert.False(capture.Output.TrimStart().StartsWith('{'));
        if (terminal == "--help")
        {
            Assert.Contains("library inspect", capture.Output, StringComparison.Ordinal);
            Assert.Contains("library-id", capture.Output, StringComparison.Ordinal);
            foreach (var flag in new[] { "--workspace", "--json", "--view", "--verbose", "--help", "--version" })
            {
                Assert.Contains(flag, capture.Output, StringComparison.Ordinal);
            }
        }

        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Theory(DisplayName = "Library Inspect composed invalid subjects stop before record inventory and operation work"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData("missing")]
    [InlineData("multiple")]
    [InlineData("malformed")]
    [InlineData("source-path")]
    [InlineData("mutation")]
    public static async Task InvalidInputPerformsNoObservation(string scenario)
    {
        using var fixture = new LibraryReadWorkspace();
        LibraryReadBoundary.Arrange(fixture, "malformed-record");
        string[] suffix = scenario switch
        {
            "missing" => [],
            "multiple" => ["team-knowledge", "other"],
            "malformed" => ["Team-Knowledge"],
            "source-path" => [".agents/directives/review.md"],
            "mutation" => ["team-knowledge", "--dry-run"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var before = fixture.Snapshot();
        var capture = await CliHostCapture.RunAsync(["library", "inspect", "--json", .. suffix], fixture.Path);

        Assert.Equal(4, capture.ExitCode);
        if (scenario is "multiple" or "mutation")
        {
            Assert.Empty(capture.Output);
            Assert.Contains(scenario == "multiple" ? "other" : "--dry-run", capture.Error, StringComparison.Ordinal);
        }
        else
        {
            using var document = JsonDocument.Parse(capture.Output);
            var payload = document.RootElement.GetProperty("result");
            Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
            Assert.Equal("not-started", payload.GetProperty("record").GetProperty("state").GetString());
            Assert.Equal("not-started", payload.GetProperty("source").GetProperty("state").GetString());
            Assert.Empty(payload.GetProperty("source").GetProperty("eligiblePaths").EnumerateArray());
            Assert.Empty(payload.GetProperty("projection").GetProperty("comparisons").EnumerateArray());
        }

        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Theory, Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData(true, true), InlineData(true, false), InlineData(false, true), InlineData(false, false)]
    public static async Task WorkspaceFailureRetainsParsedLibrarySubject(bool globalsFirst, bool ordinaryFile)
    {
        using var workspace = new LibraryMutationWorkspace();
        if (ordinaryFile)
        {
            workspace.Write("bad-workspace", "An ordinary file cannot be a workspace.\n");
        }

        var badPath = workspace.Absolute("bad-workspace");
        var before = workspace.Snapshot();
        string[] arguments = globalsFirst
            ? ["--workspace", badPath, "--json", "library", "inspect", "team-knowledge"]
            : ["library", "inspect", "team-knowledge", "--workspace", badPath, "--json"];

        var capture = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(ordinaryFile ? 5 : 3, capture.ExitCode);
        Assert.Empty(capture.Error);
        using var document = JsonDocument.Parse(capture.Output);
        var envelope = document.RootElement;
        var payload = envelope.GetProperty("result");
        Assert.Equal("library inspect", envelope.GetProperty("command").GetString());
        Assert.Equal(ordinaryFile ? "blocked" : "incomplete", envelope.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, envelope.GetProperty("workspace").ValueKind);
        var finding = Assert.Single(payload.GetProperty("findings").EnumerateArray());
        Assert.Equal(ordinaryFile ? "library-inspect.record-blocked" : "library-inspect.record-unavailable", finding.GetProperty("code").GetString());
        Assert.Equal(ordinaryFile ? "The selected workspace root is not a directory." : "The selected workspace is missing.", finding.GetProperty("cause").GetString());
        Assert.Equal(ordinaryFile ? "blocked" : "unavailable", payload.GetProperty("record").GetProperty("state").GetString());
        Assert.Equal("not-started", payload.GetProperty("source").GetProperty("state").GetString());
        Assert.Empty(payload.GetProperty("source").GetProperty("eligiblePaths").EnumerateArray());
        Assert.Empty(payload.GetProperty("projection").GetProperty("comparisons").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal("team-knowledge", payload.GetProperty("record").GetProperty("id").GetString());
        Assert.Equal("team-knowledge", finding.GetProperty("libraryId").GetString());
    }
}
