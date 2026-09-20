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
        Assert.StartsWith(".agents/docs.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("  out  ", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Direction:", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Direct links", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Inspected:", result.StandardOutput, StringComparison.Ordinal);
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
            ["references", "docs", "--direction=out", "--format=json", "--detail=full"]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("completed-with-warnings", document.RootElement.GetProperty("status").GetString());
        var occurrences = document.RootElement.GetProperty("data").GetProperty("outgoing").EnumerateArray().ToArray();
        Assert.Equal(8, occurrences.Length);
        Assert.Equal(
            ["target.md#Overview", "target.md#Overview", "space file.md", "unicodé.md#Café", "missing.md#Absent", "#Overview", "https://example.invalid/reference", "target.md#Overview"],
            occurrences.Select(value => value.GetProperty("destination").GetString()));
        Assert.Equal("external-unchecked", occurrences[6].GetProperty("state").GetString());
        Assert.Equal("missing", occurrences[4].GetProperty("state").GetString());
        Assert.Equal("fragment-missing", occurrences[5].GetProperty("state").GetString());
        Assert.Equal(["base", "base", "base", "base", "base", "base", "base", "overwrite"], occurrences.Select(value => value.GetProperty("layer").GetString()));

        // An external destination resolves to nothing, so the nullable member is omitted.
        Assert.False(occurrences[6].TryGetProperty("resolvedPath", out _));
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
            ["references", "docs", "--direction=in", "--include", "alpha", "--exclude=beta", "--format=json", "--detail=full"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var filters = document.RootElement.GetProperty("data").GetProperty("filters");
        Assert.Equal(["alpha"], filters.GetProperty("include").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(["beta"], filters.GetProperty("exclude").EnumerateArray().Select(value => value.GetString()));
        var occurrence = Assert.Single(
            document.RootElement.GetProperty("data").GetProperty("incoming").EnumerateArray());
        Assert.Equal(".agents/alpha.md", occurrence.GetProperty("path").GetString());

        // The scan that produced the row is evidence in its own right: only the included source
        // was read.
        Assert.Equal(
            [".agents/alpha.md"],
            document.RootElement.GetProperty("data").GetProperty("scanned").EnumerateArray()
                .Select(value => value.GetProperty("path").GetString()));
    }

}
