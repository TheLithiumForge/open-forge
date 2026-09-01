using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedReferencesProcessTests
{
    [Fact(DisplayName = "Published References help exposes direct grammar, defaults, examples, related commands, and result streams"), Trait("Feature", "references"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedReferencesHelpExposesCompleteContract()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedReferencesWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["references", "--help"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("open-forge references", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("<source-reference>", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--direction", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--include", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--exclude", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Examples", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Related commands", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Results and streams", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published root help registers References exactly once as a direct leaf"), Trait("Feature", "references"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRootHelpRegistersReferencesOnce()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedReferencesWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["--help"]);

        Assert.Equal(0, result.ExitCode);
        var lines = result.StandardOutput
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.TrimStart().StartsWith("references", StringComparison.Ordinal))
            .ToArray();
        Assert.Single(lines);
    }

    [Fact(DisplayName = "Published References default invocation reports both directions in expanded view with unchanged bytes"), Trait("Feature", "references"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedReferencesDefaultIsBothExpandedAndReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedReferencesWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["references", "docs"]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Workspace:", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Direction: both", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Incoming", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Outgoing", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Level 1", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("target.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("unicodé.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("space file.md", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published References outgoing journey preserves external, missing, fragment, duplicate, Unicode, and space facts on stdout"), Trait("Feature", "references"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedReferencesOutgoingPreservesDirectFacts()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedReferencesWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["references", "docs", "--direction=out", "--json"]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("attention", document.RootElement.GetProperty("status").GetString());
        var outgoing = document.RootElement.GetProperty("result").GetProperty("outgoing");
        Assert.Equal(8, outgoing.GetProperty("occurrenceCount").GetInt32());
        var occurrences = outgoing.GetProperty("occurrences").EnumerateArray().ToArray();
        Assert.Equal(8, occurrences.Length);
        Assert.Equal(
            ["target.md#Overview", "target.md#Overview", "space file.md", "unicodé.md#Café", "missing.md#Absent", "#Overview", "https://example.invalid/reference", "target.md#Overview"],
            occurrences.Select(value => value.GetProperty("rawDestination").GetString()));
        Assert.Equal("external-unchecked", occurrences[6].GetProperty("target").GetProperty("resolution").GetString());
        Assert.Equal("network-not-attempted", occurrences[6].GetProperty("target").GetProperty("network").GetString());
        Assert.Equal("missing", occurrences[4].GetProperty("target").GetProperty("resolution").GetString());
        Assert.Equal("fragment-missing", occurrences[5].GetProperty("target").GetProperty("resolution").GetString());
        Assert.Equal(["base", "base", "base", "base", "base", "base", "base", "overwrite"], occurrences.Select(value => value.GetProperty("source").GetProperty("layer").GetString()));
    }

    [Fact(DisplayName = "Published References incoming filters preserve supplied order and filtered provenance"), Trait("Feature", "references"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedReferencesIncomingFiltersPreserveSelectorEvidence()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedReferencesWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["references", "docs", "--direction=in", "--include", "alpha", "--exclude=beta", "--json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var selection = document.RootElement.GetProperty("result").GetProperty("incomingSelection");
        Assert.Equal("filtered", selection.GetProperty("mode").GetString());
        Assert.Equal(
            [("include", "alpha"), ("exclude", "beta")],
            selection.GetProperty("supplied").EnumerateArray().Select(value =>
                (value.GetProperty("role").GetString()!, value.GetProperty("value").GetString()!)));
        var occurrence = Assert.Single(
            document.RootElement.GetProperty("result").GetProperty("incoming").GetProperty("occurrences").EnumerateArray());
        Assert.Equal("filtered-incoming-scan", occurrence.GetProperty("provenance").GetString());
        Assert.Equal("alpha", occurrence.GetProperty("source").GetProperty("id").GetString());
    }

    [Fact(DisplayName = "Published References JSON ignores view and preserves one complete typed graph"), Trait("Feature", "references"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedReferencesJsonViewIsNoOp()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedReferencesWorkspace.CreateBare();
        var compact = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["references", "docs", "--json", "--view=compact"]);
        var expanded = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["references", "docs", "--json", "--view=expanded"]);

        Assert.Equal(compact.ExitCode, expanded.ExitCode);
        Assert.Equal(string.Empty, compact.StandardError);
        Assert.Equal(compact.StandardOutput, expanded.StandardOutput);
        using var document = JsonDocument.Parse(compact.StandardOutput);
        Assert.Equal("references", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("both", document.RootElement.GetProperty("result").GetProperty("requestedDirection").GetString());
        var incoming = document.RootElement.GetProperty("result").GetProperty("incoming");
        Assert.NotEqual(JsonValueKind.Null, incoming.ValueKind);
        Assert.NotEqual(JsonValueKind.Null, document.RootElement.GetProperty("result").GetProperty("outgoing").ValueKind);
        Assert.Single(
            incoming.GetProperty("occurrences").EnumerateArray(),
            occurrence => occurrence.GetProperty("source").GetProperty("path").GetString()
                == ".agents/reference-route/_references.md");
    }

    [Theory(DisplayName = "Published References semantic invalid cases retain typed status, exit, stream, and bounded finding"), Trait("Feature", "references"), Trait("Evidence", "EndToEnd")]
    [InlineData("missing-source", 4, "invalid", "references.invalid-source")]
    [InlineData("invalid-direction", 4, "invalid", "references.invalid-direction")]
    [InlineData("repeated-direction", 4, "invalid", "references.invalid-direction")]
    [InlineData("filter-with-out", 4, "invalid", "references.invalid-filter")]
    public async Task PublishedReferencesSemanticInvalidCasesRemainTyped(
        string scenario,
        int expectedExit,
        string expectedStatus,
        string expectedFinding)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedReferencesWorkspace.CreateBare();
        string[] arguments = scenario switch
        {
            "missing-source" => ["references", "--json"],
            "invalid-direction" => ["references", "docs", "--json", "--direction=incoming"],
            "repeated-direction" => ["references", "docs", "--json", "--direction=in", "--direction=out"],
            "filter-with-out" => ["references", "docs", "--json", "--direction=out", "--include=alpha"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The References invalid process case is not defined."),
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

    [Fact(DisplayName = "Published References blocked workspace keeps JSON primary output, null workspace, and unchanged bytes"), Trait("Feature", "references"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedReferencesBlockedWorkspaceUsesTypedBlockedResult()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedReferencesWorkspace.CreateBare();
        var missing = working.Combine("missing-workspace");
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["references", "docs", "--workspace", missing, "--json"]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("workspace").ValueKind);
        Assert.Contains(
            document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "references.workspace-unavailable");
        Assert.False(Directory.Exists(missing));
    }

    [Fact(DisplayName = "Published References verbose mode preserves primary bytes and keeps diagnostics bounded on stderr"), Trait("Feature", "references"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedReferencesVerboseStreamsRemainSeparate()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedReferencesWorkspace.CreateBare();
        var plain = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["references", "docs", "--direction=out", "--json"]);
        var verbose = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["references", "docs", "--direction=out", "--json", "--verbose"]);

        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Equal(plain.StandardOutput, verbose.StandardOutput);
        Assert.Equal(string.Empty, plain.StandardError);
        Assert.InRange(verbose.StandardError.Length, 1, 4096);
        Assert.EndsWith(Environment.NewLine, verbose.StandardError, StringComparison.Ordinal);
        var diagnostic = verbose.StandardError.TrimEnd('\r', '\n');
        Assert.DoesNotContain('\n', diagnostic);
        Assert.DoesNotContain('\r', diagnostic);
    }
}
