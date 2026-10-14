using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

[Trait("Feature", "repair-renamed-reference-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F08")]
public sealed class F08RepairIsolationJourneyTests
{
    private const string LibraryId = "f08-library";
    private const string LibrarySourceRoot = "f08-library-source";
    private const string LibrarySource = "f08-library-source/library-note.md";
    private const string LibraryRoot = ".agents/library-drift";
    private const string LibraryNote = ".agents/library-drift/library-note.md";
    private const string Ownership = ".agents/open-forge.lock.json";
    private const string Drift = "Unrelated user-owned Library drift.\n";
    private const string Destination = "billing/rounding.md#rounding";
    private const string Location = ".agents/guidance/notes.md@9:17";
    private const string Target = ".agents/guidance/billing/money-rounding.md#rounding";

    [Fact(DisplayName = "F08 exact relink succeeds while unrelated Library drift stays intact"), Trait("Scenarios", "X16,C06-05")]
    public async Task ExactRelinkPreservesUnrelatedLibraryDrift()
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f08-library-drift");
        await MaintenanceRenameFixture.SeedAsync(journey);
        journey.Expect(Ownership, LibraryNote);
        var librarySource = journey.Document("Library note", ["Reference"], "# Library note\n\nUnrelated Library source bytes.\n");
        journey.Write(LibrarySource, librarySource);
        await journey.TextAsync(0, false, "library", "attach", LibraryId, LibrarySourceRoot, "--to", LibraryRoot, "--automatic");
        var driftPath = journey.Combine(LibraryNote);
        var originalLink = new FileInfo(driftPath).LinkTarget;
        Assert.NotNull(originalLink);
        File.Delete(driftPath);
        journey.Write(LibraryNote, Drift);
        File.Move(journey.Combine(MaintenanceRenameFixture.Old), journey.Combine(MaintenanceRenameFixture.New));
        var record = File.ReadAllBytes(journey.Combine(Ownership));
        var before = PublishedWorkspaceTreeSnapshot.Capture(journey.Path);
        var doctor = await journey.TextAsync(2, true, "doctor");
        Assert.Contains("library-note.md", doctor.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("rounding.md", doctor.StandardOutput, StringComparison.Ordinal);
        var preview = await journey.TextAsync(2, true, [.. RelinkArguments, "--dry-run"]);
        AssertLibraryRepairText(preview.StandardOutput, journey, preview: true);
        using (var json = await journey.JsonAsync(2, true, [.. RelinkArguments, "--dry-run"])) AssertSelectedRepair(json);
        var applied = await journey.TextAsync(2, false, RelinkArguments);
        AssertLibraryRepairText(applied.StandardOutput, journey, preview: false);
        AssertRenamedBytes(journey);
        journey.AssertBytes(LibraryNote, Drift);
        journey.AssertBytes(LibrarySource, librarySource);
        Assert.Null(new FileInfo(driftPath).LinkTarget);
        Assert.Equal(record, File.ReadAllBytes(journey.Combine(Ownership)));
        PublishedJourneyAssertions.AssertOnlyFileMutations(before, PublishedWorkspaceTreeSnapshot.Capture(journey.Path),
            MaintenanceRenameFixture.Notes, MaintenanceRenameFixture.Billing);
        using (var references = await journey.JsonAsync(0, true, "references", "guidance/notes", "--direction", "out"))
        {
            var occurrence = Assert.Single(references.RootElement.GetProperty("data").GetProperty("outgoing").EnumerateArray());
            Assert.Equal("complete", occurrence.GetProperty("state").GetString());
            Assert.Equal("billing/money-rounding.md#rounding", occurrence.GetProperty("destination").GetString());
            Assert.Equal(MaintenanceRenameFixture.New, occurrence.GetProperty("resolvedPath").GetString());
        }
        var remaining = await journey.TextAsync(2, true, "doctor");
        Assert.Contains("library-note.md", remaining.StandardOutput, StringComparison.Ordinal);
        using (var diagnosis = await journey.JsonAsync(2, true, "doctor"))
        {
            Assert.Equal(1, diagnosis.RootElement.GetProperty("counts").GetProperty("errors").GetInt32());
            Assert.Equal(1, diagnosis.RootElement.GetProperty("counts").GetProperty("warnings").GetInt32());
            var problems = diagnosis.RootElement.GetProperty("findings").EnumerateArray()
                .Where(finding => finding.GetProperty("severity").GetString() is "warning" or "error").ToArray();
            Assert.Equal(2, problems.Length);
            Assert.All(problems, finding => Assert.Equal(LibraryNote, finding.GetProperty("subject").GetProperty("path").GetString()));
        }
        // The maintainer resolves the unrelated drift after verifying Repair preserved it.
        File.Delete(driftPath);
        File.CreateSymbolicLink(driftPath, originalLink);
        await journey.TextAsync(0, false, "library", "detach", LibraryId, "--automatic");
        journey.AssertBytes(LibrarySource, librarySource);
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("repair");
        await journey.NoOpAsync("index");
    }

    [Fact(DisplayName = "F08 unsafe required navigation blocks exact relink before any write"), Trait("Scenarios", "X16,C06-05")]
    public async Task UnsafeRequiredNavigationBlocksExactRelinkWithoutWrites()
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f08-unsafe-navigation");
        await MaintenanceRenameFixture.SeedAsync(journey);
        File.Move(journey.Combine(MaintenanceRenameFixture.Old), journey.Combine(MaintenanceRenameFixture.New));
        var source = journey.Document("Maintainer references", ["Guidance"], MaintenanceRenameFixture.IncomingBody);
        var navigation = journey.Combine(PublishedMaintenanceJourney.Guidance);
        var navigationBytes = File.ReadAllBytes(navigation);
        File.Delete(navigation);
        Directory.CreateDirectory(navigation);
        try
        {
            Assert.Equal(FileAttributes.Directory, File.GetAttributes(navigation) & (FileAttributes.Directory | FileAttributes.ReparsePoint));
            var before = journey.Snapshot();
            var blocked = await journey.TextAsync(5, true, RelinkArguments);
            Assert.Contains("guidance", blocked.StandardError, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(before, journey.Snapshot());
            journey.AssertBytes(MaintenanceRenameFixture.Notes, source);
            journey.AssertBytes(MaintenanceRenameFixture.New,
                journey.Document(MaintenanceRenameFixture.Description, ["Guidance"], MaintenanceRenameFixture.RoundingBody));
        }
        finally
        {
            Directory.Delete(navigation, recursive: false);
            journey.WriteBytes(PublishedMaintenanceJourney.Guidance, navigationBytes);
        }
        await journey.TextAsync(0, false, RelinkArguments);
        AssertRenamedBytes(journey);
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("repair");
        await journey.NoOpAsync("index");
    }

    private static string[] RelinkArguments => ["repair", "--automatic", "--relink", Location, Destination, Target];

    private static void AssertSelectedRepair(JsonDocument json)
    {
        Assert.Equal("completed-with-warnings", json.RootElement.GetProperty("status").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("counts").GetProperty("linksRepaired").GetInt32());
        MaintenanceRenameFixture.AssertNavigation(json, 1);
    }

    private static void AssertLibraryRepairText(string text, PublishedMaintenanceJourney journey, bool preview)
    {
        Assert.Contains(preview ? "Would repair 1 link" : "Repaired 1 link", text, StringComparison.Ordinal);
        Assert.Contains("library-note.md", text, StringComparison.Ordinal);
        Assert.Contains("notes.md", text, StringComparison.Ordinal);
        var next = preview
            ? $"open-forge repair --automatic --relink {PublishedMaintenanceJourney.Quote(Location)} {PublishedMaintenanceJourney.Quote(Destination)} {PublishedMaintenanceJourney.Quote(Target)} {journey.WorkspaceFlag}"
            : $"open-forge doctor {journey.WorkspaceFlag}";
        Assert.Contains($"Next: {next}{Environment.NewLine}", text, StringComparison.Ordinal);
    }

    private static void AssertRenamedBytes(PublishedMaintenanceJourney journey)
    {
        journey.AssertBytes(MaintenanceRenameFixture.Notes,
            journey.Document("Maintainer references", ["Guidance"], "# Notes\n\nRead [Rounding](billing/money-rounding.md#rounding).\n"));
        journey.AssertBytes(MaintenanceRenameFixture.New,
            journey.Document(MaintenanceRenameFixture.Description, ["Guidance"], MaintenanceRenameFixture.RoundingBody));
        journey.AssertBytes(MaintenanceRenameFixture.Billing,
            journey.Document("Billing rules", ["Guidance"], "# Billing\n\n## Axioms\n\n- Keep exact monetary rules.\n\n## Entries\n\n"
                + "- [Exact monetary rounding policy](money-rounding.md) - #Guidance\n"));
        Assert.False(File.Exists(journey.Combine(MaintenanceRenameFixture.Old)));
    }
}
