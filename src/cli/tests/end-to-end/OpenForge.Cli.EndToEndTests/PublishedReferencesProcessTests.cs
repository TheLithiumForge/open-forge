using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedReferencesProcessTests
{
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

    [Fact(DisplayName = "Published References outgoing journey preserves external, missing, fragment, duplicate, Unicode, and space facts on stdout"),
     Trait("Feature", "references"), Trait("Evidence", "EndToEnd")]
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
                (value.GetProperty("role").GetString(), value.GetProperty("value").GetString())));
        var occurrence = Assert.Single(
            document.RootElement.GetProperty("result").GetProperty("incoming").GetProperty("occurrences").EnumerateArray());
        Assert.Equal("filtered-incoming-scan", occurrence.GetProperty("provenance").GetString());
        Assert.Equal("alpha", occurrence.GetProperty("source").GetProperty("id").GetString());
    }

}
