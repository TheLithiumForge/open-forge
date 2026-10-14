using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;

namespace OpenForge.Cli.EndToEndTests.Journeys;

[Trait("Feature", "manual-reorganization-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F29")]
public sealed class F29ManualReorganizationJourneyTests
{
    private const string Taxes = ".agents/guidance/billing/taxes.md";
    private const string MovedTaxes = ".agents/guidance/invoices/yearly/taxes.md";
    private const string Invoices = ".agents/guidance/invoices/_invoices.md";
    private const string Yearly = ".agents/guidance/invoices/yearly/_yearly.md";
    private const string TaxBody = "# Taxes\n\nRead [Rounding](rounding.md#rounding).\n\nKeep the tax notes exactly.\n";

    [Theory(DisplayName = "F29 repairs incoming and outgoing links after an in-place rename and cross-folder move in either order")]
    [InlineData(false, false), InlineData(true, false), InlineData(false, true), InlineData(true, true)]
    [Trait("Scenarios", "X38,X42,X43,C02-15,C06-15")]
    public async Task RenameAndMove(bool indexFirst, bool edited)
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f29-move");
        var incoming = MaintenanceRenameFixture.IncomingBody + "Read [Taxes](billing/taxes.md#taxes).\n";
        journey.Write(Taxes, journey.Document("Yearly taxation references", ["Guidance"], TaxBody));
        await MaintenanceRenameFixture.SeedAsync(journey, incoming, commit: false);
        await journey.TextAsync(0, false, "index");
        await journey.CommitAsync();
        journey.Expect(MovedTaxes, Invoices, Yearly);
        var originalRounding = journey.Document(MaintenanceRenameFixture.Description, ["Guidance"], MaintenanceRenameFixture.RoundingBody);
        var rounding = originalRounding + (edited ? "One new exception.\n" : "");
        File.Move(journey.Combine(MaintenanceRenameFixture.Old), journey.Combine(MaintenanceRenameFixture.New));
        journey.Write(MaintenanceRenameFixture.New, rounding);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(journey.Combine(MovedTaxes)) ?? throw new InvalidOperationException());
        File.Move(journey.Combine(Taxes), journey.Combine(MovedTaxes));
        if (indexFirst) await journey.TextAsync(0, false, "index");
        using (var diagnosis = await journey.JsonAsync(2, true, "doctor"))
            Assert.Equal($"open-forge repair --automatic {journey.WorkspaceFlag}", PublishedMaintenanceJourney.Next(diagnosis));
        var previewText = await journey.TextAsync(0, true, "repair", "--automatic", "--dry-run");
        AssertMoveText(previewText.StandardOutput, edited);
        using (var preview = await journey.JsonAsync(0, true, "repair", "--automatic", "--dry-run"))
        {
            var data = preview.RootElement.GetProperty("data");
            Assert.Equal(2, data.GetProperty("renames").GetArrayLength());
            var renamed = Assert.Single(data.GetProperty("renames").EnumerateArray(), row => row.GetProperty("oldPath").GetString() == MaintenanceRenameFixture.Old);
            Assert.Equal(MaintenanceRenameFixture.New, renamed.GetProperty("newPath").GetString());
            Assert.Equal(edited ? "git-rename" : "committed-content", renamed.GetProperty("evidence").GetString());
            if (edited)
            {
                var oldLength = Encoding.UTF8.GetByteCount(originalRounding);
                var newLength = Encoding.UTF8.GetByteCount(rounding);
                Assert.Equal(100 * 2 * oldLength / (oldLength + newLength), renamed.GetProperty("similarity").GetInt32());
            }
            else Assert.Equal(JsonValueKind.Null, renamed.GetProperty("similarity").ValueKind);
            Assert.Equal(PublishedMaintenanceJourney.Hash(rounding), renamed.GetProperty("proof").GetProperty("newHash").GetString());
            var moved = Assert.Single(data.GetProperty("renames").EnumerateArray(), row => row.GetProperty("oldPath").GetString() == Taxes);
            Assert.Equal(MovedTaxes, moved.GetProperty("newPath").GetString());
            Assert.Equal("committed-content", moved.GetProperty("evidence").GetString());
            Assert.Equal(3, preview.RootElement.GetProperty("counts").GetProperty("linksRepaired").GetInt32());
            MaintenanceRenameFixture.AssertNavigation(preview, indexFirst ? 0 : 4, indexFirst ? 0 : 2);
            if (!indexFirst)
            {
                var expectedNavigation = new Dictionary<string, (int Before, int After)>
                {
                    [MaintenanceRenameFixture.Billing] = (2, 1),
                    [PublishedMaintenanceJourney.Guidance] = (2, 3),
                    [Invoices] = (0, 1),
                    [Yearly] = (0, 1),
                };
                foreach (var row in data.GetProperty("navigation").EnumerateArray())
                {
                    var expected = expectedNavigation[row.GetProperty("path").GetString() ?? throw new InvalidOperationException()];
                    Assert.Equal(expected.Before, row.GetProperty("before").GetInt32());
                    Assert.Equal(expected.After, row.GetProperty("after").GetInt32());
                }
            }
            var appliedResult = await journey.CopyCommandAsync(PublishedMaintenanceJourney.Next(preview), 0);
            using var applied = JsonDocument.Parse(appliedResult.StandardOutput);
            Assert.Equal(3, applied.RootElement.GetProperty("counts").GetProperty("linksRepaired").GetInt32());
        }
        journey.AssertBytes(MaintenanceRenameFixture.New, rounding);
        journey.AssertBytes(MovedTaxes, journey.Document("Yearly taxation references", ["Guidance"], "# Taxes\n\nRead [Rounding](../../billing/money-rounding.md#rounding).\n\nKeep the tax notes exactly.\n"));
        journey.AssertBytes(MaintenanceRenameFixture.Notes, journey.Document("Maintainer references", ["Guidance"], "# Notes\n\nRead [Rounding](billing/money-rounding.md#rounding).\nRead [Taxes](invoices/yearly/taxes.md#taxes).\n"));
        Assert.False(File.Exists(journey.Combine(Taxes)));
        Assert.False(File.Exists(journey.Combine(MaintenanceRenameFixture.Old)));
        journey.AssertBytes(Invoices, journey.Scaffold("guidance/invoices", "Invoices", ["Guidance"], "- [Notes in guidance/invoices/yearly](yearly/_yearly.md) - #Guidance"));
        journey.AssertBytes(Yearly, journey.Scaffold("guidance/invoices/yearly", "Yearly", ["Guidance"], "- [Yearly taxation references](taxes.md) - #Guidance"));
        Assert.DoesNotContain("](rounding.md)", File.ReadAllText(journey.Combine(MaintenanceRenameFixture.Billing)), StringComparison.Ordinal);
        Assert.DoesNotContain("](taxes.md)", File.ReadAllText(journey.Combine(MaintenanceRenameFixture.Billing)), StringComparison.Ordinal);
        await journey.TextAsync(0, true, "references", "guidance/notes", "--direction", "out");
        await journey.TextAsync(0, true, "references", "guidance/invoices/yearly/taxes", "--direction", "out");
        await journey.TextAsync(0, true, "context", "guidance/billing/money-rounding", "guidance/invoices/yearly/taxes");
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("repair");
        await journey.NoOpAsync("index");
    }

    [Theory(DisplayName = "F29 preserves canonical navigation rules when the entire folder is renamed in either order")]
    [InlineData(false), InlineData(true)]
    [Trait("Scenarios", "X46,X42,C06-21")]
    public async Task CanonicalFolder(bool indexFirst)
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f29-folder");
        const string originalBody = "# Billing\n\n## Axioms\n\n- Keep exact monetary rules.\n\n## Entries\n\n- [Exact monetary rounding policy](rounding.md) - #Guidance\n";
        await MaintenanceRenameFixture.SeedAsync(journey, MaintenanceRenameFixture.IncomingBody + "Read [Billing](billing/_billing.md).\n");
        journey.Expect(Invoices, ".agents/guidance/invoices/_billing.md", ".agents/guidance/invoices/rounding.md");
        Directory.Move(journey.Combine(".agents/guidance/billing"), journey.Combine(".agents/guidance/invoices"));
        if (indexFirst)
        {
            var index = await journey.TextAsync(0, false, "index");
            AssertFolderText(index.StandardOutput);
        }
        await journey.TextAsync(0, true, "repair", "--automatic", "--dry-run");
        using (var preview = await journey.JsonAsync(0, true, "repair", "--automatic", "--dry-run"))
        {
            Assert.Equal(2, preview.RootElement.GetProperty("counts").GetProperty("linksRepaired").GetInt32());
            await journey.CopyCommandAsync(PublishedMaintenanceJourney.Next(preview), 0);
        }
        journey.AssertBytes(Invoices, journey.Document("Billing rules", ["Guidance"], originalBody));
        journey.AssertBytes(".agents/guidance/invoices/rounding.md", journey.Document(MaintenanceRenameFixture.Description, ["Guidance"], MaintenanceRenameFixture.RoundingBody));
        journey.AssertBytes(MaintenanceRenameFixture.Notes, journey.Document("Maintainer references", ["Guidance"], "# Notes\n\nRead [Rounding](invoices/rounding.md#rounding).\nRead [Billing](invoices/_invoices.md).\n"));
        Assert.False(File.Exists(journey.Combine(".agents/guidance/invoices/_billing.md")));
        Assert.False(Directory.Exists(journey.Combine(".agents/guidance/billing")));
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
        await journey.NoOpAsync("repair");
    }

    [Theory(DisplayName = "F29 without Git uses stale metadata before Index and explicit intent after Index")]
    [InlineData(false), InlineData(true)]
    [Trait("Scenarios", "X41,X42,X40")]
    public async Task GitUnavailable(bool indexFirst)
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f29-no-git");
        await MaintenanceRenameFixture.SeedAsync(journey, commit: false);
        File.Move(journey.Combine(MaintenanceRenameFixture.Old), journey.Combine(MaintenanceRenameFixture.New));
        journey.DisableGit();
        if (indexFirst) await journey.TextAsync(0, false, "index");
        using var preview = await journey.JsonAsync(indexFirst ? 2 : 0, true, "repair", "--automatic", "--dry-run");
        if (!indexFirst)
        {
            var row = Assert.Single(preview.RootElement.GetProperty("data").GetProperty("renames").EnumerateArray());
            Assert.Equal("recorded-metadata", row.GetProperty("evidence").GetString());
            Assert.Equal(System.Text.Json.JsonValueKind.Null, row.GetProperty("proof").GetProperty("head").ValueKind);
            await journey.CopyCommandAsync(PublishedMaintenanceJourney.Next(preview), 0);
        }
        else
        {
            Assert.Empty(preview.RootElement.GetProperty("data").GetProperty("renames").EnumerateArray());
            var source = journey.Document("Maintainer references", ["Guidance"], MaintenanceRenameFixture.IncomingBody);
            var location = MaintenanceRenameFixture.DestinationLocation(MaintenanceRenameFixture.Notes, source, "billing/rounding.md#rounding");
            await journey.TextAsync(0, false, "repair", "--automatic", "--relink", location, "billing/rounding.md#rounding", MaintenanceRenameFixture.New + "#rounding");
        }
        journey.AssertBytes(MaintenanceRenameFixture.Notes, journey.Document("Maintainer references", ["Guidance"], "# Notes\n\nRead [Rounding](billing/money-rounding.md#rounding).\n"));
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("repair");
    }

    private static void AssertMoveText(string text, bool edited)
    {
        Assert.Contains("taxes.md was moved to invoices/yearly/taxes.md, same content", text, StringComparison.Ordinal);
        Assert.Contains(edited ? "content is " : "same content", text, StringComparison.Ordinal);
    }

    private static void AssertFolderText(string text)
        => Assert.Contains("renamed from _billing.md, your rules were kept", text, StringComparison.Ordinal);
}
