using OpenForge.Cli.Core.Commands.Route.Init.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;

internal sealed class RouteInitApplicationOperation
{
    internal RouteInitApplicationOperation(
        WorkspaceLockStoreRoot? lockStoreRoot,
        RouteInitDirectoryCreationObserver? directoryCreationObserver = null)
    {
        LockStoreRoot = lockStoreRoot;
        _directoryCreationObserver = directoryCreationObserver;
        var physicalPathResolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physicalPathResolver);
        var revalidator = new MutationRevalidator(validator);
        _lockManager = lockStoreRoot is null
            ? WorkspaceLockManager.CreateForCurrentUser()
            : new WorkspaceLockManager(lockStoreRoot);
        _directoryApplier = new DirectoryCreationApplier(revalidator, validator);
        _fileApplier = new FileChangeApplier(revalidator, validator);
        _revalidator = revalidator;
        _planBuilder = new RouteInitPlanBuilder();
        _planRevalidator = new RouteInitPlanRevalidator(_planBuilder);
        _verifier = new RouteInitAppliedVerifier(_planBuilder, validator);
        _recoveryLifecycle = new RouteInitRecoveryLifecycle();
    }

    internal WorkspaceLockStoreRoot? LockStoreRoot { get; }

    private readonly WorkspaceLockManager _lockManager;
    private readonly RouteInitDirectoryCreationObserver? _directoryCreationObserver;
    private readonly MutationRevalidator _revalidator;
    private readonly DirectoryCreationApplier _directoryApplier;
    private readonly FileChangeApplier _fileApplier;
    private readonly RouteInitPlanBuilder _planBuilder;
    private readonly RouteInitPlanRevalidator _planRevalidator;
    private readonly RouteInitAppliedVerifier _verifier;
    private readonly RouteInitRecoveryLifecycle _recoveryLifecycle;

    internal async ValueTask<RouteInitApplicationOutcome> ExecuteAsync(
        RouteInitPlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var directories = new List<DirectoryCreationReceipt>();
        var files = new List<FileChangeReceipt>();
        var operationId = Guid.NewGuid();
        var lockRequest = new WorkspaceLockRequest(
            plan.Request.Workspace,
            RouteInitDefinitions.CommandIdentity,
            operationId);
        WorkspaceLockResult lockResult;
        try
        {
            lockResult = await _lockManager.AcquireAsync(lockRequest, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Finish(
                plan,
                directories,
                files,
                NotCreatedRecovery(),
                RouteInitVerificationState.NotRequested,
                RouteInitFindingCode.Interrupted,
                "Route Init workspace lock acquisition was interrupted.");
        }
        catch (Exception)
        {
            return Finish(
                plan,
                directories,
                files,
                NotCreatedRecovery(),
                RouteInitVerificationState.NotRequested,
                RouteInitFindingCode.OperationFailed,
                "Route Init workspace lock acquisition failed unexpectedly.");
        }

        if (lockResult.State != WorkspaceLockState.Acquired
            || lockResult.Lease is not { } lease)
        {
            return Finish(
                plan,
                directories,
                files,
                NotCreatedRecovery(),
                RouteInitVerificationState.NotRequested,
                lockResult.State == WorkspaceLockState.Cancelled
                    ? RouteInitFindingCode.Interrupted
                    : RouteInitFindingCode.WorkspaceLockUnavailable,
                lockResult.Cause ?? "The persistent Route Init workspace lease could not be acquired.");
        }

        await using (lease.ConfigureAwait(false))
        {
            return await ExecuteUnderLeaseAsync(
                    plan,
                    lease,
                    operationId,
                    directories,
                    files,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async ValueTask<RouteInitApplicationOutcome> ExecuteUnderLeaseAsync(
        RouteInitPlan plan,
        WorkspaceLockLease lease,
        Guid operationId,
        List<DirectoryCreationReceipt> directories,
        List<FileChangeReceipt> files,
        CancellationToken cancellationToken)
    {
        var planRevalidation = await _planRevalidator.RevalidateAsync(plan, cancellationToken)
            .ConfigureAwait(false);
        if (planRevalidation.State != RouteInitPlanRevalidationState.Exact)
        {
            var code = planRevalidation.State switch
            {
                RouteInitPlanRevalidationState.Changed => RouteInitFindingCode.TargetChanged,
                RouteInitPlanRevalidationState.Cancelled => RouteInitFindingCode.Interrupted,
                RouteInitPlanRevalidationState.Failed => RouteInitFindingCode.OperationFailed,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(planRevalidation),
                    planRevalidation.State,
                    "The Route Init plan revalidation state is not defined."),
            };
            return Finish(
                plan,
                directories,
                files,
                NotCreatedRecovery(),
                RouteInitVerificationState.NotRequested,
                code,
                planRevalidation.Cause ?? "The complete Route Init plan changed before application.");
        }

        MutationValidationResult validation;
        try
        {
            validation = await _revalidator.ValidateAsync(
                    lease,
                    plan.DirectoryCreations,
                    plan.FileChanges,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Finish(
                plan,
                directories,
                files,
                NotCreatedRecovery(),
                RouteInitVerificationState.NotRequested,
                RouteInitFindingCode.Interrupted,
                "Route Init whole-plan revalidation was interrupted.");
        }
        catch (Exception)
        {
            return Finish(
                plan,
                directories,
                files,
                NotCreatedRecovery(),
                RouteInitVerificationState.NotRequested,
                RouteInitFindingCode.OperationFailed,
                "Route Init whole-plan revalidation failed unexpectedly.");
        }

        if (!HasExactChecks(plan, validation))
        {
            var (code, cause) = ReadValidationBoundary(
                validation.State,
                validation.Cause);
            return Finish(
                plan,
                directories,
                files,
                NotCreatedRecovery(),
                RouteInitVerificationState.NotRequested,
                code,
                cause);
        }

        var preparation = await _recoveryLifecycle.PrepareAsync(
                plan,
                operationId,
                cancellationToken)
            .ConfigureAwait(false);
        if (preparation.State is not (RouteInitRecoveryPreparationState.NotRequired
            or RouteInitRecoveryPreparationState.Prepared))
        {
            return Finish(
                plan,
                directories,
                files,
                preparation.Recovery,
                RouteInitVerificationState.NotRequested,
                ReadPreparationFinding(preparation.State),
                preparation.Cause ?? "Route Init recovery preparation did not complete.");
        }

        var checkIndex = 0;
        foreach (var creation in plan.DirectoryCreations)
        {
            DirectoryCreationReceipt receipt;
            var attempt = new RouteInitApplicationAttempt(
                RouteInitApplicationAttemptKind.Directory,
                creation.LogicalPath);
            try
            {
                receipt = await _directoryApplier.ApplyAsync(
                        lease,
                        creation,
                        validation.Checks[checkIndex++],
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return FinishPreparedFailure(
                    plan,
                    directories,
                    files,
                    preparation.Preparation,
                    RouteInitFindingCode.Interrupted,
                    "Route Init directory application was interrupted.",
                    attempt);
            }
            catch (Exception)
            {
                return FinishPreparedFailure(
                    plan,
                    directories,
                    files,
                    preparation.Preparation,
                    RouteInitFindingCode.OperationFailed,
                    "Route Init directory application failed unexpectedly.",
                    attempt);
            }

            directories.Add(receipt);
            if (!IsVerified(receipt))
            {
                var (code, cause) = ReadReceiptBoundary(receipt);
                return FinishPreparedFailure(
                    plan,
                    directories,
                    files,
                    preparation.Preparation,
                    code,
                    cause);
            }

            _directoryCreationObserver?.Invoke(receipt);
        }

        foreach (var change in plan.FileChanges)
        {
            FileChangeReceipt receipt;
            var attempt = new RouteInitApplicationAttempt(
                RouteInitApplicationAttemptKind.File,
                change.LogicalPath);
            try
            {
                receipt = await _fileApplier.ApplyAsync(
                        lease,
                        change,
                        validation.Checks[checkIndex++],
                        change.Kind == PlannedFileChangeKind.Create
                            ? null
                            : preparation.Preparation,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return FinishPreparedFailure(
                    plan,
                    directories,
                    files,
                    preparation.Preparation,
                    RouteInitFindingCode.Interrupted,
                    "Route Init file application was interrupted.",
                    attempt);
            }
            catch (Exception)
            {
                return FinishPreparedFailure(
                    plan,
                    directories,
                    files,
                    preparation.Preparation,
                    RouteInitFindingCode.OperationFailed,
                    "Route Init file application failed unexpectedly.",
                    attempt);
            }

            files.Add(receipt);
            if (!IsVerified(receipt))
            {
                var (code, cause) = ReadReceiptBoundary(plan, receipt);
                return FinishPreparedFailure(
                    plan,
                    directories,
                    files,
                    preparation.Preparation,
                    code,
                    cause);
            }
        }

        var verification = await _verifier.VerifyAsync(
                plan,
                lease,
                directories,
                files,
                cancellationToken)
            .ConfigureAwait(false);
        if (verification.State != RouteInitAppliedVerificationState.Verified)
        {
            return Finish(
                plan,
                directories,
                files,
                RecoveryAfterPreparation(preparation.Preparation),
                verification.State == RouteInitAppliedVerificationState.Cancelled
                    ? RouteInitVerificationState.Unknown
                    : RouteInitVerificationState.Failed,
                verification.State == RouteInitAppliedVerificationState.Cancelled
                    ? RouteInitFindingCode.Interrupted
                    : RouteInitFindingCode.VerificationFailed,
                verification.Cause ?? "Final Route Init verification did not complete.");
        }

        var deletion = await _recoveryLifecycle.DeleteAsync(
                lease,
                preparation.Preparation,
                cancellationToken)
            .ConfigureAwait(false);
        return RouteInitApplicationResultFactory.Build(
            plan,
            directories,
            files,
            deletion.Recovery,
            RouteInitVerificationState.Verified,
            deletion.FindingCode is { } findingCode
                ? RouteInitApplicationResultFactory.Finding(
                    plan,
                    findingCode,
                    deletion.Cause ?? "Route Init recovery deletion did not complete.")
                : null,
            uncertainAttempt: null);
    }

    private static bool HasExactChecks(
        RouteInitPlan plan,
        MutationValidationResult validation)
    {
        if (validation.State != MutationValidationState.Valid
            || validation.Checks.Count != plan.DirectoryCreations.Length + plan.FileChanges.Length)
        {
            return false;
        }

        var index = 0;
        foreach (var creation in plan.DirectoryCreations)
        {
            if (!Matches(validation.Checks[index++], creation.Expectation))
            {
                return false;
            }
        }

        foreach (var change in plan.FileChanges)
        {
            if (!Matches(validation.Checks[index++], change.Expectation))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Matches(
        FileExpectationValidationResult check,
        FileExpectation expectation)
        => check.State == FileExpectationValidationState.Matched
            && check.Expectation == expectation
            && check.Actual?.Expectation == expectation;

    internal static (RouteInitFindingCode Code, string Cause) ReadValidationBoundary(
        MutationValidationState state,
        string? cause)
        => state switch
        {
            MutationValidationState.Valid => (
                RouteInitFindingCode.OperationFailed,
                "Route Init whole-plan revalidation returned incoherent checks."),
            MutationValidationState.Mismatched => (
                RouteInitFindingCode.TargetChanged,
                cause ?? "A planned Route Init target changed before application."),
            MutationValidationState.Blocked => (
                RouteInitFindingCode.TargetUnsafe,
                cause ?? "A planned Route Init target became unsafe before application."),
            MutationValidationState.Failed => (
                RouteInitFindingCode.InspectionIncomplete,
                cause ?? "A planned Route Init target could not be revalidated completely."),
            MutationValidationState.Cancelled => (
                RouteInitFindingCode.Interrupted,
                "Route Init whole-plan revalidation was interrupted."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The mutation validation state is not defined."),
        };

    internal static RouteInitFindingCode ReadPreparationFinding(
        RouteInitRecoveryPreparationState state)
        => state switch
        {
            RouteInitRecoveryPreparationState.NotRequired
                or RouteInitRecoveryPreparationState.Prepared
                or RouteInitRecoveryPreparationState.Failed => RouteInitFindingCode.OperationFailed,
            RouteInitRecoveryPreparationState.Incomplete => RouteInitFindingCode.RecoveryUnavailable,
            RouteInitRecoveryPreparationState.Blocked => RouteInitFindingCode.RecoveryConflict,
            RouteInitRecoveryPreparationState.Cancelled => RouteInitFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Route Init recovery preparation state is not defined."),
        };

    private static bool IsVerified(DirectoryCreationReceipt receipt)
        => receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified;

    private static bool IsVerified(FileChangeReceipt receipt)
        => receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified;

    private static (RouteInitFindingCode Code, string Cause) ReadReceiptBoundary(
        DirectoryCreationReceipt receipt)
        => (
            ReadReceiptFinding(receipt.EffectState, receipt.VerificationState, receipt.NotStartedReason),
            receipt.Cause ?? "A planned Route Init directory could not be created and verified.");

    private static (RouteInitFindingCode Code, string Cause) ReadReceiptBoundary(
        RouteInitPlan plan,
        FileChangeReceipt receipt)
    {
        var code = ReadReceiptFinding(
            receipt.EffectState,
            receipt.VerificationState,
            receipt.NotStartedReason);
        if (IsLifecyclePath(plan, receipt.Change.LogicalPath)
            && receipt.EffectState != FilesystemEffectState.NotStarted)
        {
            code = RouteInitFindingCode.LifecyclePublicationFailed;
        }

        return (
            code,
            receipt.Cause ?? "A planned Route Init file could not be applied and verified.");
    }

    internal static RouteInitFindingCode ReadReceiptFinding(
        FilesystemEffectState effect,
        FilesystemVerificationState verification,
        FilesystemNotStartedReason? reason)
    {
        _ = effect switch
        {
            FilesystemEffectState.NotStarted
                or FilesystemEffectState.Applied
                or FilesystemEffectState.Unknown => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(effect),
                effect,
                "The filesystem effect state is not defined."),
        };
        _ = verification switch
        {
            FilesystemVerificationState.NotStarted
                or FilesystemVerificationState.Verified
                or FilesystemVerificationState.Failed => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(verification),
                verification,
                "The filesystem verification state is not defined."),
        };
        _ = reason switch
        {
            null
                or FilesystemNotStartedReason.Cancelled
                or FilesystemNotStartedReason.TargetChanged
                or FilesystemNotStartedReason.ApplicationFailed
                or FilesystemNotStartedReason.ContractRejected => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "The filesystem not-started reason is not defined."),
        };

        return (effect, verification, reason) switch
        {
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.Cancelled) => RouteInitFindingCode.Interrupted,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.TargetChanged) => RouteInitFindingCode.TargetChangedDuringApply,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ApplicationFailed) => RouteInitFindingCode.WriteFailed,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ContractRejected) => RouteInitFindingCode.OperationFailed,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed, null) =>
                RouteInitFindingCode.VerificationFailed,
            (FilesystemEffectState.Unknown, FilesystemVerificationState.NotStarted, null) =>
                RouteInitFindingCode.WriteFailed,
            _ => RouteInitFindingCode.OperationFailed,
        };
    }

    private static bool IsLifecyclePath(RouteInitPlan plan, string logicalPath)
        => string.Equals(
            Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, logicalPath)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/'),
            LifecycleSchema.RelativePath,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private static RouteInitApplicationOutcome FinishPreparedFailure(
        RouteInitPlan plan,
        IReadOnlyList<DirectoryCreationReceipt> directories,
        IReadOnlyList<FileChangeReceipt> files,
        RecoveryBundlePreparation? preparation,
        RouteInitFindingCode code,
        string cause,
        RouteInitApplicationAttempt? uncertainAttempt = null)
        => Finish(
            plan,
            directories,
            files,
            RecoveryAfterPreparation(preparation),
            uncertainAttempt is not null || directories.Count != 0 || files.Count != 0
                ? RouteInitVerificationState.Unknown
                : RouteInitVerificationState.NotRequested,
            code,
            cause,
            uncertainAttempt);

    private static RouteInitApplicationOutcome Finish(
        RouteInitPlan plan,
        IReadOnlyList<DirectoryCreationReceipt> directories,
        IReadOnlyList<FileChangeReceipt> files,
        RouteInitRecovery recovery,
        RouteInitVerificationState verification,
        RouteInitFindingCode code,
        string cause,
        RouteInitApplicationAttempt? uncertainAttempt = null)
        => RouteInitApplicationResultFactory.Build(
            plan,
            directories,
            files,
            recovery,
            verification,
            RouteInitApplicationResultFactory.Finding(plan, code, cause),
            uncertainAttempt);

    private static RouteInitRecovery RecoveryAfterPreparation(
        RecoveryBundlePreparation? preparation)
        => preparation is null
            ? NotCreatedRecovery()
            : new RouteInitRecovery(RouteInitRecoveryState.Retained, preparation.BundlePath);

    private static RouteInitRecovery NotCreatedRecovery()
        => new(RouteInitRecoveryState.NotCreated, ResidualPath: null);
}
