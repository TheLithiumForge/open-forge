using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.UnitTests.Commands.Index.Shared;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared.Result;

public sealed class IndexResultTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index result derives exact status precedence ordering and next action from typed findings"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ResultDerivesStatusOrderingAndNextAction()
    {
        var findings = new[]
        {
            new IndexFinding(
                IndexFindingCode.SourceAmbiguous,
                sourceOccurrence: 2,
                source: null,
                cause: "second",
                candidates: []),
            new IndexFinding(
                IndexFindingCode.SourceAmbiguous,
                sourceOccurrence: null,
                source: null,
                cause: "first",
                candidates: []),
            IndexTestData.Finding(IndexFindingCode.Interrupted),
            IndexTestData.Finding(IndexFindingCode.OperationFailed),
        };

        var result = IndexTestData.Result(findings: findings);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(
            [null, 2, null, null],
            result.Findings.Select(finding => finding.SourceOccurrence));
        Assert.Equal(
            [IndexFindingCode.SourceAmbiguous, IndexFindingCode.SourceAmbiguous, IndexFindingCode.OperationFailed, IndexFindingCode.Interrupted],
            result.Findings.Select(finding => finding.Code));
        Assert.Equal("open-forge index --detail debug", Assert.IsType<CliNextAction>(result.Next).Command);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index result derives the complete status precedence and exact next-action matrix"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ResultDerivesCompleteStatusAndNextMatrix()
    {
        var cases = new[]
        {
            Case(IndexTestData.Result(), CliSemanticStatus.Complete, null, null),
            Case(
                IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.InvalidInput), IndexTestData.Finding(IndexFindingCode.WorkspaceUnavailable)]),
                CliSemanticStatus.Invalid,
                "open-forge index --help",
                "Correct the named Index input, then rerun the request."),
            Case(
                IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.SourceAmbiguous)]),
                CliSemanticStatus.Blocked,
                "open-forge index",
                "Replace every ambiguous source with one listed exact path, then rerun the same Index request."),
            Case(
                IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.WorkspaceLockUnavailable), IndexTestData.Finding(IndexFindingCode.DiscoveryIncomplete)]),
                CliSemanticStatus.Blocked,
                "open-forge index",
                "Wait for the workspace lock to become available or inspect lock availability, then rerun Index from a fresh plan."),
            Case(
                IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.TargetChanged)]),
                CliSemanticStatus.Blocked,
                "open-forge index",
                "Inspect the changed target, then rerun Index from a fresh plan."),
            Case(
                IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.TargetUnsafe), IndexTestData.Finding(IndexFindingCode.MetadataIncomplete)]),
                CliSemanticStatus.Blocked,
                "open-forge doctor",
                "Inspect the blocked workspace, topology, metadata, generated-region, or recovery boundary before rerunning Index."),
            Case(
                IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.DiscoveryIncomplete)]),
                CliSemanticStatus.Incomplete,
                "open-forge doctor",
                "Inspect the unavailable discovery, metadata, projection, or recovery facts before relying on this Index result."),
            Case(
                IndexTestData.Result(
                    regions: [IndexTestData.Update(IndexRegionOutcome.Verified)],
                    findings: [IndexTestData.Finding(IndexFindingCode.RecoveryArtifactRetained)],
                    recovery: new IndexRecovery(IndexRecoveryState.Retained, IndexTestData.RecoveryPath("attention.zip"))),
                CliSemanticStatus.Attention,
                "open-forge cleanup",
                "Review and remove the reported recovery artifact after confirming the verified Index result."),
            Case(
                IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.OperationFailed), IndexTestData.Finding(IndexFindingCode.Interrupted)]),
                CliSemanticStatus.Failed,
                "open-forge index --detail debug",
                "Report the failure and retry the same Index request with bounded diagnostics."),
            Case(
                IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.Interrupted), IndexTestData.Finding(IndexFindingCode.InvalidInput)]),
                CliSemanticStatus.Interrupted,
                "open-forge index",
                "Rerun the same Index request."),
        };

        foreach (var evidence in cases)
        {
            Assert.Equal(evidence.Status, evidence.Result.Status);
            Assert.Equal(evidence.Command, evidence.Result.Next?.Command);
            Assert.Equal(evidence.Reason, evidence.Result.Next?.Reason);
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index result preserves optional warnings and validates an independently updated subset")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void OptionalAndSkippedMetadataHaveDistinctResultContracts()
    {
        var optional = IndexTestData.Result(
            findings: [IndexTestData.Finding(IndexFindingCode.MetadataOptional)]);

        Assert.Equal(CliSemanticStatus.Attention, optional.Status);
        Assert.Null(optional.Next);
        Assert.Equal(IndexRecoveryState.NotRequired, optional.Recovery.State);

        var preview = IndexTestData.Result(
            regions: [IndexTestData.Update(IndexRegionOutcome.NotRequested)],
            findings: [IndexTestData.Finding(IndexFindingCode.MetadataOptional)],
            mode: IndexMode.DryRun);

        Assert.Equal(CliSemanticStatus.Attention, preview.Status);
        Assert.Equal(IndexRegionOutcome.NotRequested, Assert.Single(preview.Regions).Outcome);
        Assert.Equal(IndexRecoveryState.NotRequired, preview.Recovery.State);

        var safe = IndexTestData.Source("alpha", ".agents/alpha/_alpha.md");
        var skipped = IndexTestData.Source("beta", ".agents/beta/_beta.md");
        var skippedFinding = new IndexFinding(
            IndexFindingCode.MetadataSkipped,
            sourceOccurrence: null,
            source: skipped,
            cause: "Malformed authored metadata was skipped.",
            candidates: [])
        {
            Details = new IndexFindingDetails
            {
                ParentPath = skipped.Path,
                MetadataProblem = IndexMetadataProblem.Invalid,
            },
        };
        var partial = IndexTestData.Result(
            regions:
            [
                IndexRegion.Update(
                    safe,
                    new IndexRegionUpdate
                    {
                        BeforeEntryCount = 1,
                        ExpectedEntryCount = 1,
                        Change = new IndexChange("old\n", "new\n"),
                        Outcome = IndexRegionOutcome.Verified,
                    }),
                IndexRegion.NotEstablished(skipped),
            ],
            findings: [skippedFinding],
            recovery: new IndexRecovery(IndexRecoveryState.Removed, null));

        Assert.Equal(CliSemanticStatus.Incomplete, partial.Status);
        Assert.Equal(1, partial.Counts.Updates);
        Assert.Equal(IndexRegionOutcome.Verified, partial.Regions[0].Outcome);
        Assert.Equal(IndexRegionOutcome.NotEstablished, partial.Regions[1].Outcome);
        Assert.Equal(IndexRecoveryState.Removed, partial.Recovery.State);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index result derives exact region counts and prevents incoherent recovery facts"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ResultCountsAndRecoveryCoherenceAreEnforced()
    {
        var unchanged = IndexRegion.Unchanged(IndexTestData.Source("loader", SourceLogicalPath.LoaderPath), 2, 2);
        var applied = IndexTestData.Update(IndexRegionOutcome.Applied);
        var verified = IndexRegion.Update(
            IndexTestData.Source("skills", ".agents/skills/_skills.md"),
            new IndexRegionUpdate
            {
                BeforeEntryCount = 1,
                ExpectedEntryCount = 2,
                Change = new IndexChange("old\n", "one\ntwo\n"),
                Outcome = IndexRegionOutcome.Verified,
            });
        var result = IndexTestData.Result(
            regions: [unchanged, applied, verified],
            findings: [IndexTestData.Finding(IndexFindingCode.WriteFailed)],
            recovery: new IndexRecovery(IndexRecoveryState.Retained, IndexTestData.RecoveryPath("failed.zip")));

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal((3, 2, 1, 2, 2), (
            result.Counts.Regions,
            result.Counts.Updates,
            result.Counts.Unchanged,
            result.Counts.Applied,
            result.Counts.Verified));
        Assert.DoesNotContain(result.Findings, finding => finding.Code == IndexFindingCode.RecoveryArtifactRetained);

        var interrupted = IndexTestData.Result(
            regions: [IndexTestData.Update(IndexRegionOutcome.Applied)],
            findings: [IndexTestData.Finding(IndexFindingCode.Interrupted)],
            recovery: new IndexRecovery(IndexRecoveryState.Retained, IndexTestData.RecoveryPath("interrupted.zip")));
        Assert.Equal(CliSemanticStatus.Interrupted, interrupted.Status);
        Assert.Equal(IndexRecoveryState.Retained, interrupted.Recovery.State);

        var unknownBefore = IndexTestData.Update(
            outcome: IndexRegionOutcome.Verified,
            beforeEntryCount: null);
        Assert.Null(unknownBefore.BeforeEntryCount);
        Assert.Equal(1, unknownBefore.ExpectedEntryCount);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index result rejects impossible complete dry-run attention recovery and source identity combinations"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ResultRejectsImpossibleGlobalCombinations()
    {
        Assert.Throws<ArgumentException>(() => IndexTestData.Result(
            regions: [IndexTestData.Update(IndexRegionOutcome.Applied)],
            recovery: new IndexRecovery(IndexRecoveryState.Removed, null)));
        Assert.Throws<ArgumentException>(() => IndexTestData.Result(
            regions: [IndexTestData.Update(IndexRegionOutcome.Verified)],
            recovery: new IndexRecovery(IndexRecoveryState.Removed, null),
            mode: IndexMode.DryRun));
        Assert.Throws<ArgumentException>(() => IndexTestData.Result(
            regions: [IndexTestData.Update(IndexRegionOutcome.Verified)],
            findings: [IndexTestData.Finding(IndexFindingCode.RecoveryArtifactRetained)],
            recovery: new IndexRecovery(IndexRecoveryState.Retained, IndexTestData.RecoveryPath("invalid-dry-run.zip")),
            mode: IndexMode.DryRun));
        Assert.Throws<ArgumentException>(() => new IndexLogicalSource(
            "wrong-id",
            ".agents/memory/_memory.md",
            IndexLogicalSourceScope.Rooted));
        Assert.Throws<ArgumentException>(() => new IndexRecovery(
            IndexRecoveryState.Retained,
            "relative/recovery.zip"));
        Assert.Throws<ArgumentException>(() => new IndexSelection(
            IndexSelectionOrigin.AutomaticLoader,
            IndexSelectionScope.Rooted,
            [IndexTestData.Source()]));
        Assert.Throws<ArgumentException>(() => new IndexResultBuilder().Create(
            new IndexResultFormation
            {
                Workspace = IndexTestData.Workspace(),
                Mode = IndexMode.Apply,
                Selection = IndexSelection.NotEstablished(IndexSelectionOrigin.ExplicitSources),
                Regions = [],
                Recovery = IndexRecovery.NotRequired,
                Findings = [],
            }));

        var automatic = new IndexSelection(
            IndexSelectionOrigin.AutomaticLoader,
            IndexSelectionScope.Rooted,
            [IndexTestData.Source("loader", SourceLogicalPath.LoaderPath)]);
        Assert.Equal(SourceLogicalPath.LoaderPath, Assert.Single(automatic.Sources).Path);
        var attention = IndexTestData.Result(
            regions: [IndexTestData.Update(IndexRegionOutcome.Verified)],
            findings: [IndexTestData.Finding(IndexFindingCode.RecoveryArtifactRetained)],
            recovery: new IndexRecovery(IndexRecoveryState.Retained, IndexTestData.RecoveryPath("valid-attention.zip")));
        Assert.Equal(CliSemanticStatus.Attention, attention.Status);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index result rejects null finding members with an intentional argument error"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ResultRejectsNullFindingMembersExplicitly()
    {
        var loader = IndexTestData.Source("loader", SourceLogicalPath.LoaderPath);
        var exception = Assert.Throws<ArgumentException>(() => new IndexResultBuilder().Create(
            new IndexResultFormation
            {
                Workspace = IndexTestData.Workspace(),
                Mode = IndexMode.Apply,
                Selection = IndexTestData.Selection(loader),
                Regions = [IndexRegion.Unchanged(loader, 0, 0)],
                Recovery = IndexRecovery.NotRequired,
                Findings = new IndexFinding[1],
            }));

        Assert.Contains("cannot contain null", exception.Message, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index value models reject undefined finite states at their invariant boundaries"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ValueModelsRejectUndefinedFiniteStates()
    {
        var source = IndexTestData.Source();

        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexLogicalSource(
            source.Id,
            source.Path,
            (IndexLogicalSourceScope)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexSelection.NotEstablished((IndexSelectionOrigin)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexSelection(
            IndexSelectionOrigin.ExplicitSources,
            (IndexSelectionScope)int.MaxValue,
            [source]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexRegion.Unchanged(source, 0, 0).WithOutcome((IndexRegionOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IndexRecovery((IndexRecoveryState)int.MaxValue, residualPath: null));
    }

    private static NextCase Case(
        IndexResult result,
        CliSemanticStatus status,
        string? command,
        string? reason)
        => new(result, status, command, reason);

    private sealed record NextCase(
        IndexResult Result,
        CliSemanticStatus Status,
        string? Command,
        string? Reason);
}
