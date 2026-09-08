using System.Text.Json;
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
}
