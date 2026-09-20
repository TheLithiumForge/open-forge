using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal static class RouteMoveApplicationProgressProjector
{
    internal static RouteMoveApplicationProgress LockBoundary(
        RouteMovePlan plan,
        WorkspaceLockResult result)
    {
        var boundary = result.State switch
        {
            WorkspaceLockState.Cancelled => (
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted),
            WorkspaceLockState.Failed => (
                RouteMoveFindingCode.WorkspaceLockUnavailable,
                CliSemanticStatus.Blocked),
            WorkspaceLockState.Acquired => (
                RouteMoveFindingCode.OperationFailed,
                CliSemanticStatus.Failed),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result), result.State, "The workspace lock state is not defined."),
        };
        return BeforeApplication(
            plan,
            boundary.Item1,
            boundary.Item2,
            result.Cause ?? "The persistent Route Move workspace lease could not be acquired.");
    }

    internal static RouteMoveApplicationProgress RevalidationBoundary(
        RouteMovePlan plan,
        RouteMovePlanRevalidation result)
    {
        var boundary = result.State switch
        {
            RouteMovePlanRevalidationState.Changed => (
                RouteMoveFindingCode.TargetChanged,
                CliSemanticStatus.Blocked),
            RouteMovePlanRevalidationState.Failed => (
                RouteMoveFindingCode.OperationFailed,
                CliSemanticStatus.Failed),
            RouteMovePlanRevalidationState.Interrupted => (
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted),
            RouteMovePlanRevalidationState.Exact => throw new ArgumentOutOfRangeException(
                nameof(result), result.State, "Exact revalidation has no boundary."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result), result.State, "The revalidation state is not defined."),
        };
        return BeforeApplication(
            plan,
            boundary.Item1,
            boundary.Item2,
            result.Cause ?? "The complete Route Move plan changed before application.");
    }

    internal static RouteMoveApplicationProgress PreparationBoundary(
        RouteMovePlan plan,
        RouteMoveRecoveryPreparationResult result)
    {
        if (result.Finding is { } finding)
        {
            return new RouteMoveApplicationProgress
            {
                Recovery = result.Recovery,
                Verification = RouteMoveVerificationState.NotRequested,
                Findings = [finding],
            };
        }

        return BeforeApplication(
            plan,
            RouteMoveFindingCode.OperationFailed,
            CliSemanticStatus.Failed,
            "Route Move recovery preparation returned no decisive finding.");
    }

    internal static RouteMoveApplicationProgress VerificationBoundary(
        RouteMovePlan plan,
        RouteMoveApplicationProgress application,
        RouteMoveAppliedVerification result)
    {
        var interrupted = result.State == RouteMoveAppliedVerificationState.Interrupted;
        return application with
        {
            Verification = interrupted
                ? RouteMoveVerificationState.Unknown
                : RouteMoveVerificationState.Failed,
            Findings =
            [
                Finding(
                    plan,
                    interrupted
                        ? RouteMoveFindingCode.Interrupted
                        : RouteMoveFindingCode.VerificationFailed,
                    interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
                    result.Cause ?? "Final Route Move verification did not complete."),
            ],
        };
    }

    internal static RouteMoveApplicationProgress BeforeApplication(
        RouteMovePlan plan,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string cause)
        => new()
        {
            Recovery = plan.Preview.Recovery,
            Verification = RouteMoveVerificationState.NotRequested,
            Findings = [Finding(plan, code, status, cause)],
        };

    internal static RouteMoveApplicationProgress LockReleaseFailed(
        RouteMovePlan plan,
        RouteMoveApplicationProgress progress)
        => progress with
        {
            Findings =
            [
                .. progress.Findings,
                Finding(
                    plan,
                    RouteMoveFindingCode.OperationFailed,
                    CliSemanticStatus.Failed,
                    "Route Move workspace lock release failed unexpectedly."),
            ],
        };

    internal static RouteMoveApplicationProgress UnexpectedAfterApplication(
        RouteMovePlan plan,
        RouteMoveApplicationProgress progress,
        bool interrupted,
        string cause)
        => progress with
        {
            Verification = progress.Verification == RouteMoveVerificationState.Verified
                ? RouteMoveVerificationState.Verified
                : RouteMoveVerificationState.Unknown,
            Findings =
            [
                .. progress.Findings,
                Finding(
                    plan,
                    interrupted
                        ? RouteMoveFindingCode.Interrupted
                        : RouteMoveFindingCode.OperationFailed,
                    interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
                    cause),
            ],
        };

    internal static RouteMoveApplicationProgress UnexpectedAfterPreparation(
        RouteMovePlan plan,
        RouteMoveRecoveryPreparationResult preparation,
        bool interrupted)
    {
        var prepared = preparation.Preparation
            ?? throw new InvalidOperationException(
                "Unexpected post-preparation failure requires the prepared recovery identity.");
        return new RouteMoveApplicationProgress
        {
            Recovery = new RouteMoveRecovery
            {
                State = RouteMoveRecoveryState.Retained,
                ProtectedPaths = plan.Preview.Recovery.ProtectedPaths,
                ResidualPath = prepared.BundlePath,
            },
            Verification = RouteMoveVerificationState.Unknown,
            Findings =
            [
                Finding(
                    plan,
                    interrupted ? RouteMoveFindingCode.Interrupted : RouteMoveFindingCode.OperationFailed,
                    interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
                    interrupted
                        ? "Route Move application was interrupted after recovery preparation."
                        : "Route Move application failed after recovery preparation."),
            ],
        };
    }

    private static RouteMoveFinding Finding(
        RouteMovePlan plan,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string cause)
        => new(code, status, plan.Preview.Destination.Path, cause);
}
