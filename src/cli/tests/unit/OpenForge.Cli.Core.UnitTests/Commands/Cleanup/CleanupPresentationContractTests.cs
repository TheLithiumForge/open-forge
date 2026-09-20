using System.Reflection;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Presentation.Cleanup;
using OpenForge.Cli.Core.Presentation.Cleanup.Models;
using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Help;
using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup;

public sealed class CleanupPresentationContractTests
{
    [Trait("Boundary", "Output")]
    [Fact(
        DisplayName = "Cleanup native data preserves frozen property order and nullable boundaries"),
     Trait("Feature", "cleanup-presentation"),
     Trait("Evidence", "Unit")]
    public void NativeDataPreservesPropertyOrderAndNullableBoundaries()
    {
        AssertProperties<CleanupData>("Mode", "Items", "NotEligible", "Lock", "FinalCheck");
        AssertProperties<CleanupDataItem>("Path", "Kind", "Outcome", "Origin", "Integrity");
        AssertProperties<CleanupDataNotEligible>("Path", "Reason");

        AssertNotNullable<CleanupData>(nameof(CleanupData.Mode));
        AssertNotNullable<CleanupData>(nameof(CleanupData.Items));
        AssertNullable<CleanupData>(nameof(CleanupData.NotEligible));
        AssertNullable<CleanupData>(nameof(CleanupData.Lock));
        AssertNullable<CleanupData>(nameof(CleanupData.FinalCheck));
        AssertNullable<CleanupDataItem>(nameof(CleanupDataItem.Origin));
        AssertNullable<CleanupDataItem>(nameof(CleanupDataItem.Integrity));
        AssertNotNullable<CleanupDataNotEligible>(nameof(CleanupDataNotEligible.Path));
        AssertNotNullable<CleanupDataNotEligible>(nameof(CleanupDataNotEligible.Reason));
    }

    [Trait("Boundary", "Output")]
    [Fact(
        DisplayName = "Cleanup native rendering uses one typed report for exact text and JSON"),
     Trait("Feature", "cleanup-presentation"),
     Trait("Evidence", "Unit")]
    public void NativeRenderingUsesOneTypedReportForJsonAndText()
    {
        var plan = CleanupTestData.Plan(CleanupTestData.Request(mode: CleanupMode.DryRun));
        var facts = CleanupTestData.Facts(
            plan,
            effects: [CleanupTestData.Effect(plan.Entries.Single(), CleanupEffectOutcome.Planned)]);
        var result = CleanupTestData.Result(facts: facts);
        var selected = Select(result, CliDetail.Minimal);

        var text = CliTextRenderer.Render(
            selected,
            CliTextStyle.Plain,
            CleanupPresentation.Rendering.DataTextRenderer).Content;
        Assert.StartsWith("Would remove 1 recovery bundle.", text, StringComparison.Ordinal);
        Assert.Contains("open-forge-cleanup-recovery", text, StringComparison.Ordinal);
        Assert.Contains("No files were changed.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("deleted", text, StringComparison.Ordinal);

        using var json = JsonDocument.Parse(
            CliJsonRenderer.Render(selected, CleanupPresentation.Rendering.DataJsonTypeInfo));
        var root = json.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("cleanup", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal("dry-run", root.GetProperty("data").GetProperty("mode").GetString());
        var item = Assert.Single(root.GetProperty("data").GetProperty("items").EnumerateArray());
        Assert.Equal("bundle", item.GetProperty("kind").GetString());
        Assert.Equal("would-be-removed", item.GetProperty("outcome").GetString());
        Assert.Equal(1, root.GetProperty("counts").GetProperty("bundlesRemoved").GetInt32());
        Assert.Single(root.GetProperty("effects").EnumerateArray());
    }

    [Trait("Boundary", "Output")]
    [Fact(
        DisplayName = "Cleanup minimal warning keeps the damaged path beside independent removal"),
     Trait("Feature", "cleanup-presentation"),
     Trait("Evidence", "Unit")]
    public void MinimalWarningKeepsDamagedPathBesideIndependentRemoval()
    {
        var eligiblePath = Path.Combine(Path.GetTempPath(), "open-forge-cleanup-recovery", "eligible.recovery.json");
        var damagedPath = Path.Combine(Path.GetTempPath(), "open-forge-cleanup-recovery", "damaged.recovery.json");
        var plan = CleanupTestData.Plan(
            CleanupTestData.Request(),
            CleanupTestData.Catalogue(
                candidates:
                [
                    CleanupTestData.Candidate(
                        integrity: RecoveryBundleIntegrity.Malformed,
                        path: damagedPath),
                    CleanupTestData.Candidate(path: eligiblePath),
                ]));
        var finding = CleanupFinding.Create(
            CleanupFindingCode.RecoveryFinalMalformed,
            "The stored bytes are not a valid recovery bundle.",
            damagedPath);
        var result = CleanupTestData.Result(
            CleanupTestData.Facts(plan, findings: [finding]),
            status: CliSemanticStatus.Attention);

        var minimal = Select(result, CliDetail.Minimal);
        var minimalText = CliTextRenderer.Render(
            minimal,
            CliTextStyle.Plain,
            CleanupPresentation.Rendering.DataTextRenderer).Content;
        Assert.StartsWith("Removed 1 recovery bundle.", minimalText, StringComparison.Ordinal);
        Assert.Contains(damagedPath, minimalText, StringComparison.Ordinal);
        Assert.Contains("damaged", minimalText, StringComparison.Ordinal);
        Assert.Single(minimal.TextFindings, finding =>
            finding.Code == CleanupWording.FindingCode(CleanupFindingCode.RecoveryFinalMalformed));

        var standard = Select(result, CliDetail.Standard);
        Assert.Single(standard.TextFindings, finding =>
            finding.Code == CleanupWording.FindingCode(CleanupFindingCode.RecoveryFinalMalformed));
    }

    [Trait("Boundary", "Output")]
    [Fact(
        DisplayName = "Cleanup native wording maps every finite finding code"),
     Trait("Feature", "cleanup-presentation"),
     Trait("Evidence", "Unit")]
    public void FindingVocabularyMapsEveryFiniteValue()
    {
        var values = Enum.GetValues<CleanupFindingCode>();
        Assert.All(values, code =>
        {
            Assert.StartsWith("cleanup.", CleanupWording.FindingCode(code), StringComparison.Ordinal);
            Assert.NotEqual(string.Empty, CleanupWording.FindingTitle(code));
        });

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CleanupWording.FindingCode((CleanupFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CleanupWording.FindingTitle((CleanupFindingCode)int.MaxValue));
    }

    [Trait("Boundary", "Output")]
    [Fact(
        DisplayName = "Cleanup help retains stable sections and names the non-recursive command boundary"),
     Trait("Feature", "cleanup-presentation"),
     Trait("Evidence", "Unit")]
    public void HelpRetainsStableSectionsAndBoundaries()
    {
        var help = CleanupHelpSections.Create();
        var headings = help.Sections.Select(section => section.Heading);
        var text = string.Join(
            Environment.NewLine,
            help.Sections.Select(section => $"{section.Heading}\n{section.Body}"));

        Assert.Equal(["Syntax", "Catalogue", "Write policy", "Global options", "Notes"], headings);
        Assert.Contains("open-forge cleanup [--dry-run] [global options]", text, StringComparison.Ordinal);
        Assert.Contains("every recognized completed recovery bundle and draft with an exact expected name", text, StringComparison.Ordinal);
        Assert.Contains("without locking the workspace or writing files", text, StringComparison.Ordinal);
        Assert.Contains("--workspace <path>, --format <text|json>, --detail <minimal|standard|full|debug>, --detail-filter <error|warning|info|all>, --help, and --version", text, StringComparison.Ordinal);
        Assert.Contains("accepts no operands, selectors, prompts, confirmations", text, StringComparison.Ordinal);
        Assert.Contains("force mode, age filters, glob filters,", text, StringComparison.Ordinal);
        Assert.Contains("or arbitrary recursive deletion.", text, StringComparison.Ordinal);
        Assert.Contains("rechecks the exact catalogue before making changes", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", text, StringComparison.Ordinal);
    }

    private static CliSelectedReport<CleanupData> Select(CleanupResult result, CliDetail detail)
    {
        var selection = new CliSelection(detail, null);
        var rendering = CleanupPresentation.Rendering;
        var selected = CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape);
        return rendering.SelectText!(selected);
    }

    private static void AssertProperties<T>(params string[] expected)
        => Assert.Equal(
            expected,
            typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name));

    private static PropertyInfo Property<T>(string name)
        => typeof(T).GetProperty(name, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Missing property {typeof(T).Name}.{name}.");

    private static void AssertNullable<T>(string name)
        => Assert.Equal(
            NullabilityState.Nullable,
            new NullabilityInfoContext().Create(Property<T>(name)).ReadState);

    private static void AssertNotNullable<T>(string name)
        => Assert.Equal(
            NullabilityState.NotNull,
            new NullabilityInfoContext().Create(Property<T>(name)).ReadState);
}
