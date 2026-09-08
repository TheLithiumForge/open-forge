using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.List;

public sealed class LibraryListCompositionTests
{
    [Theory(DisplayName = "Library List terminal modes bypass workspace and domain observation and remain text only"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData("--help")]
    [InlineData("--version")]
    public static async Task TerminalBypass(string terminal)
    {
        using var fixture = new LibraryReadWorkspace();
        var before = fixture.Snapshot();
        var capture = await CliHostCapture.RunAsync(
            ["library", "list", terminal, "--json", "--workspace", fixture.Files.Combine("does-not-exist")], fixture.Path);

        Assert.Equal(0, capture.ExitCode);
        Assert.Empty(capture.Error);
        Assert.NotEmpty(capture.Output);
        Assert.False(capture.Output.TrimStart().StartsWith('{'));
        if (terminal == "--help")
        {
            Assert.Contains("library list", capture.Output, StringComparison.Ordinal);
            foreach (var flag in new[] { "--workspace", "--json", "--view", "--verbose", "--help", "--version" })
            {
                Assert.Contains(flag, capture.Output, StringComparison.Ordinal);
            }
        }

        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Theory(DisplayName = "Library List composed invalid input stops before reading a malformed record"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData("operand")]
    [InlineData("mutation")]
    [InlineData("view")]
    [InlineData("duplicate-workspace")]
    [InlineData("duplicate-view")]
    [InlineData("help-conflict")]
    public static async Task InvalidInputPerformsNoObservation(string scenario)
    {
        using var fixture = new LibraryReadWorkspace();
        LibraryReadBoundary.Arrange(fixture, "malformed-record");
        string[] suffix = scenario switch
        {
            "operand" => ["team-knowledge"],
            "mutation" => ["--dry-run"],
            "view" => ["--view", "unsupported"],
            "duplicate-workspace" => ["--workspace", fixture.Path, "--workspace", fixture.Path],
            "duplicate-view" => ["--view", "compact", "--view", "expanded"],
            "help-conflict" => ["--help", "--version"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var before = fixture.Snapshot();
        var capture = await CliHostCapture.RunAsync(["library", "list", "--json", .. suffix], fixture.Path);

        Assert.Equal(4, capture.ExitCode);
        Assert.Empty(capture.Output);
        var offendingInput = scenario switch
        {
            "operand" => "team-knowledge",
            "mutation" => "--dry-run",
            "view" => "unsupported",
            "duplicate-workspace" => "--workspace",
            "duplicate-view" => "--view",
            "help-conflict" => "--help",
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        Assert.Contains(offendingInput, capture.Error, StringComparison.Ordinal);
        if (scenario == "help-conflict")
        {
            Assert.Contains("--version", capture.Error, StringComparison.Ordinal);
        }

        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Fact(DisplayName = "Library List uses the exact selected directory and accepts presentation-only global flags"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task ExactWorkspaceAndPresentationFlags()
    {
        using var fixture = new LibraryReadWorkspace();
        LibraryReadBoundary.Arrange(fixture, "malformed-record");
        var child = fixture.Files.CreateDirectory("child");
        var first = await CliHostCapture.RunAsync(["library", "list", "--json"], child);
        var second = await CliHostCapture.RunAsync(
            ["library", "list", "--workspace", "child", "--json", "--json", "--view:compact", "--verbose", "--verbose"], fixture.Path);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(0, second.ExitCode);
        using var current = JsonDocument.Parse(first.Output);
        using var explicitWorkspace = JsonDocument.Parse(second.Output);
        Assert.Equal(child, current.RootElement.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal("current-directory", current.RootElement.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal(child, explicitWorkspace.RootElement.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal("explicit-workspace", explicitWorkspace.RootElement.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal(current.RootElement.GetProperty("result").GetRawText(), explicitWorkspace.RootElement.GetProperty("result").GetRawText());
        fixture.AssertNoPersistentState();
    }
}
