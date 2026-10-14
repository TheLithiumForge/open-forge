using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;

namespace OpenForge.Cli.EndToEndTests.Journeys;

[Trait("Feature", "manual-batch-maintenance-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F31")]
public sealed class F31ManualBatchMaintenanceJourneyTests
{
    private const string Deleted = ".agents/guidance/deleted.md";
    private const string Bad = ".agents/memory/crystallized/platform/payments/stripe/bad.md";
    private const string Node = ".agents/memory/crystallized/platform/payments/stripe/node.md";
    private const string BadBytes = "---\nopen-forge:\n  description: [unclosed\n";

    [Fact(DisplayName = "F31 follows Doctor through a mixed batch, explicit author edits, and clean repeated maintenance")]
    [Trait("Scenarios", "X47,X44,X45,X46,C02-18,C06-21")]
    public async Task MixedBatch()
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f31-batch");
        var notesBody = MaintenanceRenameFixture.IncomingBody + "Read [Deleted](deleted.md).\nRead [Archive](archive/index.md).\nRead [Taxes](billing/taxes.md#taxes).\n";
        journey.Write(Deleted, journey.Document("Obsolete historical note", ["Guidance"], "# Deleted\n\nUnique obsolete authored content.\n"));
        const string archiveBody = "# Archive\n\n## Axioms\n\n- Keep archive rules exactly.\n\n## Entries\n\n- none - No entries - #Empty\n";
        journey.Write(".agents/guidance/archive/index.md", journey.Document("Archived team decisions", ["Guidance"], archiveBody));
        const string taxPath = ".agents/guidance/billing/taxes.md";
        const string movedTax = ".agents/guidance/finance/yearly/taxes.md";
        var taxes = journey.Document("Yearly taxation references", ["Guidance"], "# Taxes\n\nRead [Rounding](rounding.md#rounding).\n\n" + string.Concat(Enumerable.Repeat("Keep the tax notes exactly.\n", 30)));
        journey.Write(taxPath, taxes);
        await MaintenanceRenameFixture.SeedAsync(journey, notesBody, commit: false);
        await journey.TextAsync(0, false, "index");
        await journey.CommitAsync();
        File.Move(journey.Combine(MaintenanceRenameFixture.Old), journey.Combine(MaintenanceRenameFixture.New));
        File.Delete(journey.Combine(Deleted));
        journey.Expect(".agents/guidance/history/index.md");
        Directory.Move(journey.Combine(".agents/guidance/archive"), journey.Combine(".agents/guidance/history"));
        journey.Expect(movedTax, ".agents/guidance/finance/_finance.md", ".agents/guidance/finance/yearly/_yearly.md");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(journey.Combine(movedTax)) ?? throw new InvalidOperationException());
        File.Move(journey.Combine(taxPath), journey.Combine(movedTax));
        journey.Write(movedTax, taxes + "One new tax exception.\n");
        journey.Write(".agents/guidance/plain.md", "# New plain note\n\nKeep these exact words.\n");
        journey.Write(".agents/guidance/partial.md", "---\nopen-forge:\n  description: Authored partial # keep\ncustom: kept\n---\n\n# Partial\n\nKeep the partial body.\n");
        journey.Write(Node, "# Stripe notes\n\nKeep the deep body.\n");
        journey.Write(Bad, BadBytes);
        journey.Expect(".agents/memory/crystallized/platform/_platform.md", ".agents/memory/crystallized/platform/payments/_payments.md", ".agents/memory/crystallized/platform/payments/stripe/_stripe.md");
        var doctorText = await journey.TextAsync(2, true, "doctor");
        AssertBatchText(doctorText.StandardOutput);
        using (var doctor = await journey.JsonAsync(2, true, "doctor"))
        {
            var next = PublishedMaintenanceJourney.Next(doctor);
            Assert.Equal($"open-forge repair --automatic {journey.WorkspaceFlag}", next);
            await journey.TextAsync(2, true, "repair", "--automatic", "--dry-run");
        }
        using (var preview = await journey.JsonAsync(2, true, "repair", "--automatic", "--dry-run"))
        {
            Assert.Equal(4, preview.RootElement.GetProperty("counts").GetProperty("linksRepaired").GetInt32());
            Assert.Contains(preview.RootElement.GetProperty("data").GetProperty("remaining").EnumerateArray(), row => row.GetProperty("kind").GetString() == "manual");
            var appliedResult = await journey.CopyCommandAsync(PublishedMaintenanceJourney.Next(preview), 2);
            using var applied = JsonDocument.Parse(appliedResult.StandardOutput);
            Assert.Equal($"open-forge index {journey.WorkspaceFlag}", PublishedMaintenanceJourney.Next(applied));
            using var indexPreview = await journey.JsonAsync(3, true, "index", "--dry-run");
            Assert.Equal(1, indexPreview.RootElement.GetProperty("counts").GetProperty("filesSkipped").GetInt32());
            Assert.Equal(3, indexPreview.RootElement.GetProperty("counts").GetProperty("entrypointsCreated").GetInt32());
            var indexResult = await journey.CopyCommandAsync(PublishedMaintenanceJourney.Next(applied), 3);
            Assert.Contains($"{Bad} has invalid frontmatter. It was left unchanged. Other files were indexed.", indexResult.StandardOutput, StringComparison.Ordinal);
            Assert.Contains($"Next: Fix the frontmatter in {Bad}, then run open-forge index {journey.WorkspaceFlag}.", indexResult.StandardOutput, StringComparison.Ordinal);
        }
        journey.AssertBytes(Bad, BadBytes);
        journey.AssertBytes(Node, journey.Document("Stripe notes", ["Memory", "Crystallized"], "# Stripe notes\n\nKeep the deep body.\n"));
        journey.AssertBytes(".agents/guidance/history/index.md", journey.Document("Archived team decisions", ["Guidance"], archiveBody));
        Assert.False(File.Exists(journey.Combine(Deleted)));
        var repairedBody = "# Notes\n\nRead [Rounding](billing/money-rounding.md#rounding).\nRead [Deleted](deleted.md).\nRead [Archive](history/index.md).\nRead [Taxes](finance/yearly/taxes.md#taxes).\n";
        journey.AssertBytes(MaintenanceRenameFixture.Notes, journey.Document("Maintainer references", ["Guidance"], repairedBody));
        journey.AssertBytes(movedTax, taxes.Replace("(rounding.md#rounding)", "(../../billing/money-rounding.md#rounding)", StringComparison.Ordinal) + "One new tax exception.\n");
        using (var doctor = await journey.JsonAsync(2, true, "doctor"))
        {
            var next = PublishedMaintenanceJourney.Next(doctor);
            Assert.Equal($"Fix the frontmatter in {Bad}, then run open-forge index {journey.WorkspaceFlag}.", next);
        }
        journey.Write(Bad, "# Corrected frontmatter\n\nThe author replaced the invalid header.\n");
        await journey.CopyCommandAsync($"open-forge index {journey.WorkspaceFlag}", 0);
        using (var doctor = await journey.JsonAsync(2, true, "doctor"))
        {
            var missing = Assert.Single(doctor.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "reference.target-missing");
            Assert.Equal("manual-decision", missing.GetProperty("resolution").GetString());
            Assert.Empty(missing.GetProperty("candidates").EnumerateArray());
            Assert.Equal($"{Deleted} is missing and no replacement was found.", missing.GetProperty("message").GetString());
            Assert.Equal($"Update or remove this link in {MaintenanceRenameFixture.Notes} at line 10, column 6.", PublishedMaintenanceJourney.Next(doctor));
        }
        journey.Write(MaintenanceRenameFixture.Notes, journey.Document("Maintainer references", ["Guidance"], "# Notes\n\nRead [Rounding](billing/money-rounding.md#rounding).\nThe author removed the obsolete reference.\nRead [Archive](history/index.md).\nRead [Taxes](finance/yearly/taxes.md#taxes).\n"));
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
        await journey.NoOpAsync("repair");
    }

    [Fact(DisplayName = "F31 reviews an unproven old navigation file beside the new canonical file")]
    [Trait("Scenarios", "X46")]
    public async Task OldNavigationReview()
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f31-old-navigation");
        const string old = ".agents/guidance/invoices/_billing.md";
        const string canonical = ".agents/guidance/invoices/_invoices.md";
        var original = journey.Document("Billing rules", ["Guidance"], "# Billing\n\n## Axioms\n\n- Keep exact monetary rules.\n\n## Entries\n\n- none - No entries - #Empty\n");
        journey.Write(old, original);
        journey.Write(".agents/guidance/invoices/node.md", "# Node\n\nKeep the note.\n");
        journey.Expect(canonical);
        journey.DisableGit();
        await journey.TextAsync(0, false, "index");
        var doctorText = await journey.TextAsync(2, true, "doctor");
        AssertOldNavigationText(doctorText.StandardOutput, old, canonical);
        using (var doctor = await journey.JsonAsync(2, true, "doctor"))
            Assert.Equal($"Copy any rules you want to keep from {old} into {canonical}, then delete {old} and run open-forge index {journey.WorkspaceFlag}.", PublishedMaintenanceJourney.Next(doctor));
        Assert.Contains("- Keep exact monetary rules.", File.ReadAllText(journey.Combine(old)), StringComparison.Ordinal);
        var reviewed = journey.Document("Notes in guidance/invoices", ["Guidance"], "# Invoices\n\nNotes in guidance/invoices.\n\n## Axioms\n\n- Keep exact monetary rules.\n\n## Entries\n\n- [Node](node.md) - #Guidance\n");
        journey.Write(canonical, reviewed);
        File.Delete(journey.Combine(old));
        await journey.CopyCommandAsync($"open-forge index {journey.WorkspaceFlag}", 0);
        journey.AssertBytes(canonical, reviewed);
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
    }

    private static void AssertBatchText(string text)
    {
        Assert.Contains("Repair can update the links and affected Entries.", text, StringComparison.Ordinal);
        Assert.Contains("needs a description or tags. Index can fill the missing fields and keep your content.", text, StringComparison.Ordinal);
    }

    private static void AssertOldNavigationText(string text, string old, string canonical)
    {
        Assert.Contains($"{old} looks like this folder's old navigation file.", text, StringComparison.Ordinal);
        Assert.Contains($"Copy any rules you want to keep from {old} into {canonical}, then delete {old} and run open-forge index", text, StringComparison.Ordinal);
    }
}
