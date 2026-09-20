using System.Text;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Projection;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Operation;
using OpenForge.Cli.Core.Commands.Index.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared.Operation;

public sealed class IndexMutationMapperTests
{
    private const string RootPath = ".agents/root/_root.md";
    private const string Prefix = "# Root\n";
    private const string Suffix = "Footer\n";
    private const string BeforeBody = "before\n";
    private const string ExpectedBody = "expected\n";

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index operation events use bounded command-authored causes"),
     Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void OperationEventCausesNeverExposeRawFilesystemFailures()
    {
        var codes = new[]
        {
            IndexFindingCode.WorkspaceUnsafe,
            IndexFindingCode.WorkspaceLockUnavailable,
            IndexFindingCode.TargetChanged,
            IndexFindingCode.TargetUnsafe,
            IndexFindingCode.ProjectionIncomplete,
            IndexFindingCode.RecoveryUnavailable,
            IndexFindingCode.RecoveryConflict,
            IndexFindingCode.TargetChangedDuringApply,
            IndexFindingCode.WriteFailed,
            IndexFindingCode.VerificationFailed,
            IndexFindingCode.RecoveryFailed,
            IndexFindingCode.OperationFailed,
            IndexFindingCode.Interrupted,
            IndexFindingCode.RecoveryArtifactRetained,
        };
        foreach (var code in codes)
        {
            var finding = IndexMutationMapper.CreateFinding(code);

            Assert.Equal(code, finding.Code);
            Assert.False(string.IsNullOrWhiteSpace(finding.Cause));
            Assert.DoesNotContain(nameof(IOException), finding.Cause, StringComparison.Ordinal);
            Assert.DoesNotContain("0x", finding.Cause, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexMutationMapper.CreateFinding((IndexFindingCode)int.MaxValue));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index operation retains automatic and explicit cancellation origins"),
     Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void CancelledProjectionReadsRetainRequestOrigin()
    {
        var automatic = IndexProjectionReadResult.Cancelled(
            Request(IndexMode.Apply),
            Finding(IndexFindingCode.Interrupted));
        var explicitSelection = IndexProjectionReadResult.Cancelled(
            Request(IndexMode.Apply, RootPath),
            Finding(IndexFindingCode.Interrupted));

        Assert.Equal(IndexProjectionReadState.Cancelled, automatic.State);
        Assert.Equal(IndexSelectionOrigin.AutomaticLoader, automatic.Selection.Origin);
        Assert.Equal(IndexSelectionOrigin.ExplicitSources, explicitSelection.Selection.Origin);
        Assert.Equal(IndexFindingCode.Interrupted, Assert.Single(explicitSelection.Findings).Code);
        Assert.Throws<ArgumentException>(() => IndexProjectionReadResult.Cancelled(
            Request(IndexMode.Apply),
            Finding(IndexFindingCode.OperationFailed)));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index operation maps workspace-lock failures and cancellation without inferring contention"),
     Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void WorkspaceLockFailuresAndCancellationUseOnlyTypedFacts()
    {
        var mapped = new[]
        {
            (FilesystemFailureKind.InvalidPath, IndexFindingCode.WorkspaceUnsafe),
            (FilesystemFailureKind.AccessDenied, IndexFindingCode.WorkspaceLockUnavailable),
            (FilesystemFailureKind.InputOutput, IndexFindingCode.WorkspaceLockUnavailable),
            (FilesystemFailureKind.Unsupported, IndexFindingCode.WorkspaceLockUnavailable),
        };
        foreach (var testCase in mapped)
        {
            var failure = Failure(testCase.Item1);
            Assert.Equal(testCase.Item2, IndexMutationMapper.ReadLockFailureCode(failure));
            Assert.Equal(
                testCase.Item2,
                IndexMutationMapper.ReadLockFindingCode(WorkspaceLockResult.Failed(failure)));
        }

        Assert.Equal(
            IndexFindingCode.Interrupted,
            IndexMutationMapper.ReadLockFindingCode(WorkspaceLockResult.Cancelled()));
        Assert.Throws<InvalidOperationException>(() =>
            IndexMutationMapper.ReadLockFailureCode(Failure(FilesystemFailureKind.InvalidEncoding)));
        Assert.Throws<InvalidOperationException>(() =>
            IndexMutationMapper.ReadLockFailureCode(Failure(FilesystemFailureKind.InvalidSyntax)));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index operation requires exact ordered whole-plan revalidation"),
     Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void WholePlanValidationMapsEveryTypedStateAndRejectsIncoherentValidChecks()
    {
        var plan = Plan(IndexMode.Apply, Projection(Update(BeforeBody, ExpectedBody)));
        var change = Assert.Single(plan.Updates);
        var before = Assert.Single(plan.RecoveryTargets).Before;
        var physicalPath = before.PhysicalPath
            ?? throw new InvalidOperationException("The update fixture requires its physical path.");
        var matched = FileExpectationValidationResult.Matched(
            expectation: change.Expectation,
            actual: before,
            physicalPath: physicalPath);

        var exact = IndexMutationMapper.ReadValidation(Validation(plan, [matched]));
        Assert.Null(exact.FindingCode);
        Assert.Null(exact.Source);
        Assert.Null(exact.Cause);
        var incoherent = IndexMutationMapper.ReadValidation(new IndexValidationMappingInput
        {
            Plan = plan,
            Validation = MutationValidationResult.Valid(),
        });
        Assert.Equal(
            IndexFindingCode.OperationFailed,
            incoherent.FindingCode);
        Assert.Null(incoherent.Source);

        var different = FileStateSnapshot.File(
            logicalPath: before.LogicalPath,
            physicalPath: physicalPath,
            bytes: "different"u8);
        var cases = new[]
        {
            (
                Result: MutationValidationResult.FromChecks([
                    FileExpectationValidationResult.Mismatched(
                        expectation: change.Expectation,
                        actual: different,
                        physicalPath: physicalPath,
                        cause: "Changed.")]),
                Finding: IndexFindingCode.TargetChanged,
                HasSource: true),
            (
                Result: MutationValidationResult.Blocked("Unsafe."),
                Finding: IndexFindingCode.TargetUnsafe,
                HasSource: false),
            (
                Result: MutationValidationResult.Blocked(
                    cause: "Unsafe target.",
                    checks: [FileExpectationValidationResult.Blocked(
                        change.Expectation,
                        "Unsafe target.")]),
                Finding: IndexFindingCode.TargetUnsafe,
                HasSource: true),
            (
                Result: MutationValidationResult.FromChecks([
                    FileExpectationValidationResult.Failed(
                        change.Expectation,
                        Failure(FilesystemFailureKind.InputOutput))]),
                Finding: IndexFindingCode.ProjectionIncomplete,
                HasSource: true),
            (
                Result: MutationValidationResult.FromChecks([
                    FileExpectationValidationResult.Cancelled(change.Expectation)]),
                Finding: IndexFindingCode.Interrupted,
                HasSource: true),
            (
                Result: MutationValidationResult.Cancelled(),
                Finding: IndexFindingCode.Interrupted,
                HasSource: false),
        };
        foreach (var testCase in cases)
        {
            var mapping = IndexMutationMapper.ReadValidation(new IndexValidationMappingInput
            {
                Plan = plan,
                Validation = testCase.Result,
            });
            Assert.Equal(
                testCase.Finding,
                mapping.FindingCode);
            if (testCase.HasSource)
            {
                Assert.Equal(RootPath, Assert.IsType<IndexLogicalSource>(mapping.Source).Path);
            }
            else
            {
                Assert.Null(mapping.Source);
            }
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index operation maps nonprepared recovery without claiming artifact presence"),
     Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void NonpreparedRecoveryRetainsOnlyObservedResidualPathTruth()
    {
        var residualPath = Physical("recovery/incomplete.zip");
        var cases = new[]
        {
            (
                Result: RecoveryBundlePreparationResult.NotNeeded(),
                Finding: IndexFindingCode.OperationFailed,
                State: IndexRecoveryState.NotCreated,
                Path: (string?)null),
            (
                Result: RecoveryBundlePreparationResult.Incomplete("Unavailable."),
                Finding: IndexFindingCode.RecoveryUnavailable,
                State: IndexRecoveryState.NotCreated,
                Path: (string?)null),
            (
                Result: RecoveryBundlePreparationResult.Incomplete(
                    cause: "Unavailable.",
                    residualPath: residualPath),
                Finding: IndexFindingCode.RecoveryUnavailable,
                State: IndexRecoveryState.Unknown,
                Path: (string?)residualPath),
            (
                Result: RecoveryBundlePreparationResult.Blocked("Conflict."),
                Finding: IndexFindingCode.RecoveryConflict,
                State: IndexRecoveryState.NotCreated,
                Path: (string?)null),
            (
                Result: RecoveryBundlePreparationResult.Blocked(
                    cause: "Conflict.",
                    residualPath: residualPath),
                Finding: IndexFindingCode.RecoveryConflict,
                State: IndexRecoveryState.Unknown,
                Path: (string?)residualPath),
            (
                Result: RecoveryBundlePreparationResult.Cancelled(),
                Finding: IndexFindingCode.Interrupted,
                State: IndexRecoveryState.NotCreated,
                Path: (string?)null),
            (
                Result: RecoveryBundlePreparationResult.Cancelled(residualPath: residualPath),
                Finding: IndexFindingCode.Interrupted,
                State: IndexRecoveryState.Unknown,
                Path: (string?)residualPath),
        };
        foreach (var testCase in cases)
        {
            var mapping = IndexMutationMapper.ReadPreparation(testCase.Result);

            Assert.False(mapping.CanApply);
            Assert.Null(mapping.Preparation);
            Assert.Equal(testCase.Finding, mapping.FindingCode);
            Assert.Equal(testCase.State, mapping.Recovery.State);
            Assert.Equal(testCase.Path, mapping.Recovery.ResidualPath);
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index operation maps every coherent file receipt to its public stop behavior"),
     Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void FileReceiptsMapExactEffectVerificationAndReasonTruth()
    {
        var target = MutationTarget();
        var intended = FileStateSnapshot.File(
            logicalPath: target.Before.LogicalPath,
            physicalPath: target.Before.PhysicalPath
                ?? throw new InvalidOperationException("The receipt fixture requires its physical path."),
            bytes: target.Change.IntendedBytes.AsSpan());
        var observedDifferent = FileStateSnapshot.File(
            logicalPath: target.Before.LogicalPath,
            physicalPath: intended.PhysicalPath
                ?? throw new InvalidOperationException("The receipt fixture requires its intended identity."),
            bytes: "third state"u8);
        var cases = new[]
        {
            (
                Receipt: FileChangeReceipt.Verified(
                    change: target.Change,
                    before: target.Before,
                    after: intended),
                Outcome: IndexRegionOutcome.Verified,
                Finding: (IndexFindingCode?)null,
                Continue: true),
            (
                Receipt: FileChangeReceipt.NotStarted(
                    change: target.Change,
                    before: target.Before,
                    reason: FilesystemNotStartedReason.TargetChanged,
                    cause: "Changed."),
                Outcome: IndexRegionOutcome.NotStarted,
                Finding: (IndexFindingCode?)IndexFindingCode.TargetChangedDuringApply,
                Continue: false),
            (
                Receipt: FileChangeReceipt.NotStarted(
                    change: target.Change,
                    before: target.Before,
                    reason: FilesystemNotStartedReason.Cancelled,
                    cause: "Cancelled."),
                Outcome: IndexRegionOutcome.NotStarted,
                Finding: (IndexFindingCode?)IndexFindingCode.Interrupted,
                Continue: false),
            (
                Receipt: FileChangeReceipt.NotStarted(
                    change: target.Change,
                    before: target.Before,
                    reason: FilesystemNotStartedReason.ApplicationFailed,
                    cause: "Unavailable."),
                Outcome: IndexRegionOutcome.NotStarted,
                Finding: (IndexFindingCode?)IndexFindingCode.WriteFailed,
                Continue: false),
            (
                Receipt: FileChangeReceipt.NotStarted(
                    change: target.Change,
                    before: target.Before,
                    reason: FilesystemNotStartedReason.ContractRejected,
                    cause: "Rejected."),
                Outcome: IndexRegionOutcome.NotStarted,
                Finding: (IndexFindingCode?)IndexFindingCode.OperationFailed,
                Continue: false),
            (
                Receipt: FileChangeReceipt.VerificationFailed(
                    change: target.Change,
                    before: target.Before,
                    after: observedDifferent,
                    cause: "Mismatch."),
                Outcome: IndexRegionOutcome.Applied,
                Finding: (IndexFindingCode?)IndexFindingCode.VerificationFailed,
                Continue: false),
            (
                Receipt: FileChangeReceipt.VerificationUnavailable(
                    change: target.Change,
                    before: target.Before,
                    cause: "Unavailable."),
                Outcome: IndexRegionOutcome.Applied,
                Finding: (IndexFindingCode?)IndexFindingCode.VerificationFailed,
                Continue: false),
            (
                Receipt: FileChangeReceipt.CompletionUnknown(
                    change: target.Change,
                    before: target.Before,
                    after: observedDifferent,
                    cause: "Unknown."),
                Outcome: IndexRegionOutcome.Unknown,
                Finding: (IndexFindingCode?)IndexFindingCode.WriteFailed,
                Continue: false),
        };
        foreach (var testCase in cases)
        {
            var mapping = IndexMutationMapper.ReadReceipt(testCase.Receipt);

            Assert.Equal(testCase.Outcome, mapping.Outcome);
            Assert.Equal(testCase.Finding, mapping.FindingCode);
            Assert.Equal(testCase.Continue, mapping.ShouldContinue);
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexReceiptMapping(
            outcome: (IndexRegionOutcome)int.MaxValue,
            findingCode: IndexFindingCode.OperationFailed,
            shouldContinue: false));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index operation maps recovery deletion state and disposition independently"),
     Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void RecoveryDeletionMappingsNeverInferRetentionFromUnknown()
    {
        var path = Physical("recovery/final.zip");
        var cases = new[]
        {
            (
                Result: RecoveryBundleDeletionResult.Deleted(),
                State: IndexRecoveryState.Removed,
                Finding: (IndexFindingCode?)null,
                Path: (string?)null),
            (
                Result: RecoveryBundleDeletionResult.FailedRetained(
                    residualPath: path,
                    cause: "Failure."),
                State: IndexRecoveryState.Retained,
                Finding: (IndexFindingCode?)IndexFindingCode.RecoveryArtifactRetained,
                Path: (string?)path),
            (
                Result: RecoveryBundleDeletionResult.FailedUnknown(
                    residualPath: path,
                    cause: "Failure."),
                State: IndexRecoveryState.Unknown,
                Finding: (IndexFindingCode?)IndexFindingCode.RecoveryFailed,
                Path: (string?)path),
            (
                Result: RecoveryBundleDeletionResult.BlockedRetained(
                    residualPath: path,
                    cause: "Blocked."),
                State: IndexRecoveryState.Retained,
                Finding: (IndexFindingCode?)IndexFindingCode.RecoveryFailed,
                Path: (string?)path),
            (
                Result: RecoveryBundleDeletionResult.BlockedUnknown(
                    cause: "Blocked.",
                    residualPath: path),
                State: IndexRecoveryState.Unknown,
                Finding: (IndexFindingCode?)IndexFindingCode.RecoveryFailed,
                Path: (string?)path),
            (
                Result: RecoveryBundleDeletionResult.CancelledRetained(path),
                State: IndexRecoveryState.Retained,
                Finding: (IndexFindingCode?)IndexFindingCode.Interrupted,
                Path: (string?)path),
            (
                Result: RecoveryBundleDeletionResult.CancelledUnknown(),
                State: IndexRecoveryState.Unknown,
                Finding: (IndexFindingCode?)IndexFindingCode.Interrupted,
                Path: (string?)null),
        };
        foreach (var testCase in cases)
        {
            var mapping = IndexMutationMapper.ReadDeletion(testCase.Result);

            Assert.Equal(testCase.State, mapping.Recovery.State);
            Assert.Equal(testCase.Finding, mapping.FindingCode);
            Assert.Equal(testCase.Path, mapping.Recovery.ResidualPath);
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index fresh verification requires original identity and expected bodies"),
     Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void FreshProjectionProvesExactSelectionTargetsAndOriginalExpectedBodies()
    {
        var original = Plan(IndexMode.Apply, Projection(Update(BeforeBody, ExpectedBody)));
        var exact = IndexProjectionReadResult.Projected(Projection(Unchanged(ExpectedBody)));
        var stale = IndexProjectionReadResult.Projected(Projection(Unchanged(BeforeBody)));
        var originallyUnchanged = Plan(IndexMode.Apply, Projection(Unchanged(ExpectedBody)));
        var unchangedDrift = IndexProjectionReadResult.Projected(Projection(Unchanged("drifted\n")));
        var identityDrift = IndexProjectionReadResult.Projected(Projection(ProjectedRegion(
            beforeBody: ExpectedBody,
            expectedBody: ExpectedBody,
            physicalPath: "workspace-moved/.agents/root/_root.md")));
        var cancelled = IndexProjectionReadResult.Cancelled(
            original.Input.Request,
            Finding(IndexFindingCode.Interrupted));

        var exactMapping = IndexMutationMapper.ReadFreshVerification(new IndexFreshVerificationInput
        {
            Original = original,
            Fresh = exact,
        });
        Assert.Null(exactMapping.FindingCode);
        Assert.Null(exactMapping.Source);
        Assert.Null(exactMapping.Cause);

        var cases = new[]
        {
            (Original: original, Fresh: stale, Finding: IndexFindingCode.VerificationFailed, HasSource: true),
            (
                Original: originallyUnchanged,
                Fresh: unchangedDrift,
                Finding: IndexFindingCode.VerificationFailed,
                HasSource: true),
            (Original: original, Fresh: identityDrift, Finding: IndexFindingCode.VerificationFailed, HasSource: true),
            (Original: original, Fresh: cancelled, Finding: IndexFindingCode.Interrupted, HasSource: false),
        };
        foreach (var testCase in cases)
        {
            var mapping = IndexMutationMapper.ReadFreshVerification(new IndexFreshVerificationInput
            {
                Original = testCase.Original,
                Fresh = testCase.Fresh,
            });
            Assert.Equal(testCase.Finding, mapping.FindingCode);
            Assert.False(string.IsNullOrWhiteSpace(mapping.Cause));
            if (testCase.HasSource)
            {
                Assert.Equal(RootPath, Assert.IsType<IndexLogicalSource>(mapping.Source).Path);
            }
            else
            {
                Assert.Null(mapping.Source);
            }
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index fresh verification permits only the same skipped metadata boundary")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void FreshProjectionMatchesSkippedMetadataAsCurrentUnsafeFacts()
    {
        var original = Plan(
            IndexMode.Apply,
            PartialProjection(IndexFindingCode.MetadataSkipped, BeforeBody, ExpectedBody));
        var fresh = IndexProjectionReadResult.Projected(
            PartialProjection(IndexFindingCode.MetadataUnsafe, ExpectedBody, ExpectedBody));

        var mapping = IndexMutationMapper.ReadFreshVerification(new IndexFreshVerificationInput
        {
            Original = original,
            Fresh = fresh,
        });

        Assert.Null(mapping.FindingCode);
        Assert.Null(mapping.Source);
        Assert.Null(mapping.Cause);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index fresh verification maps every projection-read state and rejects undefined values"),
     Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void FreshProjectionReadStateMappingIsExact()
    {
        Assert.Equal(
            IndexFindingCode.Interrupted,
            IndexMutationMapper.ReadProjectionReadState(IndexProjectionReadState.Cancelled));
        Assert.Equal(
            IndexFindingCode.VerificationFailed,
            IndexMutationMapper.ReadProjectionReadState(IndexProjectionReadState.SelectionIncomplete));
        Assert.Null(IndexMutationMapper.ReadProjectionReadState(IndexProjectionReadState.Projected));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexMutationMapper.ReadProjectionReadState((IndexProjectionReadState)int.MaxValue));
    }

    private static IndexValidationMappingInput Validation(
        IndexPlan plan,
        IReadOnlyList<FileExpectationValidationResult> checks)
        => new()
        {
            Plan = plan,
            Validation = MutationValidationResult.FromChecks(checks),
        };

    private static IndexPlan Plan(
        IndexMode mode,
        IndexProjectionFormation projection)
        => new IndexPlanBuilder().Build(new IndexPlanningInput
        {
            Request = Request(mode, RootPath),
            Projection = projection,
        });

    private static IndexProjectionFormation Projection(IndexProjectedRegion region)
    {
        var navigation = new GeneratedNavigationProjection([region.Region]);
        var selection = new IndexSelectionResolution(
            new IndexSelection(
                IndexSelectionOrigin.ExplicitSources,
                IndexSelectionScope.Rooted,
                [region.Source]),
            [region.Region.Source],
            []);
        return new IndexProjectionFormation(
            selection: selection,
            projection: navigation,
            regions: [region],
            findings: []);
    }

    private static IndexProjectionFormation PartialProjection(
        IndexFindingCode skippedCode,
        string safeBefore,
        string safeExpected)
    {
        var safe = ProjectedRegion(safeBefore, safeExpected);
        var skippedSource = Source(
            "workspace/.agents/beta/_beta.md",
            ".agents/beta/_beta.md");
        var skippedRegion = GeneratedNavigationRegion.Unavailable(
            skippedSource,
            GeneratedNavigationRegionUnavailableReason.MetadataInvalid,
            "The authored metadata is malformed.");
        var skipped = new IndexProjectedRegion(
            region: skippedRegion,
            source: new IndexLogicalSource(
                id: skippedSource.Identity.AutomaticId,
                path: skippedSource.Identity.CanonicalBasePath,
                scope: IndexLogicalSourceScope.Rooted),
            beforeEntryCount: null);
        var projection = new GeneratedNavigationProjection([safe.Region, skipped.Region]);
        var selection = new IndexSelectionResolution(
            new IndexSelection(
                IndexSelectionOrigin.ExplicitSources,
                IndexSelectionScope.Rooted,
                [safe.Source, skipped.Source]),
            [safe.Region.Source, skipped.Region.Source],
            []);
        var finding = new IndexFinding(
            skippedCode,
            sourceOccurrence: null,
            source: skipped.Source,
            cause: "The authored metadata is invalid.",
            candidates: [])
        {
            Details = new IndexFindingDetails
            {
                ParentPath = skipped.Source.Path,
                MetadataProblem = IndexMetadataProblem.Invalid,
            },
        };
        return new IndexProjectionFormation(
            selection,
            projection,
            [skipped, safe],
            [finding]);
    }

    private static IndexProjectedRegion Update(
        string beforeBody,
        string expectedBody)
        => ProjectedRegion(beforeBody, expectedBody);

    private static IndexProjectedRegion Unchanged(string body)
        => ProjectedRegion(body, body);

    private static IndexProjectedRegion ProjectedRegion(
        string beforeBody,
        string expectedBody,
        string physicalPath = "workspace/.agents/root/_root.md")
    {
        var source = Source(physicalPath);
        var change = new GeneratedNavigationBoundedChange(new GeneratedNavigationBoundedChangeInput
        {
            ContentLocation = new SourceLocation(
                line: 2,
                column: 1,
                byteOffset: Encoding.UTF8.GetByteCount(Prefix),
                byteLength: Encoding.UTF8.GetByteCount(beforeBody)),
            BeforeBody = beforeBody,
            ExpectedBody = expectedBody,
            Prefix = Prefix,
            Suffix = Suffix,
        });
        return new IndexProjectedRegion(
            region: GeneratedNavigationRegion.Available(
                source: source,
                entries: [],
                change: change),
            source: new IndexLogicalSource(
                id: source.Identity.AutomaticId,
                path: source.Identity.CanonicalBasePath,
                scope: IndexLogicalSourceScope.Rooted),
            beforeEntryCount: 0);
    }

    private static SourceLogicalSource Source(string physicalPath)
        => Source(physicalPath, RootPath);

    private static SourceLogicalSource Source(
        string physicalPath,
        string canonicalPath)
    {
        var automaticId = SourceIdentity.DeriveId(canonicalPath)
            ?? throw new InvalidOperationException("The operation fixture requires a derived source ID.");
        return new SourceLogicalSource(
            new SourceLogicalIdentity(
                automaticId: automaticId,
                canonicalBasePath: canonicalPath),
            new SourceLayer(
                canonicalPath: canonicalPath,
                physicalPath: Physical(physicalPath),
                form: SourceDocumentForm.CanonicalEntrypoint,
                kind: SourceLayerKind.Base));
    }

    private static RecoveryBundleTarget MutationTarget()
    {
        var before = FileStateSnapshot.File(
            logicalPath: Physical("workspace/.agents/root/_root.md"),
            physicalPath: Physical("workspace/.agents/root/_root.md"),
            bytes: "before"u8);
        return RecoveryBundleTarget.Create(
            change: PlannedFileChange.ReplaceGeneratedRegion(
                expectation: before.Expectation,
                intendedDocumentBytes: "after"u8),
            before: before);
    }

    private static IndexRequest Request(
        IndexMode mode,
        params string[] sources)
        => new(
            Workspace(),
            sources,
            mode);

    private static IndexFinding Finding(IndexFindingCode code)
        => new(
            code: code,
            sourceOccurrence: null,
            source: null,
            cause: "Operation mapping evidence.",
            candidates: []);

    private static FilesystemFailure Failure(FilesystemFailureKind kind)
        => new(kind, "Typed filesystem failure.");

    private static CliWorkspace Workspace()
        => new(
            lexicalRoot: Physical("workspace"),
            physicalRoot: Physical("workspace"),
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static string Physical(string relativePath)
        => Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "open-forge-index-operation-unit",
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
}
