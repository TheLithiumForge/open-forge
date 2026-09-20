using OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;

internal sealed class RouteCreateApplicationOperation
{
    private readonly WorkspaceLockManager _lockManager;
    private readonly MutationRevalidator _mutationRevalidator;
    private readonly RouteCreatePlanRevalidator _planRevalidator;
    private readonly RouteCreateEffectApplication _effectApplication;
    private readonly RouteCreateAppliedVerifier _appliedVerifier;

    internal RouteCreateApplicationOperation(
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        var pathResolver = new PhysicalPathResolver();
        var expectationValidator = new FileExpectationValidator(pathResolver);
        _mutationRevalidator = new MutationRevalidator(expectationValidator);
        _lockManager = lockStoreRoot is null
            ? WorkspaceLockManager.CreateForCurrentUser()
            : new WorkspaceLockManager(lockStoreRoot);
        var planBuilder = new RouteCreatePlanBuilder();
        _planRevalidator = new RouteCreatePlanRevalidator(planBuilder);
        _effectApplication = new RouteCreateEffectApplication(
            new DirectoryCreationApplier(_mutationRevalidator, expectationValidator),
            new FileChangeApplier(_mutationRevalidator, expectationValidator));
        _appliedVerifier = new RouteCreateAppliedVerifier(
            planBuilder,
            expectationValidator);
    }

    internal async ValueTask<RouteCreateResultFormation> ExecuteAsync(
        RouteCreatePlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (plan.IsNoOp)
        {
            return plan.Preview;
        }

        var operationId = Guid.NewGuid();
        var lockRequest = new WorkspaceLockRequest(
            plan.Request.Workspace,
            RouteCreateDefinitions.CommandIdentity,
            operationId);
        WorkspaceLockResult lockResult;
        try
        {
            lockResult = await _lockManager.AcquireAsync(lockRequest, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure(
                plan,
                RouteCreateFindingCode.Interrupted,
                "Route Create workspace lock acquisition was interrupted.");
        }
        catch (Exception)
        {
            return Failure(
                plan,
                RouteCreateFindingCode.OperationFailed,
                "Route Create workspace lock acquisition failed unexpectedly.");
        }

        if (lockResult.State != WorkspaceLockState.Acquired
            || lockResult.Lease is not { } lease)
        {
            var code = lockResult.State switch
            {
                WorkspaceLockState.Cancelled => RouteCreateFindingCode.Interrupted,
                WorkspaceLockState.Failed => RouteCreateFindingCode.WorkspaceLockUnavailable,
                WorkspaceLockState.Acquired => RouteCreateFindingCode.OperationFailed,
                _ => throw new ArgumentOutOfRangeException(
                    null,
                    lockResult.State,
                    "The workspace lock state is not defined."),
            };
            return Failure(
                plan,
                code,
                lockResult.Cause
                    ?? "The persistent Route Create workspace lease could not be acquired.");
        }

        await using (lease.ConfigureAwait(false))
        {
            return await ExecuteUnderLeaseAsync(
                    plan,
                    lease,
                    operationId,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async ValueTask<RouteCreateResultFormation> ExecuteUnderLeaseAsync(
        RouteCreatePlan plan,
        WorkspaceLockLease lease,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        var planRevalidation = await _planRevalidator.RevalidateAsync(
                plan,
                cancellationToken)
            .ConfigureAwait(false);
        if (planRevalidation.State != RouteCreatePlanRevalidationState.Exact)
        {
            return Failure(
                plan,
                RevalidationFinding(planRevalidation.State),
                planRevalidation.Cause
                    ?? "The complete Route Create plan changed before application.");
        }

        MutationValidationResult validation;
        try
        {
            validation = await _mutationRevalidator.ValidateAsync(
                    lease,
                    plan.DirectoryCreations,
                    plan.FileChanges,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure(
                plan,
                RouteCreateFindingCode.Interrupted,
                "Route Create whole-plan revalidation was interrupted.");
        }
        catch (Exception)
        {
            return Failure(
                plan,
                RouteCreateFindingCode.OperationFailed,
                "Route Create whole-plan revalidation failed unexpectedly.");
        }

        if (!HasExactChecks(plan, validation))
        {
            var (code, cause) = ValidationBoundary(validation);
            return Failure(plan, code, cause);
        }

        var recovery = await RouteCreateRecoveryLifecycle.PrepareAsync(
                plan,
                operationId,
                cancellationToken)
            .ConfigureAwait(false);
        if (recovery.State is not (RouteCreateRecoveryPreparationState.NotRequired
            or RouteCreateRecoveryPreparationState.Prepared))
        {
            return Failure(
                plan,
                PreparationFinding(recovery.State),
                recovery.Cause ?? "Route Create recovery preparation did not complete.",
                recovery.Recovery);
        }

        var application = await _effectApplication.ApplyAsync(
                plan,
                lease,
                validation,
                recovery.Preparation,
                cancellationToken)
            .ConfigureAwait(false);
        if (!application.Findings.IsEmpty)
        {
            return RouteCreateApplicationResultFactory.Build(plan, application);
        }

        var verification = await _appliedVerifier.VerifyAsync(
                plan,
                lease,
                application.DirectoryReceipts,
                application.Receipts,
                cancellationToken)
            .ConfigureAwait(false);
        if (verification.State != RouteCreateAppliedVerificationState.Verified)
        {
            return RouteCreateApplicationResultFactory.Build(
                plan,
                application with
                {
                    Verification = verification.State == RouteCreateAppliedVerificationState.Cancelled
                        ? RouteCreateVerificationState.Unknown
                        : RouteCreateVerificationState.Failed,
                    Findings =
                    [
                        RouteCreateEffectApplication.Finding(
                            plan,
                            verification.State == RouteCreateAppliedVerificationState.Cancelled
                                ? RouteCreateFindingCode.Interrupted
                                : RouteCreateFindingCode.VerificationFailed,
                            verification.Cause
                                ?? "Final Route Create verification did not complete."),
                    ],
                });
        }

        var deletion = recovery.Preparation is null
            ? new RouteCreateRecoveryDeletionResult
            {
                Recovery = RouteCreateEffectApplication.BaseRecovery(plan),
                FindingCode = null,
                Cause = null,
            }
            : await RouteCreateRecoveryLifecycle.DeleteAsync(
                    lease,
                    recovery.Preparation,
                    cancellationToken)
                .ConfigureAwait(false);
        return RouteCreateApplicationResultFactory.Build(
            plan,
            application with
            {
                Recovery = deletion.Recovery,
                Verification = RouteCreateVerificationState.Verified,
                Findings = deletion.FindingCode is { } findingCode
                    ?
                    [
                        RouteCreateEffectApplication.Finding(
                            plan,
                            findingCode,
                            deletion.Cause
                                ?? "Route Create recovery deletion did not complete."),
                    ]
                    : [],
            });
    }

    private static bool HasExactChecks(
        RouteCreatePlan plan,
        MutationValidationResult validation)
    {
        if (validation.State != MutationValidationState.Valid
            || validation.Checks.Count != plan.DirectoryCreations.Length + plan.FileChanges.Length)
        {
            return false;
        }

        var checkIndex = 0;
        foreach (var creation in plan.DirectoryCreations)
        {
            var check = validation.Checks[checkIndex++];
            var expectation = creation.Expectation;
            if (check.State != FileExpectationValidationState.Matched
                || check.Expectation != expectation
                || check.Actual?.Expectation != expectation)
            {
                return false;
            }
        }

        foreach (var change in plan.FileChanges)
        {
            var check = validation.Checks[checkIndex++];
            var expectation = change.Expectation;
            if (check.State != FileExpectationValidationState.Matched
                || check.Expectation != expectation
                || check.Actual?.Expectation != expectation)
            {
                return false;
            }
        }

        return true;
    }

    private static RouteCreateFindingCode RevalidationFinding(
        RouteCreatePlanRevalidationState state)
        => state switch
        {
            RouteCreatePlanRevalidationState.Changed => RouteCreateFindingCode.TargetChanged,
            RouteCreatePlanRevalidationState.Cancelled => RouteCreateFindingCode.Interrupted,
            RouteCreatePlanRevalidationState.Failed => RouteCreateFindingCode.OperationFailed,
            RouteCreatePlanRevalidationState.Exact => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "An exact Route Create plan revalidation has no finding."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Route Create plan revalidation state is not defined."),
        };

    private static (RouteCreateFindingCode Code, string Cause) ValidationBoundary(
        MutationValidationResult validation)
        => validation.State switch
        {
            MutationValidationState.Mismatched => (
                RouteCreateFindingCode.TargetChanged,
                validation.Cause
                    ?? "A planned Route Create target changed before application."),
            MutationValidationState.Blocked => (
                RouteCreateFindingCode.TargetUnsafe,
                validation.Cause
                    ?? "A planned Route Create target became unsafe before application."),
            MutationValidationState.Failed => (
                RouteCreateFindingCode.InspectionIncomplete,
                validation.Cause
                    ?? "A planned Route Create target could not be revalidated completely."),
            MutationValidationState.Cancelled => (
                RouteCreateFindingCode.Interrupted,
                "Route Create whole-plan revalidation was interrupted."),
            MutationValidationState.Valid => (
                RouteCreateFindingCode.OperationFailed,
                "Route Create whole-plan revalidation returned incoherent checks."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(validation),
                validation.State,
                "The mutation validation state is not defined."),
        };

    private static RouteCreateFindingCode PreparationFinding(
        RouteCreateRecoveryPreparationState state)
        => state switch
        {
            RouteCreateRecoveryPreparationState.Incomplete =>
                RouteCreateFindingCode.RecoveryUnavailable,
            RouteCreateRecoveryPreparationState.Blocked => RouteCreateFindingCode.RecoveryConflict,
            RouteCreateRecoveryPreparationState.Cancelled => RouteCreateFindingCode.Interrupted,
            RouteCreateRecoveryPreparationState.Failed => RouteCreateFindingCode.OperationFailed,
            RouteCreateRecoveryPreparationState.NotRequired
                or RouteCreateRecoveryPreparationState.Prepared =>
                throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "A completed recovery preparation has no finding."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Route Create recovery preparation state is not defined."),
        };

    private static RouteCreateResultFormation Failure(
        RouteCreatePlan plan,
        RouteCreateFindingCode code,
        string cause,
        RouteCreateRecovery? recovery = null)
        => RouteCreateApplicationResultFactory.Build(
            plan,
            new RouteCreateApplicationProgress
            {
                DirectoryReceipts = [],
                UncertainDirectoryAttempt = null,
                Receipts = [],
                UncertainAttempt = null,
                Recovery = recovery ?? RouteCreateEffectApplication.BaseRecovery(plan),
                Verification = RouteCreateVerificationState.NotRequested,
                Findings = [RouteCreateEffectApplication.Finding(plan, code, cause)],
            });
}
