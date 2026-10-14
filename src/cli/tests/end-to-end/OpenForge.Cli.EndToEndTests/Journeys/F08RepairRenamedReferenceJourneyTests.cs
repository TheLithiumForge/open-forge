using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;

namespace OpenForge.Cli.EndToEndTests.Journeys;

[Trait("Feature", "repair-renamed-reference-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F08")]
public sealed class F08RepairRenamedReferenceJourneyTests
{
    [Theory(DisplayName = "F08 repairs a tracked manual rename before or after Index without choosing a target")]
    [InlineData(false), InlineData(true)]
    [Trait("Scenarios", "C10-01,C10-05,X37,X42,C02-15,C06-14,C06-01")]
    public async Task ProvenRename(bool indexFirst)
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f08-rename");
        await MaintenanceRenameFixture.SeedAsync(journey);
        var expectedTarget = journey.Document(MaintenanceRenameFixture.Description, ["Guidance"], MaintenanceRenameFixture.RoundingBody);
        var references = await journey.TextAsync(0, true, "references", "guidance/billing/rounding");
        Assert.Contains("notes.md", references.StandardOutput, StringComparison.Ordinal);
        using (var initial = await journey.JsonAsync(0, true, "references", "guidance/billing/rounding"))
            Assert.Contains(initial.RootElement.GetProperty("data").GetProperty("incoming").EnumerateArray(), row => row.GetProperty("path").GetString() == MaintenanceRenameFixture.Notes);
        File.Move(journey.Combine(MaintenanceRenameFixture.Old), journey.Combine(MaintenanceRenameFixture.New));
        if (indexFirst) await journey.TextAsync(0, false, "index");
        var broken = await journey.TextAsync(2, true, "references", "guidance/notes", "--direction", "out");
        Assert.StartsWith(MaintenanceRenameFixture.Notes + Environment.NewLine, broken.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("  out  :9:6   billing/rounding.md#rounding   missing", broken.StandardOutput, StringComparison.Ordinal);
        using (var missing = await journey.JsonAsync(2, true, "references", "guidance/notes", "--direction", "out"))
        {
            Assert.Equal("completed-with-warnings", missing.RootElement.GetProperty("status").GetString());
            var occurrence = Assert.Single(missing.RootElement.GetProperty("data").GetProperty("outgoing").EnumerateArray());
            Assert.Equal(":9:6", occurrence.GetProperty("location").GetString());
            Assert.Equal("billing/rounding.md#rounding", occurrence.GetProperty("destination").GetString());
            Assert.Equal("missing", occurrence.GetProperty("state").GetString());
            Assert.Equal(MaintenanceRenameFixture.Old, occurrence.GetProperty("resolvedPath").GetString());
        }
        using (var doctor = await journey.JsonAsync(2, true, "doctor"))
        {
            Assert.Contains(doctor.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "reference.target-renamed");
            Assert.Equal($"open-forge repair --automatic {journey.WorkspaceFlag}", PublishedMaintenanceJourney.Next(doctor));
        }
        var previewText = await journey.TextAsync(0, true, "repair", "--automatic", "--dry-run");
        AssertProofText(previewText.StandardOutput);
        using (var preview = await journey.JsonAsync(0, true, "repair", "--automatic", "--dry-run"))
        {
            await MaintenanceRenameFixture.AssertRenameAsync(journey, preview, MaintenanceRenameFixture.Old, MaintenanceRenameFixture.New, expectedTarget);
            Assert.Equal(1, preview.RootElement.GetProperty("counts").GetProperty("linksRepaired").GetInt32());
            MaintenanceRenameFixture.AssertNavigation(preview, indexFirst ? 0 : 1);
            var apply = await journey.CopyCommandAsync(PublishedMaintenanceJourney.Next(preview), 0);
            using var applied = JsonDocument.Parse(apply.StandardOutput);
            await MaintenanceRenameFixture.AssertRenameAsync(journey, applied, MaintenanceRenameFixture.Old, MaintenanceRenameFixture.New, expectedTarget);
        }
        journey.AssertBytes(MaintenanceRenameFixture.New, expectedTarget);
        journey.AssertBytes(MaintenanceRenameFixture.Notes, journey.Document("Maintainer references", ["Guidance"], "# Notes\n\nRead [Rounding](billing/money-rounding.md#rounding).\n"));
        Assert.False(File.Exists(journey.Combine(MaintenanceRenameFixture.Old)));
        var navigation = File.ReadAllText(journey.Combine(MaintenanceRenameFixture.Billing));
        Assert.Contains("- [Exact monetary rounding policy](money-rounding.md) - #Guidance\n", navigation, StringComparison.Ordinal);
        Assert.DoesNotContain("](rounding.md)", navigation, StringComparison.Ordinal);
        using (var resolved = await journey.JsonAsync(0, true, "references", "guidance/notes", "--direction", "out"))
        {
            var occurrence = Assert.Single(resolved.RootElement.GetProperty("data").GetProperty("outgoing").EnumerateArray());
            Assert.Equal("billing/money-rounding.md#rounding", occurrence.GetProperty("destination").GetString());
            Assert.Equal("complete", occurrence.GetProperty("state").GetString());
            Assert.Equal(MaintenanceRenameFixture.New, occurrence.GetProperty("resolvedPath").GetString());
        }
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("repair");
        await journey.NoOpAsync("index");
    }

    [Theory(DisplayName = "F08 leaves two strong successors untouched until a printed candidate command is chosen")]
    [InlineData(0), InlineData(1)]
    [Trait("Scenarios", "X39,C06-03")]
    public async Task AmbiguousSuccessors(int chosen)
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f08-choice");
        await MaintenanceRenameFixture.SeedAsync(journey);
        const string alternate = ".agents/guidance/billing/alternate-rounding.md";
        var target = journey.Document(MaintenanceRenameFixture.Description, ["Guidance"], MaintenanceRenameFixture.RoundingBody);
        File.Move(journey.Combine(MaintenanceRenameFixture.Old), journey.Combine(MaintenanceRenameFixture.New));
        journey.Write(alternate, target);
        await journey.TextAsync(0, false, "index");
        var before = journey.Snapshot();
        using (var preview = await journey.JsonAsync(2, true, "repair", "--automatic", "--dry-run")) AssertChoices(preview, alternate);
        var automatic = await journey.TextAsync(2, true, "repair", "--automatic");
        Assert.Equal(before, journey.Snapshot());
        string command;
        using (var json = await journey.JsonAsync(2, true, "repair", "--automatic"))
        {
            AssertChoices(json, alternate);
            Assert.Equal("Choose a target above, then run its command.", PublishedMaintenanceJourney.Next(json));
            var finding = Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray(), row => row.GetProperty("code").GetString() == "repair.guided-finding-remaining");
            var actions = finding.GetProperty("actions").EnumerateArray().ToArray();
            Assert.Equal(2, actions.Length);
            command = actions[chosen].GetProperty("command").GetString()!;
        }
        var guided = await journey.TextAsync(5, true, "repair");
        AssertGuidedText(guided.StandardOutput + guided.StandardError, journey, alternate);
        var candidates = new[] { alternate, MaintenanceRenameFixture.New };
        var source = journey.Document("Maintainer references", ["Guidance"], MaintenanceRenameFixture.IncomingBody);
        var location = MaintenanceRenameFixture.DestinationLocation(MaintenanceRenameFixture.Notes, source, "billing/rounding.md#rounding");
        var selected = candidates[chosen];
        var expectedDestination = PublishedMaintenanceJourney.Quote("billing/rounding.md#rounding");
        var selectedTarget = PublishedMaintenanceJourney.Quote(selected + "#rounding");
        Assert.Equal($"open-forge repair --relink {PublishedMaintenanceJourney.Quote(location)} {expectedDestination} {selectedTarget} --automatic {journey.WorkspaceFlag}", command);
        Assert.Contains(command, automatic.StandardOutput + guided.StandardOutput + guided.StandardError, StringComparison.Ordinal);
        await journey.CopyCommandAsync(command, 0);
        var name = System.IO.Path.GetFileName(selected);
        journey.AssertBytes(MaintenanceRenameFixture.Notes, journey.Document("Maintainer references", ["Guidance"], $"# Notes\n\nRead [Rounding](billing/{name}#rounding).\n"));
        journey.AssertBytes(alternate, target);
        journey.AssertBytes(MaintenanceRenameFixture.New, target);
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("repair");
    }

    private static void AssertChoices(JsonDocument json, string alternate)
    {
        var data = json.RootElement.GetProperty("data");
        Assert.Empty(data.GetProperty("renames").EnumerateArray());
        Assert.Equal(0, json.RootElement.GetProperty("counts").GetProperty("linksRepaired").GetInt32());
        var remaining = Assert.Single(data.GetProperty("remaining").EnumerateArray());
        Assert.Equal("guided", remaining.GetProperty("kind").GetString());
        Assert.Equal([alternate + "#rounding", MaintenanceRenameFixture.New + "#rounding"], remaining.GetProperty("candidates").EnumerateArray().Select(row => row.GetProperty("path").GetString()).Order(StringComparer.Ordinal));
        Assert.All(remaining.GetProperty("candidates").EnumerateArray(), row => Assert.Equal(new[] { "title match: Rounding", "content match: Rounding" }, row.GetProperty("reasons").EnumerateArray().Select(reason => reason.GetString())));
    }

    [Fact(DisplayName = "F08 leaves a deleted note as a precise author edit and never guesses a replacement")]
    [Trait("Scenarios", "X45")]
    public async Task DeletedNote()
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f08-deleted");
        await MaintenanceRenameFixture.SeedAsync(journey);
        File.Delete(journey.Combine(MaintenanceRenameFixture.Old));
        var source = journey.Document("Maintainer references", ["Guidance"], MaintenanceRenameFixture.IncomingBody);
        using (var preview = await journey.JsonAsync(2, true, "repair", "--automatic", "--dry-run"))
        {
            Assert.Empty(preview.RootElement.GetProperty("data").GetProperty("renames").EnumerateArray());
            Assert.Equal(0, preview.RootElement.GetProperty("counts").GetProperty("linksRepaired").GetInt32());
            var remaining = Assert.Single(preview.RootElement.GetProperty("data").GetProperty("remaining").EnumerateArray());
            Assert.Equal("manual", remaining.GetProperty("kind").GetString());
            Assert.Empty(remaining.GetProperty("candidates").EnumerateArray());
        }
        await journey.TextAsync(2, true, "repair", "--automatic");
        journey.AssertBytes(MaintenanceRenameFixture.Notes, source);
        await journey.TextAsync(0, false, "index");
        using (var doctor = await journey.JsonAsync(2, true, "doctor"))
        {
            var finding = Assert.Single(doctor.RootElement.GetProperty("findings").EnumerateArray(), row => row.GetProperty("code").GetString() == "reference.target-missing");
            Assert.Equal($"{MaintenanceRenameFixture.Old} is missing and no replacement was found.", finding.GetProperty("message").GetString());
            Assert.Equal("manual-decision", finding.GetProperty("resolution").GetString());
            Assert.Empty(finding.GetProperty("candidates").EnumerateArray());
            Assert.Equal($"Update or remove this link in {MaintenanceRenameFixture.Notes} at line 9, column 6.", PublishedMaintenanceJourney.Next(doctor));
        }
        Assert.DoesNotContain("](rounding.md)", File.ReadAllText(journey.Combine(MaintenanceRenameFixture.Billing)), StringComparison.Ordinal);
        journey.Write(MaintenanceRenameFixture.Notes, journey.Document("Maintainer references", ["Guidance"], "# Notes\n\nThe author removed the obsolete reference.\n"));
        Assert.False(File.Exists(journey.Combine(MaintenanceRenameFixture.Old)));
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
        await journey.NoOpAsync("repair");
    }

    private static void AssertProofText(string text)
        => Assert.Contains("rounding.md was renamed to billing/money-rounding.md, same content", text, StringComparison.Ordinal);

    private static void AssertGuidedText(string text, PublishedMaintenanceJourney journey, string alternate)
    {
        Assert.Contains("More than one file looks like its new location.", text, StringComparison.Ordinal);
        Assert.Contains($"To use billing/{System.IO.Path.GetFileName(alternate)} (same content):", text, StringComparison.Ordinal);
        Assert.Contains("To use billing/money-rounding.md (same content):", text, StringComparison.Ordinal);
        Assert.Contains("Next: Choose a target above, then run its command.", text, StringComparison.Ordinal);
        Assert.Contains(journey.WorkspaceFlag, text, StringComparison.Ordinal);
    }
}
