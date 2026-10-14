using System.Text;
using System.Text.Json;

namespace OpenForge.Cli.EndToEndTests.Shared.Journeys;

internal static class MaintenanceRenameFixture
{
    internal const string Old = ".agents/guidance/billing/rounding.md";
    internal const string New = ".agents/guidance/billing/money-rounding.md";
    internal const string Billing = ".agents/guidance/billing/_billing.md";
    internal const string Notes = ".agents/guidance/notes.md";
    internal const string Description = "Exact monetary rounding policy";
    internal const string IncomingBody = "# Notes\n\nRead [Rounding](billing/rounding.md#rounding).\n";
    internal static string RoundingBody { get; } = "# Rounding\n\n" + string.Concat(Enumerable.Range(1, 30).Select(number => $"Rule {number:D2}: keep monetary amounts in minor units.\n"));

    internal static async Task SeedAsync(PublishedMaintenanceJourney journey, string? notesBody = null, bool commit = true)
    {
        journey.Expect(New);
        journey.Write(Billing, journey.Document("Billing rules", ["Guidance"], $"# Billing\n\n## Axioms\n\n- Keep exact monetary rules.\n\n## Entries\n\n- none - No entries - #Empty\n"));
        journey.Write(Old, journey.Document(Description, ["Guidance"], RoundingBody));
        journey.Write(Notes, journey.Document("Maintainer references", ["Guidance"], notesBody ?? IncomingBody));
        await journey.TextAsync(0, false, "index");
        await journey.CleanDoctorAsync();
        if (commit) await journey.CommitAsync();
    }

    internal static async Task AssertRenameAsync(PublishedMaintenanceJourney journey, JsonDocument json, string oldPath, string newPath, string expectedBytes)
    {
        var row = Assert.Single(json.RootElement.GetProperty("data").GetProperty("renames").EnumerateArray(), row => row.GetProperty("oldPath").GetString() == oldPath);
        Assert.Equal(newPath, row.GetProperty("newPath").GetString());
        Assert.Equal("committed-content", row.GetProperty("evidence").GetString());
        Assert.Equal(JsonValueKind.Null, row.GetProperty("similarity").ValueKind);
        var proof = row.GetProperty("proof");
        Assert.Equal(await journey.GitAsync("rev-parse", "HEAD"), proof.GetProperty("head").GetString());
        Assert.Equal(await journey.GitAsync("rev-parse", $"HEAD:{oldPath}"), proof.GetProperty("oldBlob").GetString());
        Assert.Equal(PublishedMaintenanceJourney.Hash(expectedBytes), proof.GetProperty("oldHash").GetString());
        Assert.Equal(PublishedMaintenanceJourney.Hash(expectedBytes), proof.GetProperty("newHash").GetString());
        Assert.Equal(Description, proof.GetProperty("metadata").GetProperty("description").GetString());
        Assert.Equal(["Guidance"], proof.GetProperty("metadata").GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()));
        Assert.Empty(proof.GetProperty("metadata").GetProperty("applyTo").EnumerateArray());
    }

    internal static void AssertNavigation(JsonDocument json, int entries, int entrypoints = 0)
    {
        var data = json.RootElement.GetProperty("data");
        Assert.Equal(entries, data.GetProperty("navigation").GetArrayLength());
        Assert.Equal(entries, json.RootElement.GetProperty("counts").GetProperty("entriesUpdated").GetInt32());
        Assert.Equal(entrypoints, data.GetProperty("entrypoints").GetArrayLength());
        Assert.Equal(entrypoints, json.RootElement.GetProperty("counts").GetProperty("entrypointsCreated").GetInt32());
        Assert.Empty(data.GetProperty("metadata").EnumerateArray());
        Assert.Equal(0, json.RootElement.GetProperty("counts").GetProperty("metadataCompleted").GetInt32());
        Assert.All(data.GetProperty("navigation").EnumerateArray(), row =>
        {
            Assert.NotNull(row.GetProperty("path").GetString());
            Assert.True(row.GetProperty("before").GetInt32() >= 0);
            Assert.True(row.GetProperty("after").GetInt32() >= 0);
        });
    }

    internal static string DestinationLocation(string path, string document, string destination)
    {
        var start = document.IndexOf(destination, StringComparison.Ordinal);
        Assert.True(start >= 0);
        var line = 1 + document[..start].Count(character => character == '\n');
        var previousNewline = document.LastIndexOf('\n', start);
        var column = document[(previousNewline + 1)..start].EnumerateRunes().Count() + 1;
        return $"{path}@{line}:{column}";
    }
}
