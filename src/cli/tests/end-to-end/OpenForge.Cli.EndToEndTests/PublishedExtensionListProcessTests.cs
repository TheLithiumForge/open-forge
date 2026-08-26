using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedExtensionListProcessTests
{
    [Fact(DisplayName = "Published Extension group and List help expose exact grammar sections examples and streams"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedHelpExposesCompleteContract()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create();
        var group = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension"]);
        var leaf = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--help"]);

        Assert.Equal(0, group.ExitCode);
        Assert.Equal(0, leaf.ExitCode);
        Assert.Contains("list", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("inspect", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("open-forge extension list", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--installed", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--available", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--source", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Results and streams", leaf.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Extension List reports trusted Installed and embedded Available facts without writes"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedDefaultReportsBothSections()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create(trustedInstalled: true);
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("extension list", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var commandResult = document.RootElement.GetProperty("result");
        Assert.Equal("trusted", commandResult.GetProperty("coverage").GetProperty("lifecycleTrust").GetString());
        Assert.Equal("development-toolkit", Assert.Single(commandResult.GetProperty("installed").EnumerateArray()).GetProperty("id").GetString());
        Assert.Equal("development-toolkit", Assert.Single(commandResult.GetProperty("available").EnumerateArray()).GetProperty("id").GetString());
    }

    [Fact(DisplayName = "Published Extension List JSON preserves typed lifecycle facts with opaque framework duplicates"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedJsonIgnoresOpaqueFrameworkDuplicateProperties()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create(
            trustedInstalled: true,
            opaqueFrameworkDuplicates: true);

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--installed", "--json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var commandResult = document.RootElement.GetProperty("result");
        Assert.Equal("trusted", commandResult.GetProperty("coverage").GetProperty("lifecycleTrust").GetString());
        Assert.Equal(
            "development-toolkit",
            Assert.Single(commandResult.GetProperty("installed").EnumerateArray()).GetProperty("id").GetString());
    }

    [Fact(DisplayName = "Published Extension List section filters compose idempotently and JSON ignores view"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFiltersAndJsonViewAreDeterministic()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create(trustedInstalled: true);
        var compact = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--installed", "--installed", "--available", "--json", "--view=compact"]);
        var expanded = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--installed", "--available", "--json", "--view=expanded"]);
        var installed = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--installed", "--json"]);
        var available = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--available", "--json"]);

        Assert.Equal(compact.StandardOutput, expanded.StandardOutput);
        using var installedDocument = JsonDocument.Parse(installed.StandardOutput);
        using var availableDocument = JsonDocument.Parse(available.StandardOutput);
        Assert.Empty(installedDocument.RootElement.GetProperty("result").GetProperty("available").EnumerateArray());
        Assert.Empty(availableDocument.RootElement.GetProperty("result").GetProperty("installed").EnumerateArray());
        Assert.Equal("not-requested", installedDocument.RootElement.GetProperty("result").GetProperty("coverage").GetProperty("available").GetString());
        Assert.Equal("not-requested", availableDocument.RootElement.GetProperty("result").GetProperty("coverage").GetProperty("installed").GetString());
    }

    [Fact(DisplayName = "Published Extension List reads one exact explicit package and preserves source bytes"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedExplicitPackageIsExactAndReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create();
        var beforeSource = working.SnapshotSource();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--available", "--source", working.SourcePath, "--json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var commandResult = document.RootElement.GetProperty("result");
        Assert.Equal("package", commandResult.GetProperty("source").GetProperty("kind").GetString());
        Assert.Equal("local-toolkit", Assert.Single(commandResult.GetProperty("available").EnumerateArray()).GetProperty("id").GetString());
        Assert.Equal(beforeSource, working.SnapshotSource());
    }

    [Theory(DisplayName = "Published Extension List retains exact unavailable invalid and blocked source statuses"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    [InlineData("missing-available", 3, "incomplete", "extension-list.source-unavailable")]
    [InlineData("missing-installed", 2, "attention", "extension-list.source-unavailable")]
    [InlineData("malformed", 4, "invalid", "extension-list.source-invalid")]
    [InlineData("overlap", 5, "blocked", "extension-list.source-blocked")]
    public async Task PublishedSourceConditionsRetainTypedResults(
        string scenario,
        int expectedExit,
        string expectedStatus,
        string expectedFinding)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create();
        var malformed = scenario == "malformed" ? working.CreateMalformedSource() : working.SourcePath;
        var missing = working.MissingSourcePath;
        string[] arguments = scenario switch
        {
            "missing-available" => ["extension", "list", "--available", "--source", missing, "--json"],
            "missing-installed" => ["extension", "list", "--installed", "--source", missing, "--json"],
            "malformed" => ["extension", "list", "--available", "--source", malformed, "--json"],
            "overlap" => ["extension", "list", "--available", "--source", working.Path, "--json"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Extension List source scenario is not defined."),
        };
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            arguments);

        Assert.Equal(expectedExit, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == expectedFinding);
    }

    [Theory(DisplayName = "Published Extension List human statuses use the contracted stdout and stderr streams"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    [InlineData("complete", 0, true, "complete")]
    [InlineData("attention", 2, true, "requires attention")]
    [InlineData("incomplete", 3, true, "incomplete")]
    [InlineData("invalid", 4, false, "invalid")]
    [InlineData("blocked", 5, false, "blocked")]
    public async Task PublishedHumanStatusesPreserveStreamAndExitPolicy(
        string scenario,
        int expectedExitCode,
        bool usesStandardOutput,
        string expectedHumanStatus)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create();
        var malformed = scenario == "invalid" ? working.CreateMalformedSource() : working.SourcePath;
        string[] arguments = scenario switch
        {
            "complete" => ["extension", "list", "--available"],
            "attention" => ["extension", "list", "--installed", "--source", working.MissingSourcePath],
            "incomplete" => ["extension", "list", "--available", "--source", working.MissingSourcePath],
            "invalid" => ["extension", "list", "--available", "--source", malformed],
            "blocked" => ["extension", "list", "--available", "--source", working.Path],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The human status scenario is not defined."),
        };
        var beforeSource = working.SnapshotSource();

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            arguments);

        var primary = usesStandardOutput ? result.StandardOutput : result.StandardError;
        var secondary = usesStandardOutput ? result.StandardError : result.StandardOutput;
        Assert.Equal(expectedExitCode, result.ExitCode);
        Assert.Contains($"Status: {expectedHumanStatus}", primary, StringComparison.Ordinal);
        Assert.Equal(string.Empty, secondary);
        Assert.Equal(beforeSource, working.SnapshotSource());
    }

    [Fact(DisplayName = "Published Extension List rejects repeated source syntax on stderr"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRepeatedSourceIsAParserFailure()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--source", working.SourcePath, "--source", working.SourcePath, "--json"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("--source", result.StandardError, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Extension List verbose JSON preserves primary bytes and bounded stderr diagnostics"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedVerboseStreamsRemainSeparate()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create();
        var plain = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--json"]);
        var verbose = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--json", "--verbose"]);

        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Equal(plain.StandardOutput, verbose.StandardOutput);
        Assert.Equal(string.Empty, plain.StandardError);
        Assert.InRange(verbose.StandardError.Length, 1, 4096);
        Assert.DoesNotContain('\n', verbose.StandardError.TrimEnd('\r', '\n'));
        Assert.DoesNotContain('\r', verbose.StandardError.TrimEnd('\r', '\n'));
    }
}
