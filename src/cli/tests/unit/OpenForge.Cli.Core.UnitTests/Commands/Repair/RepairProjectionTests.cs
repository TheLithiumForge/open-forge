using System.Text.Json;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Presentation.Repair;
using OpenForge.Cli.Core.Presentation.Repair.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairProjectionTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair retains Cleanup priority beside remaining Library observations"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void LibraryObservationDoesNotReplaceRetainedArtifactCleanup()
    {
        var retained = new RepairFinding(RepairFindingCode.RecoveryArtifactRetained,
            "The prepared recovery artifact remains available for separate cleanup.");
        var selected = Select(
            RepairTestData.Result(
                findings: [retained],
                facts: RepairTestData.CompleteFacts(
                    findings: [retained],
                    postDiagnosis: PostDiagnosis(LibraryObservation()),
                    counts: Counts(remaining: 1, manual: 1))),
            CliDetail.Minimal);

        Assert.Equal("open-forge cleanup", selected.Report.Next?.Command);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair native JSON preserves the frozen envelope and minimal data contract"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void NativeJsonCarriesMinimalCatalogueData()
    {
        using var document = JsonDocument.Parse(Json(NoOpResult(), CliDetail.Minimal));
        var root = document.RootElement;

        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("repair", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal("Nothing to repair.", root.GetProperty("summary").GetProperty("headline").GetString());

        var data = root.GetProperty("data");
        Assert.Equal("dry-run", data.GetProperty("mode").GetString());
        Assert.Equal("relink", data.GetProperty("selection").GetString());
        Assert.Empty(data.GetProperty("repairs").EnumerateArray());
        Assert.Empty(data.GetProperty("remaining").EnumerateArray());
        Assert.Equal(0, root.GetProperty("counts").GetProperty("linksRepaired").GetInt32());
        Assert.Null(root.GetProperty("next").GetString());
        Assert.DoesNotContain("verified", Json(NoOpResult(), CliDetail.Minimal), StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair native JSON keeps deterministic envelope and data property order"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void NativeJsonPropertyOrderIsStable()
    {
        using var document = JsonDocument.Parse(Json(NoOpResult(), CliDetail.Full));

        Assert.Equal(
            ["schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next"],
            document.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["mode", "selection", "repairs", "remaining", "diagnosis", "verification"],
            document.RootElement.GetProperty("data").EnumerateObject().Select(property => property.Name));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair native text and JSON use one no-op headline and omit verification"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void NativeTextAndJsonShareNoOpSemantics()
    {
        var result = NoOpResult();
        var selected = Select(result, CliDetail.Minimal);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, RepairPresentation.Rendering.DataTextRenderer).Content;
        using var document = JsonDocument.Parse(Json(selected));

        Assert.Equal("Nothing to repair.\n", text);
        Assert.Equal("Nothing to repair.", document.RootElement.GetProperty("summary").GetProperty("headline").GetString());
        Assert.DoesNotContain("verified", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("verified", Json(selected), StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair native invalid output keeps the catalogue refusal wording"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void NativeInvalidOutputUsesFrozenWording()
    {
        var result = RepairTestData.Result(
            findings: [new RepairFinding(RepairFindingCode.InvalidInput, "Select a current link.")],
            mode: RepairMode.Apply,
            automatic: false,
            selectionMode: RepairSelectionMode.InteractivePrompt);
        var text = CliTextRenderer.Render(
            Select(result, CliDetail.Minimal),
            CliTextStyle.Plain,
            RepairPresentation.Rendering.DataTextRenderer).Content;

        Assert.Equal("Cannot repair: Select a current link.\nNext: open-forge repair --help\n", text);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair native output does not duplicate a visible unselected finding"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void NativeOutputDoesNotDuplicateVisibleUnselectedFinding()
    {
        var result = AmbiguousResult(visibleFinding: true);
        var minimal = Select(result, CliDetail.Minimal);
        var text = CliTextRenderer.Render(minimal, CliTextStyle.Plain, RepairPresentation.Rendering.DataTextRenderer).Content;

        Assert.Contains($"{RepairTestData.SourcePath}:1:13", text, StringComparison.Ordinal);
        Assert.Contains("Broken link; 2 possible targets", text, StringComparison.Ordinal);
        Assert.DoesNotContain("\"target.md\"; Broken link; 2 possible targets", text, StringComparison.Ordinal);
        Assert.Empty(minimal.Report.Effects);
        Assert.Empty(minimal.Report.Data.Repairs);

        using var minimalJson = JsonDocument.Parse(Json(minimal));
        var minimalRemaining = Assert.Single(minimalJson.RootElement.GetProperty("data").GetProperty("remaining").EnumerateArray());
        Assert.Equal(2, minimalRemaining.GetProperty("candidates").GetInt32());
        Assert.Empty(minimalJson.RootElement.GetProperty("effects").EnumerateArray());

        using var standardJson = JsonDocument.Parse(Json(result, CliDetail.Standard));
        var standardCandidates = Assert.Single(standardJson.RootElement.GetProperty("data").GetProperty("remaining").EnumerateArray())
            .GetProperty("candidates")
            .EnumerateArray()
            .Select(candidate => candidate.GetProperty("path").GetString())
            .ToArray();
        Assert.Equal([".agents/docs/target-a.md", ".agents/docs/target-b.md"], standardCandidates);
    }

    [Fact(DisplayName = "Repair native output names an unselected ambiguous link without a visible finding"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void NativeOutputNamesUnselectedLinkWithoutVisibleFinding()
    {
        var result = AmbiguousResult(visibleFinding: false);
        var selected = Select(result, CliDetail.Minimal);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, RepairPresentation.Rendering.DataTextRenderer).Content;

        Assert.Contains($"{RepairTestData.SourcePath}:1:13", text, StringComparison.Ordinal);
        Assert.Contains("\"target.md\"; Broken link; 2 possible targets", text, StringComparison.Ordinal);
        Assert.Empty(selected.Report.Effects);
    }

    [Theory(DisplayName = "Repair native output uses the reference step outcome for unapplied effects"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    [InlineData((int)RepairStepOutcome.Failed, "failed")]
    [InlineData((int)RepairStepOutcome.Interrupted, "final state unknown")]
    public void NativeOutputUsesReferenceStepOutcomeForUnappliedEffect(
        int outcome,
        string expectedWording)
    {
        var selected = RepairTestData.Selected();
        var effect = RepairTestData.Effect();
        var step = RepairTestData.Step(selected, effect: effect, outcome: (RepairStepOutcome)outcome);
        var plan = RepairTestData.Plan(selected, step);
        var result = RepairTestData.Result(
            facts: RepairTestData.CompleteFacts(selection: plan.Selection, plan: plan),
            mode: RepairMode.Apply,
            automatic: true);

        var text = CliTextRenderer.Render(
            Select(result, CliDetail.Minimal),
            CliTextStyle.Plain,
            RepairPresentation.Rendering.DataTextRenderer).Content;

        Assert.Contains($"{RepairTestData.SourcePath}:1:1", text, StringComparison.Ordinal);
        Assert.Contains(expectedWording, text, StringComparison.Ordinal);
        Assert.DoesNotContain("old -> new", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair projects a post-diagnosis Library observation with its exact cause and path"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void PostDiagnosisLibraryObservationKeepsExactFindingFacts()
    {
        const string path = ".agents/library-drift/library-note.md";
        const string cause = "The registered Library projection is not current.";
        var selected = Select(
            RepairTestData.Result(
                findings: [],
                facts: RepairTestData.CompleteFacts(
                    postDiagnosis: PostDiagnosis(LibraryObservation(path, "team-knowledge", cause)),
                    counts: Counts(remaining: 1, manual: 1))),
            CliDetail.Minimal);

        var finding = Assert.Single(selected.Report.Findings);
        Assert.Equal("repair.manual-finding-remaining", finding.Code);
        Assert.Equal("Library diagnosis remains", finding.Title);
        Assert.Equal(cause, finding.Message);
        Assert.Equal(CliSubjectKind.File, finding.Subject.Kind);
        Assert.Equal(path, finding.Subject.Path);
        Assert.Equal("team-knowledge", finding.Subject.Id);
        Assert.Single(selected.Report.Data.Remaining);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair omits a pre-diagnosis Library observation resolved by post-diagnosis"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void ResolvedPreDiagnosisLibraryObservationIsAbsent()
    {
        var preDiagnosis = LibraryObservation();
        var selected = Select(
            RepairTestData.Result(
                findings: [preDiagnosis],
                facts: RepairTestData.CompleteFacts(
                    findings: [preDiagnosis],
                    postDiagnosis: PostDiagnosis(),
                    counts: Counts(remaining: 0))),
            CliDetail.Minimal);

        Assert.Empty(selected.Report.Findings);
        Assert.Empty(selected.Report.Data.Remaining);
        Assert.Equal(CliSemanticStatus.Complete, selected.Report.Status);
        Assert.Null(selected.Report.Next);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair collapses duplicate post-diagnosis Library observations by typed identity"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void DuplicatePostDiagnosisLibraryObservationsCollapse()
    {
        var observation = LibraryObservation();
        var selected = Select(
            RepairTestData.Result(
                findings: [observation],
                facts: RepairTestData.CompleteFacts(
                    findings: [observation],
                    postDiagnosis: PostDiagnosis(observation, LibraryObservation()),
                    counts: Counts(remaining: 2, manual: 2))),
            CliDetail.Minimal);

        Assert.Single(selected.Report.Findings);
        Assert.Single(selected.Report.Data.Remaining);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair sends Library-only Attention to Doctor"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void LibraryOnlyAttentionUsesDoctorNextAction()
    {
        var selected = Select(
            RepairTestData.Result(
                findings: [],
                facts: RepairTestData.CompleteFacts(
                    postDiagnosis: PostDiagnosis(LibraryObservation()),
                    counts: Counts(remaining: 1, manual: 1))),
            CliDetail.Minimal);

        Assert.Equal("open-forge doctor", selected.Report.Next?.Command);
        Assert.Equal("Inspect remaining Library problems with open-forge doctor.", selected.Report.Next?.Reason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair keeps the Repair action and names Doctor for mixed remaining findings"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void MixedAttentionKeepsRepairAndPointsToDoctor()
    {
        var selected = Select(
            RepairTestData.Result(
                findings: [],
                facts: RepairTestData.CompleteFacts(
                    postDiagnosis: PostDiagnosis(LibraryObservation(), GuidedFinding()),
                    counts: Counts(remaining: 2, manual: 1, guided: 1))),
            CliDetail.Minimal);

        Assert.Equal("open-forge repair", selected.Report.Next?.Command);
        Assert.Equal(
            "Review remaining broken links in Repair, then inspect remaining Library problems with open-forge doctor.",
            selected.Report.Next?.Reason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair cancellation does not gain Library projection output"), Trait("Feature", "repair-presentation"), Trait("Evidence", "Unit")]
    public void InterruptedOutputHasNoExtraCancellationOrLibraryLine()
    {
        var result = RepairTestData.Result(
            findings: [new RepairFinding(RepairFindingCode.Interrupted, "Repair execution was interrupted.")],
            mode: RepairMode.Apply);
        var selected = Select(result, CliDetail.Minimal);
        var text = CliTextRenderer.Render(
            selected,
            CliTextStyle.Plain,
            RepairPresentation.Rendering.DataTextRenderer).Content;

        Assert.Equal("Repair was cancelled. Nothing was changed.\nNext: open-forge repair\n", text);
        Assert.DoesNotContain("Library", text, StringComparison.Ordinal);
    }

    private static RepairFinding LibraryObservation(
        string path = ".agents/library-drift/library-note.md",
        string identifier = "team-knowledge",
        string cause = "The registered Library projection is not current.")
        => new(
            RepairFindingCode.ManualFindingRemaining,
            cause,
            path)
        {
            Observation = new RepairDiagnosisObservation(path, identifier),
        };

    private static RepairFinding GuidedFinding()
        => new(
            RepairFindingCode.GuidedFindingRemaining,
            "A bounded guided local-reference finding remains.",
            RepairTestData.SourcePath,
            RepairTestData.Occurrence());

    private static RepairPostDiagnosis PostDiagnosis(params RepairFinding[] findings)
        => new(RepairPostDiagnosisState.Complete, RepairTestData.CompleteCoverage(), findings);

    private static RepairCounts Counts(int remaining, int manual = 0, int guided = 0)
        => new(
            selectedFindings: 0,
            unselectedFindings: remaining,
            repaired: 0,
            remaining: remaining,
            newFindings: remaining,
            manual: manual,
            guided: guided,
            blocked: 0,
            selectedEffects: 0,
            appliedEffects: 0,
            verifiedEffects: 0,
            noOps: 0,
            conflicts: 0);

    private static RepairResult AmbiguousResult(bool visibleFinding)
    {
        var occurrence = RepairTestData.Occurrence(byteOffset: 12, byteLength: 9);
        var candidates = new RepairCandidateSet(
        [
            new RepairCandidate(
                RepairTestData.Target(".agents/docs/target-a.md"),
                [new RepairCandidateEvidence(RepairCandidateEvidenceKind.Filename, "target-a.md", null)]),
            new RepairCandidate(
                RepairTestData.Target(".agents/docs/target-b.md"),
                [new RepairCandidateEvidence(RepairCandidateEvidenceKind.Filename, "target-b.md", null)]),
        ]);
        var proposal = new RepairProposal(
            RepairCatalogueMember.MissingTargetRelink,
            RepairTestData.SourcePath,
            occurrence,
            "target.md",
            candidates);
        var selection = new RepairSelection(
            RepairSelectionMode.Automatic,
            [],
            [proposal],
            RepairLibrarySelection.Empty);
        var plan = new RepairPlan(
            RepairTestData.Request(mode: RepairMode.Apply, automatic: true),
            selection,
            [],
            [],
            []);
        var finding = new RepairFinding(
            RepairFindingCode.GuidedFindingRemaining,
            "A current Repair proposal remains unselected.",
            RepairTestData.SourcePath,
            occurrence);
        var facts = RepairTestData.CompleteFacts(
            selection: selection,
            plan: plan,
            affectedPaths: [RepairTestData.SourcePath],
            counts: new RepairCounts(
                selectedFindings: 0,
                unselectedFindings: 1,
                repaired: 0,
                remaining: 1,
                newFindings: 1,
                manual: 0,
                guided: 1,
                blocked: 0,
                selectedEffects: 0,
                appliedEffects: 0,
                verifiedEffects: 0,
                noOps: 0,
                conflicts: 0));
        var result = RepairTestData.Result(
            findings: visibleFinding
                ? new[] { finding }
                : Array.Empty<RepairFinding>(),
            facts: facts with
            {
                PostDiagnosis = visibleFinding
                    ? RepairPostDiagnosis.NotRequested
                    : new RepairPostDiagnosis(
                        RepairPostDiagnosisState.Complete,
                        RepairTestData.CompleteCoverage(),
                        [finding]),
            },
            mode: RepairMode.Apply,
            automatic: true,
            selectionMode: RepairSelectionMode.Automatic);
        return result;
    }

    private static RepairResult NoOpResult()
    {
        var relink = RepairTestData.Relink(expectedDestination: "old");
        var selected = RepairTestData.Selected();
        var selection = RepairTestData.Selection(selected, RepairSelectionMode.AutomaticAndExplicit);
        var request = RepairTestData.Request(automatic: true, relinks: [relink]);
        var step = RepairTestData.Step(selected, noOp: RepairTestData.NoOp(), outcome: RepairStepOutcome.NoOp);
        var plan = new RepairPlan(request, selection, [step], [], []);
        var facts = RepairTestData.CompleteFacts(
            selection: selection,
            plan: plan,
            affectedPaths: [RepairTestData.SourcePath],
            counts: new RepairCounts(
                selectedFindings: 1,
                unselectedFindings: 0,
                repaired: 0,
                remaining: 0,
                newFindings: 0,
                manual: 0,
                guided: 0,
                blocked: 0,
                selectedEffects: 0,
                appliedEffects: 0,
                verifiedEffects: 0,
                noOps: 1,
                conflicts: 0));
        return RepairTestData.Result(
            facts: facts,
            automatic: true,
            selectionMode: RepairSelectionMode.AutomaticAndExplicit,
            relinks: [relink]);
    }

    private static CliSelectedReport<RepairData> Select(RepairResult result, CliDetail detail)
    {
        var selection = new CliSelection(detail, null);
        var rendering = RepairPresentation.Rendering;
        return rendering.SelectText!(CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape));
    }

    private static string Json(RepairResult result, CliDetail detail)
        => Json(Select(result, detail));

    private static string Json(CliSelectedReport<RepairData> selected)
        => CliJsonRenderer.Render(selected, RepairPresentation.Rendering.DataJsonTypeInfo);
}
