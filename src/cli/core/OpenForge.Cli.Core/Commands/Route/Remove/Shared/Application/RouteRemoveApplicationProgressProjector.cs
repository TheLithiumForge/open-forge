using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal static class RouteRemoveApplicationProgressProjector
{
    internal static RouteRemoveApplicationProgress LockBoundary(
        RouteRemovePlan plan,
        WorkspaceLockResult result)
    {
        var boundary = result.State switch
        {
            WorkspaceLockState.Cancelled => (
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted),
            WorkspaceLockState.Failed => (
                RouteRemoveFindingCode.WorkspaceLockUnavailable,
                CliSemanticStatus.Blocked),
            WorkspaceLockState.Acquired => (
                RouteRemoveFindingCode.OperationFailed,
                CliSemanticStatus.Failed),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result), result.State, "The workspace lock state is not defined."),
        };
        return BeforeApplication(
            plan,
            boundary.Item1,
            boundary.Item2,
            result.Cause ?? "The persistent Route Remove workspace lease could not be acquired.");
    }

    internal static RouteRemoveApplicationProgress RevalidationBoundary(
        RouteRemovePlan plan,
        RouteRemovePlanRevalidation result)
    {
        var boundary = result.State switch
        {
            RouteRemovePlanRevalidationState.Changed => (
                RouteRemoveFindingCode.TargetChanged,
                CliSemanticStatus.Blocked),
            RouteRemovePlanRevalidationState.Failed => (
                RouteRemoveFindingCode.OperationFailed,
                CliSemanticStatus.Failed),
            RouteRemovePlanRevalidationState.Interrupted => (
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted),
            RouteRemovePlanRevalidationState.Exact => throw new ArgumentOutOfRangeException(
                nameof(result), result.State, "Exact revalidation has no boundary."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result), result.State, "The revalidation state is not defined."),
        };
        return BeforeApplication(
            plan,
            boundary.Item1,
            boundary.Item2,
            result.Cause ?? "The complete Route Remove plan changed before application.");
    }

    internal static RouteRemoveApplicationProgress PreparationBoundary(
        RouteRemovePlan plan,
        RouteRemoveRecoveryPreparationResult result)
    {
        if (result.Finding is { } finding)
        {
            return new RouteRemoveApplicationProgress
            {
                Recovery = result.Recovery,
                Verification = RouteRemoveVerificationState.NotRequested,
                Findings = [finding],
            };
        }

        return BeforeApplication(
            plan,
            RouteRemoveFindingCode.OperationFailed,
            CliSemanticStatus.Failed,
            "Route Remove recovery preparation returned no decisive finding.");
    }

    internal static RouteRemoveApplicationProgress VerificationBoundary(
        RouteRemovePlan plan,
        RouteRemoveApplicationProgress application,
        RouteRemoveAppliedVerification result)
    {
        var interrupted = result.State == RouteRemoveAppliedVerificationState.Interrupted;
        return application with
        {
            Verification = interrupted
                ? RouteRemoveVerificationState.Unknown
                : RouteRemoveVerificationState.Failed,
            Findings =
            [
                Finding(
                    plan,
                    interrupted
                        ? RouteRemoveFindingCode.Interrupted
                        : RouteRemoveFindingCode.VerificationFailed,
                    interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
                    result.Cause ?? "Final Route Remove verification did not complete."),
            ],
        };
    }

    internal static RouteRemoveApplicationProgress BeforeApplication(
        RouteRemovePlan plan,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string cause)
        => new()
        {
            Recovery = plan.Preview.Recovery,
            Verification = RouteRemoveVerificationState.NotRequested,
            Findings = [Finding(plan, code, status, cause)],
        };

    internal static RouteRemoveApplicationProgress LockReleaseFailed(
        RouteRemovePlan plan,
        RouteRemoveApplicationProgress progress)
        => progress with
        {
            Findings =
            [
                .. progress.Findings,
                Finding(
                    plan,
                    RouteRemoveFindingCode.OperationFailed,
                    CliSemanticStatus.Failed,
                    "Route Remove workspace lock release failed unexpectedly."),
            ],
        };

    internal static RouteRemoveApplicationProgress UnexpectedAfterApplication(
        RouteRemovePlan plan,
        RouteRemoveApplicationProgress progress,
        bool interrupted,
        string cause)
        => progress with
        {
            Verification = progress.Verification == RouteRemoveVerificationState.Verified
                ? RouteRemoveVerificationState.Verified
                : RouteRemoveVerificationState.Unknown,
            Findings =
            [
                .. progress.Findings,
                Finding(
                    plan,
                    interrupted
                        ? RouteRemoveFindingCode.Interrupted
                        : RouteRemoveFindingCode.OperationFailed,
                    interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
                    cause),
            ],
        };

    internal static RouteRemoveApplicationProgress UnexpectedAfterPreparation(
        RouteRemovePlan plan,
        RouteRemoveRecoveryPreparationResult preparation,
        bool interrupted)
    {
        var prepared = preparation.Preparation
            ?? throw new InvalidOperationException(
                "Unexpected post-preparation failure requires the prepared recovery identity.");
        return new RouteRemoveApplicationProgress
        {
            Recovery = new RouteRemoveRecovery
            {
                State = RouteRemoveRecoveryState.Retained,
                ProtectedPaths = plan.Preview.Recovery.ProtectedPaths,
                ResidualPath = prepared.BundlePath,
            },
            Verification = RouteRemoveVerificationState.Unknown,
            Findings =
            [
                Finding(
                    plan,
                    interrupted ? RouteRemoveFindingCode.Interrupted : RouteRemoveFindingCode.OperationFailed,
                    interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
                    interrupted
                        ? "Route Remove application was interrupted after recovery preparation."
                        : "Route Remove application failed after recovery preparation."),
            ],
        };
    }

    private static RouteRemoveFinding Finding(
        RouteRemovePlan plan,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string cause)
        => new(code, status, plan.Preview.Source.Path, cause);
}
