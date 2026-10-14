using System.Text.Json;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Repair;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairRenameProjectionTests
{
    [Theory(DisplayName = "Listed Repair choices precede remaining Index work in preview and application")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ListedChoicesPrecedeIndex(bool automatic, bool preview)
    {
        var reason = new RepairCandidateEvidence(RepairCandidateEvidenceKind.LiteralContent, "Rounding", null);
        var candidates = new RepairCandidateSet(new[] { "first.md", "second.md" }.Select(name =>
            new RepairCandidate(RepairTestData.Target($".agents/docs/{name}", provenance: [reason]), [reason])));
        var proposal = new RepairProposal(RepairCatalogueMember.MissingTargetRelink, RepairTestData.SourcePath, RepairTestData.Occurrence(), "missing.md", candidates);
        var finding = new RepairFinding(RepairFindingCode.GuidedFindingRemaining, "Choose its new location.", proposal.SourceCanonicalPath, proposal.Occurrence);
        var mode = automatic ? RepairSelectionMode.Automatic : RepairSelectionMode.NonInteractiveBlocked;
        var findings = automatic ? new[] { finding } : [new RepairFinding(RepairFindingCode.SelectionRequired, "This session cannot ask.")];
        var selection = new RepairSelection(mode, [], [proposal], RepairLibrarySelection.Empty);
        var plan = new RepairPlan(RepairTestData.Request(preview ? RepairMode.DryRun : RepairMode.Apply, automatic), selection, [], [], []);
        var post = new RepairPostDiagnosis(RepairPostDiagnosisState.Complete, RepairTestData.CompleteCoverage(), [finding])
        { NeedsIndex = true, Proposals = [proposal] };
        var result = RepairTestData.Result(findings: findings, mode: preview ? RepairMode.DryRun : RepairMode.Apply, automatic: automatic, selectionMode: mode,
            facts: RepairTestData.CompleteFacts(selection: selection, plan: plan, postDiagnosis: post, findings: findings,
                counts: new(selectedFindings: 0, unselectedFindings: 1, repaired: 0, remaining: 1, newFindings: 0, manual: 0, guided: 1,
                    blocked: 0, selectedEffects: 0, appliedEffects: 0, verifiedEffects: 0, noOps: 0, conflicts: 0)));
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var report = RepairPresentation.Rendering.Selector(result, new(detail));
            var guided = Assert.Single(report.Findings, row => row.Code == "repair.guided-finding-remaining");
            Assert.Equal(2, guided.TextChoices.Count);
            Assert.Equal(guided.TextChoices.Select(choice => choice.Command), guided.Actions.Select(action => action.Command));
            var trimmed = CliReportTrimmer.Trim(report, new(detail), CliCommandShape.ChangeReport);
            using var json = JsonDocument.Parse(CliJsonRenderer.Render(trimmed, RepairPresentation.Rendering.DataJsonTypeInfo));
            var findingJson = Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray(), row => row.GetProperty("code").GetString() == guided.Code);
            Assert.Equal(guided.TextChoices.Select(choice => choice.Command), findingJson.GetProperty("actions").EnumerateArray().Select(action => action.GetProperty("command").GetString()));
            var text = CliTextRenderer.Render(RepairPresentation.Rendering.SelectText!(trimmed), CliTextStyle.Plain, RepairPresentation.Rendering.DataTextRenderer).Content;
            Assert.Equal(2, text.Split("open-forge repair --relink", StringSplitOptions.None).Length - 1);
            Assert.Equal(CliNextActionKind.Sentence, report.Next?.Kind);
            Assert.Equal("Choose a target above, then run its command.", report.Next?.Command);
        }
    }

    [Theory, InlineData(0), InlineData(1), InlineData(2), Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ProofVocabularyAndAdditiveWireOrderMatchAtEveryDetail(int basisValue)
    {
        var result = Result((RenameEvidenceBasis)basisValue, RepairMode.DryRun, RepairStepOutcome.Planned);
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var selected = RepairPresentation.Rendering.SelectText!(CliReportTrimmer.Trim(RepairPresentation.Rendering.Selector(result, new(detail)), new(detail), CliCommandShape.ChangeReport));
            var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, RepairPresentation.Rendering.DataTextRenderer).Content;
            var expected = basisValue switch { 0 => "content is 94% unchanged", 1 => "same content", _ => "recorded description and tags match" };
            var prefix = detail >= CliDetail.Full ? ".agents/guidance/" : "";
            Assert.Contains($"{prefix}billing/rounding.md was renamed to {prefix}billing/money-rounding.md, {expected}", string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)));
            Assert.Equal(1, text.Split("was renamed to", StringSplitOptions.None).Length - 1);
            using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, RepairPresentation.Rendering.DataJsonTypeInfo));
            var data = json.RootElement.GetProperty("data");
            var rename = Assert.Single(data.GetProperty("renames").EnumerateArray());
            Assert.Equal(".agents/guidance/billing/rounding.md", rename.GetProperty("oldPath").GetString());
            Assert.Equal(".agents/guidance/billing/money-rounding.md", rename.GetProperty("newPath").GetString());
            Assert.Equal(detail >= CliDetail.Full, rename.TryGetProperty("proof", out var proof));
            if (detail >= CliDetail.Full)
            {
                Assert.Equal(["head", "oldBlob", "oldHash", "newHash", "metadata"], proof.EnumerateObject().Select(property => property.Name));
                Assert.Contains(json.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "repair.rename-proven");
            }
            Assert.Equal(["linksRepaired", "problemsRemaining", "problemsNeedingChoice", "problemsNeedingHand", "librarySteps", "infos", "entriesUpdated", "metadataCompleted", "entrypointsCreated", "filesSkipped"],
                json.RootElement.GetProperty("counts").EnumerateObject().Select(property => property.Name));
        }
    }

    [Theory, InlineData((int)RepairStepOutcome.Blocked), InlineData((int)RepairStepOutcome.Failed), InlineData((int)RepairStepOutcome.Interrupted), Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void UnverifiedApplicationNeverClaimsProof(int outcome)
    {
        var result = Result(RenameEvidenceBasis.CommittedContent, RepairMode.Apply, (RepairStepOutcome)outcome);
        var selected = CliReportTrimmer.Trim(RepairPresentation.Rendering.Selector(result, new(CliDetail.Full)), new(CliDetail.Full), CliCommandShape.ChangeReport);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, RepairPresentation.Rendering.DataTextRenderer).Content;
        Assert.Empty(selected.Report.Data.Renames);
        Assert.DoesNotContain("was renamed to", text);
        Assert.DoesNotContain(selected.Report.Findings, finding => finding.Code == "repair.rename-proven");
    }

    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void SeverityFilterCannotRevealHiddenProofRows()
    {
        var result = Result(RenameEvidenceBasis.CommittedContent, RepairMode.DryRun, RepairStepOutcome.Planned);
        var selection = new CliSelection(CliDetail.Minimal, new HashSet<CliSeverity> { CliSeverity.Error });
        var selected = CliReportTrimmer.Trim(RepairPresentation.Rendering.Selector(result, selection), selection, CliCommandShape.ChangeReport);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, RepairPresentation.Rendering.DataTextRenderer).Content;
        Assert.DoesNotContain("was renamed to", text);
    }

    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ProofIsAttachedToItsLinkAtEveryDetail()
    {
        var result = Result(RenameEvidenceBasis.CommittedContent, RepairMode.DryRun, RepairStepOutcome.Planned);
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var rows = RepairPresentation.Rendering.Selector(result, new(detail)).Data.TextRows;
            Assert.Contains(" -> ", rows[0].Wording);
            Assert.Contains("was renamed to", rows[1].Wording);
        }
    }

    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void MalformedSkipUsesIndexCodeAndExactEditText()
    {
        var finding = new RepairFinding(RepairFindingCode.MetadataSkipped, "Malformed metadata.", ".agents/guidance/broken.md");
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "repair workspace"));
        var result = RepairTestData.Result(findings: [finding], mode: RepairMode.Apply,
            workspace: new(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace));
        var report = RepairPresentation.Rendering.Selector(result, new(CliDetail.Standard));
        var projected = Assert.Single(report.Findings);
        Assert.Equal("index.metadata-skipped", projected.Code);
        Assert.Equal(CliSeverity.Error, projected.Severity);
        Assert.Equal($".agents/guidance/broken.md has invalid frontmatter. Fix its frontmatter, then run open-forge index --workspace '{root}'.", projected.Message);
        Assert.Equal(CliResolution.ManualDecision, projected.Resolution);
    }

    private static RepairResult Result(RenameEvidenceBasis basis, RepairMode mode, RepairStepOutcome outcome)
    {
        const string oldPath = ".agents/guidance/billing/rounding.md";
        const string newPath = ".agents/guidance/billing/money-rounding.md";
        var candidate = RepairTestData.FileState("# Rounding\n\nKeep monetary rules.\n", newPath);
        var proof = new RenameEvidenceObservation
        {
            OldPath = oldPath,
            NewPath = newPath,
            Basis = basis,
            Head = null,
            OldBlob = null,
            OldHash = candidate.ContentHash,
            NewHash = candidate.ContentHash!,
            Similarity = basis == RenameEvidenceBasis.GitRename ? 94 : null,
            SameContent = basis == RenameEvidenceBasis.CommittedContent,
            Metadata = null,
            CandidateSnapshot = candidate,
            MissingOldExpectation = FileExpectation.Missing(Path.Combine(RepairTestData.WorkspaceRoot, oldPath)),
            RecordExpectation = null,
        };
        var target = RepairTestData.Target(newPath) with { RenameProof = proof };
        var proposal = new RepairProposal(RepairCatalogueMember.MissingTargetRelink, RepairTestData.SourcePath, RepairTestData.Occurrence(), "old", "new", target) { RenameProof = proof };
        var selected = new RepairSelectedProposal(proposal, proposal.Resolution!, [RepairSelectionOrigin.Automatic]);
        var step = RepairTestData.Step(selected, effect: outcome == RepairStepOutcome.Blocked ? null : RepairTestData.Effect(), outcome: outcome);
        var plan = RepairTestData.Plan(selected, step, RepairTestData.Request(mode));
        var findings = mode == RepairMode.DryRun || outcome == RepairStepOutcome.Verified
            ? new[] { new RepairFinding(RepairFindingCode.RenameProven, "Identity proven.", oldPath) } : [];
        return RepairTestData.Result(findings: findings, facts: RepairTestData.CompleteFacts(selection: plan.Selection, plan: plan, findings: findings), mode: mode);
    }
}
