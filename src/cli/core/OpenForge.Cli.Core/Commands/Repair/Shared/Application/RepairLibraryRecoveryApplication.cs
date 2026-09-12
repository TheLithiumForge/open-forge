using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Application;
using OpenForge.Cli.Core.Framework.Recovery.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Observation;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Application;

internal static class RepairLibraryRecoveryApplication
{
    internal static async ValueTask<RepairPreflightOutcome> PreflightAsync(
        RepairMutationServices services,
        RepairPlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();
        if (plan.IsBlocked)
        {
            return new RepairPreflightOutcome(
                RepairApplicationOutcomeFactory.BeforeApplication(
                    RepairPreflightState.Blocked,
                    "The atomic Repair plan is blocked.",
                    RepairFindingCode.PlanConflict),
                []);
        }

        var changes = plan.Effects.Select(effect => effect.FileChange).ToArray();
        if (changes.Length != 0)
        {
            var validation = await services.Preflight.ValidateAsync(
                plan.Request.Workspace,
                changes,
                cancellationToken).ConfigureAwait(false);
            if (validation.State != MutationValidationState.Valid)
            {
                return new RepairPreflightOutcome(
                    RepairApplicationOutcomeFactory.ValidationBoundary(validation),
                    []);
            }
        }

        var targets = await new RepairTargetReader().ReadAsync(plan, cancellationToken)
            .ConfigureAwait(false);
        if (targets is null)
        {
            return new RepairPreflightOutcome(
                RepairApplicationOutcomeFactory.BeforeApplication(
                    RepairPreflightState.Blocked,
                    "A selected Repair reference target could not be proven.",
                    RepairFindingCode.TargetUnsafe),
                []);
        }

        foreach (var step in plan.LibrarySteps)
        {
            var evidence = step.Selection.Proposal.Evidence;
            var permission = await LibraryRecoveryPermissionReader.ObserveAsync(
                plan.Request.Workspace, evidence, cancellationToken).ConfigureAwait(false);
            if (!permission.IsAdmitted)
            {
                return new RepairPreflightOutcome(
                    RepairApplicationOutcomeFactory.BeforeApplication(
                        RepairPreflightState.Blocked,
                        "Current consumer permissions or Library source protection do not admit the selected recovery.",
                        RepairFindingCode.TargetUnsafe),
                    []);
            }

            var selected = await RecoveryBundleReader.ReadSelectedFinalAsync(
                plan.Request.Workspace,
                evidence.Residual.Candidate,
                cancellationToken).ConfigureAwait(false);
            if (selected.Read.State == RecoveryBundleReadState.Cancelled)
            {
                return new RepairPreflightOutcome(
                    RepairApplicationOutcomeFactory.BeforeApplication(
                        RepairPreflightState.Incomplete,
                        "Library recovery preflight was interrupted.",
                        RepairFindingCode.Interrupted),
                    []);
            }

            if (selected.Preparation is null
                || !await MatchesObservedEntryAsync(step, cancellationToken).ConfigureAwait(false))
            {
                return new RepairPreflightOutcome(
                    RepairApplicationOutcomeFactory.BeforeApplication(
                        RepairPreflightState.Blocked,
                        "The selected Library residual or current no-follow entry state changed before preflight.",
                        RepairFindingCode.TargetChanged),
                    []);
            }
        }

        return new RepairPreflightOutcome(
            RepairApplicationOutcomeFactory.Preview(
                changes.Length != 0 || plan.LibrarySteps.Any(step => step.Effect is not null)),
            targets);
    }

    internal static async ValueTask<RepairApplicationOutcome> ExecuteAsync(
        RepairMutationServices services,
        RepairPlanRevalidator revalidator,
        RepairPostVerifier postVerifier,
        RepairPlan plan,
        IReadOnlyList<FileExpectation> referenceTargets,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(postVerifier);
        var preparation = await PrepareAtomicAsync(
            services,
            revalidator,
            plan,
            referenceTargets,
            cancellationToken).ConfigureAwait(false);
        if (preparation.Boundary is { } boundary)
        {
            return boundary;
        }

        var prepared = preparation.Prepared
            ?? throw new InvalidOperationException("A ready atomic Repair application requires its held preparation.");
        await using var lease = prepared.Lease.ConfigureAwait(false);
        var execution = await ApplyAtomicAsync(prepared, cancellationToken).ConfigureAwait(false);
        try
        {
            var postVerification = await postVerifier.VerifyLibraryAsync(
                plan,
                execution,
                execution.Cancellation is null ? cancellationToken : CancellationToken.None).ConfigureAwait(false);
            execution = execution with
            {
                PostDiagnosis = postVerification.Diagnosis,
                ReferencePostVerification = postVerification.Verification,
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            execution = execution with
            {
                Cancellation = execution.Cancellation
                    ?? new RepairLibraryCancellation(RepairLibraryExecutionStage.PostDiagnosis, null),
            };
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            execution = execution with
            {
                UnexpectedFailure = execution.UnexpectedFailure
                    ?? new RepairLibraryUnexpectedFailure(
                        RepairLibraryExecutionStage.PostDiagnosis,
                        null,
                        $"Fresh post-diagnosis failed: {exception.GetType().Name}."),
            };
        }

        return Complete(plan, execution);
    }

    private static async ValueTask<AtomicPreparationResult> PrepareAtomicAsync(
        RepairMutationServices services,
        RepairPlanRevalidator revalidator,
        RepairPlan plan,
        IReadOnlyList<FileExpectation> referenceTargets,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(revalidator);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(referenceTargets);
        if (plan.IsBlocked)
        {
            return AtomicPreparationResult.BoundaryResult(Boundary(
                RepairPreflightState.Blocked,
                RepairFindingCode.PlanConflict,
                "The atomic Repair plan is blocked before application."));
        }

        var operationId = Guid.NewGuid();
        WorkspaceLockResult acquired;
        try
        {
            acquired = await services.LockManager.AcquireAsync(
                new WorkspaceLockRequest(
                    plan.Request.Workspace,
                    RepairDefinitions.CommandIdentity,
                    operationId),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return AtomicPreparationResult.BoundaryResult(Cancelled(
                RepairLibraryExecutionStage.Lease,
                "Repair lock acquisition was interrupted."));
        }

        if (acquired.State != WorkspaceLockState.Acquired || acquired.Lease is null)
        {
            return AtomicPreparationResult.BoundaryResult(acquired.State == WorkspaceLockState.Cancelled
                ? Cancelled(RepairLibraryExecutionStage.Lease, "Repair lock acquisition was interrupted.")
                : Boundary(
                    RepairPreflightState.Blocked,
                    RepairFindingCode.WorkspaceLockUnavailable,
                    acquired.Cause ?? "The Repair workspace lock could not be acquired."));
        }

        var lease = acquired.Lease;
        try
        {
            foreach (var step in plan.LibrarySteps)
            {
                var evidence = step.Selection.Proposal.Evidence;
                var permission = await LibraryRecoveryPermissionReader.ReadAsync(
                    lease, evidence, cancellationToken).ConfigureAwait(false);
                if (!permission.IsAdmitted)
                {
                    await lease.DisposeAsync().ConfigureAwait(false);
                    return AtomicPreparationResult.BoundaryResult(Boundary(
                        RepairPreflightState.Blocked,
                        RepairFindingCode.TargetUnsafe,
                        "Current consumer permissions or Library source protection do not admit the selected recovery."));
                }

            }
            var changes = plan.Effects.Select(effect => effect.FileChange).ToArray();
            var validation = changes.Length == 0
                ? MutationValidationResult.Valid()
                : await services.Revalidator.ValidateAsync(
                    lease,
                    changes,
                    cancellationToken).ConfigureAwait(false);
            if (!await new RepairTargetReader().ValidateAsync(
                    plan,
                    referenceTargets,
                    cancellationToken).ConfigureAwait(false)
                || !await revalidator.ValidateAsync(plan, cancellationToken).ConfigureAwait(false))
            {
                await lease.DisposeAsync().ConfigureAwait(false);
                return AtomicPreparationResult.BoundaryResult(Boundary(
                    RepairPreflightState.Blocked,
                    RepairFindingCode.TargetChanged,
                    validation.Cause ?? "The complete Repair plan changed under the held workspace lease."));
            }

            var originals = ImmutableArray.CreateBuilder<RecoveryBundlePreparation>();
            foreach (var step in plan.LibrarySteps.Where(step => step.Effect is not null))
            {
                var evidence = step.Selection.Proposal.Evidence;
                var read = await RecoveryBundleReader.ReadSelectedFinalAsync(
                    plan.Request.Workspace,
                    evidence.Residual.Candidate,
                    cancellationToken).ConfigureAwait(false);
                if (read.Preparation is null
                    || !await MatchesObservedEntryAsync(step, cancellationToken).ConfigureAwait(false))
                {
                    await lease.DisposeAsync().ConfigureAwait(false);
                    return AtomicPreparationResult.BoundaryResult(Boundary(
                        RepairPreflightState.Blocked,
                        RepairFindingCode.TargetChanged,
                        "A selected Library residual or target changed under the held workspace lease."));
                }

                originals.Add(read.Preparation);
            }

            RecoveryBundlePreparation? forward = null;
            if (changes.Length != 0)
            {
                var result = await RepairRecoveryLifecycle.PrepareAsync(
                    plan,
                    operationId,
                    cancellationToken).ConfigureAwait(false);
                if (result.State != RecoveryBundlePreparationState.Prepared
                    || result.Preparation is null)
                {
                    await lease.DisposeAsync().ConfigureAwait(false);
                    return AtomicPreparationResult.BoundaryResult(
                        RepairApplicationOutcomeFactory.Preparation(
                            result,
                            RecoveryBundleAttribution.Create(
                                RecoveryBundleProducer.Repair,
                                RecoveryBundleOperation.Repair,
                                plan.Request.Workspace)) with
                        {
                            LibraryExecution = EmptyExecution() with
                            {
                                Cancellation = result.State == RecoveryBundlePreparationState.Cancelled
                                    ? new RepairLibraryCancellation(
                                        RepairLibraryExecutionStage.ForwardPreparation,
                                        null)
                                    : null,
                            },
                        });
                }

                forward = result.Preparation;
            }

            return AtomicPreparationResult.Ready(new RepairLibraryPreparedApplication
            {
                Plan = plan,
                Services = services,
                Lease = lease,
                ForwardPreparation = forward,
                ReferenceTargets = [.. referenceTargets],
                ReferenceChecks = [.. validation.Checks],
                OriginalPreparations = originals.ToImmutable(),
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await lease.DisposeAsync().ConfigureAwait(false);
            return AtomicPreparationResult.BoundaryResult(Cancelled(
                RepairLibraryExecutionStage.Revalidation,
                "Atomic Repair revalidation was interrupted."));
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            await lease.DisposeAsync().ConfigureAwait(false);
            return AtomicPreparationResult.BoundaryResult(Failed(
                RepairLibraryExecutionStage.Revalidation,
                $"Atomic Repair preparation failed: {exception.GetType().Name}."));
        }
    }

    private static async ValueTask<RepairLibraryExecution> ApplyAtomicAsync(
        RepairLibraryPreparedApplication prepared,
        CancellationToken cancellationToken)
    {
        if (!prepared.Lease.IsHeldFor(prepared.Plan.Request.Workspace))
        {
            throw new ArgumentException("Atomic Repair application requires its held workspace lease.", nameof(prepared));
        }

        var ordered = RepairAtomicEffectOrder.Read(prepared.Plan);
        var references = ImmutableArray.CreateBuilder<FileChangeReceipt>();
        var libraries = ImmutableArray.CreateBuilder<RepairLibraryRecoveryReceipt>();
        foreach (var step in ordered)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                switch (step.Kind)
                {
                    case RepairAtomicEffectKind.Reference:
                        var reference = step.Reference
                            ?? throw new ArgumentException("A reference effect payload is required.", nameof(prepared));
                        var referenceReceipt = await ApplyReferenceAsync(
                            prepared,
                            reference,
                            cancellationToken).ConfigureAwait(false);
                        references.Add(referenceReceipt);
                        if (!IsVerified(referenceReceipt))
                        {
                            return await FinishAsync(
                                prepared,
                                references,
                                libraries,
                                cancellation: referenceReceipt.NotStartedReason == FilesystemNotStartedReason.Cancelled
                                    ? new RepairLibraryCancellation(RepairLibraryExecutionStage.Effect, step.Ordinal)
                                    : null,
                                failure: referenceReceipt.NotStartedReason == FilesystemNotStartedReason.Cancelled
                                    ? null
                                    : new RepairLibraryUnexpectedFailure(
                                        referenceReceipt.EffectState == FilesystemEffectState.Applied
                                            ? RepairLibraryExecutionStage.Verification
                                            : RepairLibraryExecutionStage.Effect,
                                        step.Ordinal,
                                        referenceReceipt.Cause ?? "A Repair reference effect did not verify."),
                                cancellationToken).ConfigureAwait(false);
                        }
                        break;
                    case RepairAtomicEffectKind.LibraryRecovery:
                        var library = step.LibraryRecovery
                            ?? throw new ArgumentException("A Library recovery effect payload is required.", nameof(prepared));
                        var libraryReceipt = await ApplyLibraryEntryAsync(
                            prepared,
                            library,
                            cancellationToken).ConfigureAwait(false);
                        libraries.Add(libraryReceipt);
                        if (!IsVerified(libraryReceipt))
                        {
                            return await FinishAsync(
                                prepared,
                                references,
                                libraries,
                                cancellation: IsCancelled(libraryReceipt)
                                    ? new RepairLibraryCancellation(RepairLibraryExecutionStage.Effect, step.Ordinal)
                                    : null,
                                failure: IsCancelled(libraryReceipt)
                                    ? null
                                    : new RepairLibraryUnexpectedFailure(
                                        RepairLibraryExecutionStage.Effect,
                                        step.Ordinal,
                                        Cause(libraryReceipt)),
                                cancellationToken).ConfigureAwait(false);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(prepared), "The atomic Repair effect kind is not defined.");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return await FinishAsync(
                    prepared,
                    references,
                    libraries,
                    new RepairLibraryCancellation(RepairLibraryExecutionStage.Effect, step.Ordinal),
                    failure: null,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                return await FinishAsync(
                    prepared,
                    references,
                    libraries,
                    cancellation: null,
                    new RepairLibraryUnexpectedFailure(
                        RepairLibraryExecutionStage.Effect,
                        step.Ordinal,
                        $"Atomic Repair effect failed: {exception.GetType().Name}."),
                    cancellationToken).ConfigureAwait(false);
            }
        }

        return await FinishAsync(
            prepared,
            references,
            libraries,
            cancellation: null,
            failure: null,
            cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask<FileChangeReceipt> ApplyReferenceAsync(
        RepairLibraryPreparedApplication prepared,
        RepairEffect effect,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(prepared);
        ArgumentNullException.ThrowIfNull(effect);
        var index = prepared.Plan.Effects.ToList().IndexOf(effect);
        if (index < 0 || index >= prepared.ReferenceChecks.Length)
        {
            throw new ArgumentException("A reference effect requires its all-effect revalidation check.", nameof(effect));
        }

        return await prepared.Services.Applier.ApplyAsync(
            prepared.Lease,
            effect.FileChange,
            prepared.ReferenceChecks[index],
            prepared.ForwardPreparation,
            cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask<RepairLibraryExecution> FinishAsync(
        RepairLibraryPreparedApplication prepared,
        ImmutableArray<FileChangeReceipt>.Builder referenceReceipts,
        ImmutableArray<RepairLibraryRecoveryReceipt>.Builder libraryReceipts,
        RepairLibraryCancellation? cancellation,
        RepairLibraryUnexpectedFailure? failure,
        CancellationToken cancellationToken)
    {
        RecoveryBundleDeletionResult? cleanup = null;
        if (cancellation is null && failure is null && prepared.ForwardPreparation is { } forward)
        {
            try
            {
                cleanup = await RepairRecoveryLifecycle.DeleteAsync(
                    prepared.Lease,
                    forward,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                cancellation = new RepairLibraryCancellation(
                    RepairLibraryExecutionStage.ForwardCleanup,
                    null);
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                failure = new RepairLibraryUnexpectedFailure(
                    RepairLibraryExecutionStage.ForwardCleanup,
                    null,
                    $"Forward Repair recovery cleanup failed: {exception.GetType().Name}.");
            }
        }

        return new RepairLibraryExecution
        {
            ReferenceReceipts = referenceReceipts.ToImmutable(),
            LibraryReceipts = libraryReceipts.ToImmutable(),
            ForwardPreparation = prepared.ForwardPreparation,
            ForwardCleanup = cleanup,
            PostDiagnosis = null,
            Cancellation = cancellation,
            UnexpectedFailure = failure,
        };
    }

    private static async ValueTask<RepairLibraryRecoveryReceipt> ApplyLibraryEntryAsync(
        RepairLibraryPreparedApplication prepared,
        RepairLibraryRecoveryEffect effect,
        CancellationToken cancellationToken)
    {
        var evidence = effect.Selection.Proposal.Evidence;
        var original = prepared.OriginalPreparations.SingleOrDefault(value =>
            string.Equals(value.BundlePath, evidence.Residual.Candidate.Path, StringComparison.Ordinal)
            && value.Entries.Contains(effect.Entry))
            ?? throw new ArgumentException("Selected Library recovery requires its exact prepared original residual.", nameof(effect));
        var verified = evidence.Residual.Candidate.Verified
            ?? throw new ArgumentException("Selected Library recovery requires its original verified final.", nameof(effect));
        if (!string.Equals(original.BundlePath, verified.BundlePath, StringComparison.Ordinal)
            || original.OperationId != verified.OperationId
            || original.Attribution != verified.Attribution
            || !string.Equals(original.Command, verified.Command, StringComparison.Ordinal)
            || !prepared.Lease.IsHeldFor(prepared.Plan.Request.Workspace)
            || !original.MatchesWorkspace(prepared.Plan.Request.Workspace)
            || !original.Entries.Contains(effect.Entry))
        {
            throw new ArgumentException("Library recovery requires an exact original residual entry and held workspace lease.", nameof(effect));
        }

        var resolver = new PhysicalPathResolver();
        return effect.Entry.Kind switch
        {
            RecoveryEntryKind.OrdinaryCreate or RecoveryEntryKind.OrdinaryReplace
                or RecoveryEntryKind.OrdinaryReplaceGeneratedRegion or RecoveryEntryKind.OrdinaryDelete
                => new RepairLibraryRecoveryReceipt(
                    RepairLibraryRecoveryKind.Ordinary,
                    effect,
                    original,
                    await OrdinaryFileRecoveryApplier.ApplyAsync(
                        resolver,
                        prepared.Lease,
                        original,
                        effect.Entry,
                        cancellationToken).ConfigureAwait(false),
                    relativeFileLink: null),
            RecoveryEntryKind.RelativeFileLinkCreate or RecoveryEntryKind.RelativeFileLinkDelete
                => new RepairLibraryRecoveryReceipt(
                    RepairLibraryRecoveryKind.RelativeFileLink,
                    effect,
                    original,
                    ordinary: null,
                    await RelativeFileLinkRecoveryApplier.ApplyAsync(
                        resolver,
                        prepared.Lease,
                        original,
                        effect.Entry,
                        cancellationToken).ConfigureAwait(false)),
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Entry.Kind, "The selected recovery entry kind is not defined."),
        };
    }

    internal static RepairApplicationOutcome Complete(RepairPlan plan, RepairLibraryExecution execution)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(execution);
        if (execution.ReferenceReceipts.IsDefault || execution.LibraryReceipts.IsDefault)
        {
            throw new ArgumentException("Repair completion requires retained immutable execution receipts.", nameof(execution));
        }

        if (execution.Cancellation is { } cancellation && !Enum.IsDefined(cancellation.Stage)
            || execution.UnexpectedFailure is { } failure && !Enum.IsDefined(failure.Stage))
        {
            throw new ArgumentException("Repair execution timing requires defined stages.", nameof(execution));
        }

        var applied = execution.ReferenceReceipts.Count(receipt =>
                receipt.EffectState == FilesystemEffectState.Applied)
            + execution.LibraryReceipts.Count(IsApplied);
        var verified = execution.ReferenceReceipts.All(IsVerified)
            && execution.LibraryReceipts.All(IsVerified)
            && applied == plan.Effects.Count
                + plan.LibrarySteps.Count(step => step.Effect is not null);
        var application = execution.UnexpectedFailure is { } unexpected
            ? new RepairApplication(RepairApplicationState.Failed, applied, unexpected.Cause)
            : execution.Cancellation is not null
                ? new RepairApplication(
                    RepairApplicationState.Interrupted,
                    applied,
                    "Repair execution was interrupted.")
                : verified
                    ? RepairApplicationOutcomeFactory.Applied(applied)
                    : new RepairApplication(RepairApplicationState.NotStarted, applied, cause: null);
        var coverage = execution.PostDiagnosis is { } diagnosis
            ? RepairCoverageMapper.Read(diagnosis.Observation)
            : null;
        var referenceVerification = execution.ReferencePostVerification?.Verification;
        var verification = new RepairVerification(
            verified && referenceVerification?.Targets is not RepairVerificationState.Failed
                and not RepairVerificationState.Unknown
                ? RepairVerificationState.Verified
                : referenceVerification?.Targets ?? RepairVerificationState.Planned,
            verified && referenceVerification?.ResultingBytes is not RepairVerificationState.Failed
                and not RepairVerificationState.Unknown
                ? RepairVerificationState.Verified
                : referenceVerification?.ResultingBytes ?? RepairVerificationState.Planned,
            referenceVerification?.PostConditions
                ?? (coverage is null
                    ? RepairVerificationState.Planned
                    : coverage.SelectedScope == RepairCoverageState.Complete
                        ? RepairVerificationState.Verified
                        : RepairVerificationState.Failed),
            referenceVerification?.Effects ?? []);
        var postDiagnosis = execution.ReferencePostVerification?.Diagnosis
            ?? (execution.PostDiagnosis is { } read && coverage is { } observedCoverage
                ? new RepairPostDiagnosis(
                    RepairCoverageMapper.ReadPostDiagnosisState(observedCoverage),
                    observedCoverage,
                    RepairRemainingFindingReader.Read(read.Observation.LocalReferences))
                : RepairPostDiagnosis.NotRequested);
        var recovery = ReadForwardRecovery(plan, execution);
        var findings = new List<RepairFinding>();
        if (execution.UnexpectedFailure is { } executionFailure)
        {
            findings.Add(new RepairFinding(RepairFindingCode.OperationFailed, executionFailure.Cause));
        }
        else if (execution.Cancellation is not null)
        {
            findings.Add(new RepairFinding(RepairFindingCode.Interrupted, "Repair execution was interrupted."));
        }

        if (execution.ForwardCleanup?.Disposition == RecoveryBundleDisposition.Retained)
        {
            findings.Add(new RepairFinding(
                RepairFindingCode.RecoveryArtifactRetained,
                execution.ForwardCleanup.Cause ?? "The forward Repair recovery final was retained."));
        }
        else if (execution.ForwardCleanup?.Disposition == RecoveryBundleDisposition.Unknown)
        {
            findings.Add(new RepairFinding(
                RepairFindingCode.RecoveryFailed,
                execution.ForwardCleanup.Cause ?? "The forward Repair recovery final disposition is unknown."));
        }

        return new RepairApplicationOutcome(
            RepairApplicationOutcomeFactory.Ready(execution.ForwardPreparation is null
                ? RepairRecoveryState.NotRequired
                : RepairRecoveryState.Prepared),
            application,
            verification,
            recovery,
            postDiagnosis,
            findings)
        {
            LibraryExecution = execution,
        };
    }

    private static RepairRecovery ReadForwardRecovery(
        RepairPlan plan,
        RepairLibraryExecution execution)
    {
        var attribution = execution.ForwardPreparation?.Attribution
            ?? RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Repair,
                RecoveryBundleOperation.Repair,
                plan.Request.Workspace);
        if (execution.ForwardCleanup is { } cleanup)
        {
            return RepairRecoveryLifecycle.ReadDeletion(cleanup, attribution);
        }

        return execution.ForwardPreparation is { } preparation
            ? new RepairRecovery(
                RepairRecoveryState.Prepared,
                RepairResidualState.Retained,
                preparation.BundlePath,
                attribution)
            : RepairRecovery.NotRequired;
    }

    private static async ValueTask<bool> MatchesObservedEntryAsync(
        RepairLibraryRecoveryStep step,
        CancellationToken cancellationToken)
    {
        var evidence = step.Selection.Proposal.Evidence;
        var observed = await RecoveryEntryObservationReader.ReadAsync(
            new PhysicalPathResolver(),
            evidence.Entry.Input.Context,
            cancellationToken).ConfigureAwait(false);
        return RecoveryEntryComparer.Compare(observed).State == evidence.Entry.State;
    }

    private static bool IsApplied(RepairLibraryRecoveryReceipt receipt)
        => receipt.Kind switch
        {
            RepairLibraryRecoveryKind.Ordinary =>
                receipt.Ordinary?.Effect == FilesystemEffectState.Applied,
            RepairLibraryRecoveryKind.RelativeFileLink =>
                receipt.RelativeFileLink?.State == RelativeFileLinkRecoveryState.Restored,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.Kind, "The Library recovery kind is not defined."),
        };

    private static bool IsVerified(RepairLibraryRecoveryReceipt receipt)
        => receipt.Kind switch
        {
            RepairLibraryRecoveryKind.Ordinary => receipt.Ordinary is
            {
                Effect: FilesystemEffectState.Applied,
                Verification: FilesystemVerificationState.Verified,
            },
            RepairLibraryRecoveryKind.RelativeFileLink =>
                receipt.RelativeFileLink?.State == RelativeFileLinkRecoveryState.Restored,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.Kind, "The Library recovery kind is not defined."),
        };

    private static bool IsVerified(FileChangeReceipt receipt)
        => receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified;

    private static bool IsCancelled(RepairLibraryRecoveryReceipt receipt)
        => receipt.RelativeFileLink?.State == RelativeFileLinkRecoveryState.Cancelled;

    private static string Cause(RepairLibraryRecoveryReceipt receipt)
        => receipt.Ordinary?.Cause
            ?? receipt.RelativeFileLink?.Cause
            ?? "A selected Library inverse effect did not verify.";

    private static RepairApplicationOutcome Boundary(
        RepairPreflightState state,
        RepairFindingCode code,
        string cause)
        => RepairApplicationOutcomeFactory.BeforeApplication(state, cause, code) with
        {
            LibraryExecution = EmptyExecution(),
        };

    private static RepairApplicationOutcome Cancelled(
        RepairLibraryExecutionStage stage,
        string cause)
        => new(
            new RepairPreflight(RepairPreflightState.Incomplete, cause, RepairRecoveryState.NotCreated),
            new RepairApplication(RepairApplicationState.Interrupted, 0, cause),
            RepairVerification.NotRequested,
            new RepairRecovery(RepairRecoveryState.NotCreated, RepairResidualState.None, null, null),
            RepairPostDiagnosis.NotRequested,
            [new RepairFinding(RepairFindingCode.Interrupted, cause)])
        {
            LibraryExecution = EmptyExecution() with
            {
                Cancellation = new RepairLibraryCancellation(stage, null),
            },
        };

    private static RepairApplicationOutcome Failed(
        RepairLibraryExecutionStage stage,
        string cause)
        => new(
            new RepairPreflight(RepairPreflightState.Incomplete, cause, RepairRecoveryState.NotCreated),
            new RepairApplication(RepairApplicationState.Failed, 0, cause),
            RepairVerification.NotRequested,
            new RepairRecovery(RepairRecoveryState.NotCreated, RepairResidualState.None, null, null),
            RepairPostDiagnosis.NotRequested,
            [new RepairFinding(RepairFindingCode.OperationFailed, cause)])
        {
            LibraryExecution = EmptyExecution() with
            {
                UnexpectedFailure = new RepairLibraryUnexpectedFailure(stage, null, cause),
            },
        };

    private static RepairLibraryExecution EmptyExecution()
        => new()
        {
            ReferenceReceipts = [],
            LibraryReceipts = [],
            ForwardPreparation = null,
            ForwardCleanup = null,
            PostDiagnosis = null,
            ReferencePostVerification = null,
            Cancellation = null,
            UnexpectedFailure = null,
        };

    private sealed record AtomicPreparationResult(
        RepairLibraryPreparedApplication? Prepared,
        RepairApplicationOutcome? Boundary)
    {
        internal static AtomicPreparationResult Ready(RepairLibraryPreparedApplication prepared)
            => new(prepared, Boundary: null);

        internal static AtomicPreparationResult BoundaryResult(RepairApplicationOutcome boundary)
            => new(Prepared: null, boundary);
    }
}
