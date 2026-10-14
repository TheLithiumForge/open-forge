using System.Collections.Immutable;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Commands.Repair.Shared.Result;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Presentation.Repair;
using OpenForge.Cli.Core.Presentation.Repair.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class LibraryRepairProjectionTests
{
    [Theory(DisplayName = "Library recovery source counts and JSON count each remaining link once")]
    [InlineData(1), InlineData(20)]
    [Trait("Boundary", "Output"), Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void LibraryCountsDoNotDoubleCountDiagnosisAndSelection(int linkCount)
    {
        var proposals = Enumerable.Range(0, linkCount)
            .Select(index => new Core.Commands.Repair.Models.Selection.RepairLibraryRecoveryProposal(
                LibraryRepairData.Evidence(Core.Framework.Recovery.Models.Entries.RecoveryEntryKind.RelativeFileLinkDelete,
                    targetPath: $".agents/directives/item-{index}.md")))
            .ToImmutableArray();
        var input = LibraryRepairData.Input(proposals[0].Evidence) with
        {
            Request = new(proposals[0].Evidence.Entry.Input.Context.Workspace, RepairMode.DryRun, true, [], false),
            Libraries = proposals,
        };
        var plan = RepairLibraryRecoveryPlanner.Build(input);
        var observations = proposals.SelectMany(proposal => new[] { "The projection is missing.", "The Library residual needs recovery." }
            .Select(cause => new RepairFinding(Core.Commands.Repair.RepairFindingCode.ManualFindingRemaining, cause)
            {
                Observation = new(proposal.TargetPath, proposal.LibraryIdValue),
            })).ToArray();
        var result = RepairResultBuilder.Build(new RepairResultInput
        {
            Request = input.Request,
            Plan = plan,
            Diagnosis = RepairTestData.CompleteCoverage(),
            Findings = observations,
            InitialFindings = observations,
            Preflight = new(RepairPreflightState.Ready, cause: null, RepairRecoveryState.NotRequired),
            Application = RepairApplication.NotRequested,
            Verification = RepairVerification.NotRequested,
            Recovery = RepairRecovery.NotRequired,
            PostDiagnosis = RepairPostDiagnosis.NotRequested,
        });
        var expected = linkCount - 1;
        Assert.Equal(expected, result.Counts.Remaining);
        Assert.Equal(expected, result.Counts.Manual);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.All(result.Findings, finding => Assert.Contains("The Library residual needs recovery.", finding.Cause, StringComparison.Ordinal));
        var selected = Select(result, CliDetail.Standard);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, RepairPresentation.Rendering.DataTextRenderer).Content;
        Assert.Contains("Would restore 1 Library link.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("39", text, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(CliJsonRenderer.Render(selected, RepairPresentation.Rendering.DataJsonTypeInfo));
        var counts = document.RootElement.GetProperty("counts");
        Assert.Equal(expected, counts.GetProperty("problemsRemaining").GetInt32());
        Assert.Equal(expected, counts.GetProperty("problemsNeedingHand").GetInt32());
        Assert.Equal(expected, counts.TryGetProperty("warnings", out var warnings) ? warnings.GetInt32() : 0);
        Assert.Equal(expected, document.RootElement.GetProperty("findings").GetArrayLength());
        CommandOutputSnapshot.MatchSnapshot(counts.GetRawText(), $"library-counts.{linkCount}.json");
    }
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair native Library output retains the selected recovery identity and effect"), Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void NativeLibraryOutputRetainsSelectedIdentity()
    {
        var evidence = LibraryRepairData.Evidence();
        var plan = LibraryRepairData.Plan(evidence);
        var result = RepairTestData.Result(
            facts: RepairTestData.CompleteFacts(selection: plan.Selection, plan: plan),
            mode: RepairMode.Apply,
            automatic: true);

        var selected = Select(result, CliDetail.Standard);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, RepairPresentation.Rendering.DataTextRenderer).Content;
        using var document = JsonDocument.Parse(CliJsonRenderer.Render(selected, RepairPresentation.Rendering.DataJsonTypeInfo));
        var data = document.RootElement.GetProperty("data");
        var recovery = Assert.Single(data.GetProperty("libraryRecovery").EnumerateArray());

        Assert.Contains(evidence.Entry.Input.Context.Entry.TargetPath.ToString(), text, StringComparison.Ordinal);
        Assert.Contains("restored from recovery", text, StringComparison.Ordinal);
        Assert.Equal("team-knowledge", recovery.GetProperty("id").GetString());
        Assert.Equal(evidence.Entry.Input.Context.Entry.TargetPath.ToString(), recovery.GetProperty("path").GetString());
        Assert.True(recovery.GetProperty("selected").GetBoolean());
        Assert.Equal(evidence.Entry.Input.Context.Entry.TargetPath.ToString(),
            Assert.Single(document.RootElement.GetProperty("effects").EnumerateArray()).GetProperty("path").GetString());
    }

    [Fact(DisplayName = "Repair native Library output names a selected blocked residual without a choice row"), Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void NativeLibraryOutputRetainsSelectedBlockedResidualPath()
    {
        var evidence = LibraryRepairData.Evidence(comparison: RecoveryBundleTargetComparisonState.Third);
        var plan = RepairLibraryRecoveryPlanner.Build(LibraryRepairData.Input(evidence));
        var result = RepairTestData.Result(
            facts: RepairTestData.CompleteFacts(selection: plan.Selection, plan: plan),
            mode: RepairMode.Apply,
            automatic: true);

        var selected = Select(result, CliDetail.Standard);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, RepairPresentation.Rendering.DataTextRenderer).Content;
        using var document = JsonDocument.Parse(CliJsonRenderer.Render(selected, RepairPresentation.Rendering.DataJsonTypeInfo));
        var data = document.RootElement.GetProperty("data");

        Assert.Contains(evidence.Entry.Input.Context.Entry.TargetPath.ToString(), text, StringComparison.Ordinal);
        Assert.Contains("not started", text, StringComparison.Ordinal);
        Assert.DoesNotContain("choice", text, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.True(Assert.Single(data.GetProperty("libraryRecovery").EnumerateArray()).GetProperty("selected").GetBoolean());
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Repair native Library data keeps each recovery entry target"), Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("OrdinaryCreate", ".agents/open-forge.libraries.json")]
    [InlineData("OrdinaryReplace", ".agents/open-forge.libraries.json")]
    [InlineData("OrdinaryReplaceGeneratedRegion", ".agents/directives/_directives.md")]
    [InlineData("OrdinaryDelete", ".agents/open-forge.libraries.json")]
    [InlineData("RelativeFileLinkCreate", ".agents/directives/review.md")]
    [InlineData("RelativeFileLinkDelete", ".agents/directives/review.md")]
    public void NativeLibraryDataRetainsTargetPath(string kind, string expectedPath)
    {
        var plan = LibraryRepairData.Plan(LibraryRepairData.Evidence(Enum.Parse<OpenForge.Cli.Core.Framework.Recovery.Models.Entries.RecoveryEntryKind>(kind)));
        var result = RepairTestData.Result(
            facts: RepairTestData.CompleteFacts(selection: plan.Selection, plan: plan),
            mode: RepairMode.Apply,
            automatic: true);
        var selected = Select(result, CliDetail.Standard);
        using var document = JsonDocument.Parse(CliJsonRenderer.Render(selected, RepairPresentation.Rendering.DataJsonTypeInfo));

        Assert.Equal(expectedPath, Assert.Single(document.RootElement.GetProperty("data").GetProperty("libraryRecovery").EnumerateArray()).GetProperty("path").GetString());
    }

    private static CliSelectedReport<RepairData> Select(RepairResult result, CliDetail detail)
    {
        var selection = new CliSelection(detail, null);
        var rendering = RepairPresentation.Rendering;
        return rendering.SelectText!(CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape));
    }
}
