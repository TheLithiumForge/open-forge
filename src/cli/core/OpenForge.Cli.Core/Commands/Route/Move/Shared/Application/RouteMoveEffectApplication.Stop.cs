using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMoveEffectApplication
{
    private static FilesystemNotStartedReason Reason(FileExpectationValidationState state)
        => state switch
        {
            FileExpectationValidationState.Mismatched => FilesystemNotStartedReason.TargetChanged,
            FileExpectationValidationState.Cancelled => FilesystemNotStartedReason.Cancelled,
            FileExpectationValidationState.Failed => FilesystemNotStartedReason.ApplicationFailed,
            FileExpectationValidationState.Blocked => FilesystemNotStartedReason.ContractRejected,
            FileExpectationValidationState.Matched => throw new ArgumentOutOfRangeException(
                nameof(state), state, "A matched check has no not-started reason."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state), state, "The file validation state is not defined."),
        };

    private static string Cause(FileExpectationValidationResult result)
        => result.Cause ?? result.State switch
        {
            FileExpectationValidationState.Mismatched => "The planned target changed before its Route Move effect.",
            FileExpectationValidationState.Cancelled => "Route Move effect application was interrupted.",
            FileExpectationValidationState.Failed => "The planned target could not be inspected before its Route Move effect.",
            FileExpectationValidationState.Blocked => "The planned target became unsafe before its Route Move effect.",
            FileExpectationValidationState.Matched => throw new ArgumentOutOfRangeException(
                nameof(result), result.State, "A matched check has no failure cause."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result), result.State, "The file validation state is not defined."),
        };

    private static ApplicationStop? ReadStop(RouteMovePlan plan, DirectoryCreationReceipt receipt)
        => IsVerified(receipt.EffectState, receipt.VerificationState)
            ? null
            : Stop(plan, receipt.NotStartedReason, receipt.Cause);

    private static ApplicationStop? ReadStop(RouteMovePlan plan, FileChangeReceipt receipt)
        => IsVerified(receipt.EffectState, receipt.VerificationState)
            ? null
            : Stop(plan, receipt.NotStartedReason, receipt.Cause);

    private static ApplicationStop? ReadStop(RouteMovePlan plan, DirectoryDeletionReceipt receipt)
        => IsVerified(receipt.State.EffectState, receipt.State.VerificationState)
            ? null
            : Stop(plan, receipt.State.NotStartedReason, receipt.Cause);

    private static bool IsVerified(
        FilesystemEffectState effect,
        FilesystemVerificationState verification)
        => effect == FilesystemEffectState.Applied
            && verification == FilesystemVerificationState.Verified;

    private static ApplicationStop Stop(
        RouteMovePlan plan,
        FilesystemNotStartedReason? reason,
        string? cause)
    {
        var code = reason switch
        {
            FilesystemNotStartedReason.Cancelled => RouteMoveFindingCode.Interrupted,
            FilesystemNotStartedReason.TargetChanged => RouteMoveFindingCode.TargetChangedDuringApply,
            FilesystemNotStartedReason.ApplicationFailed => RouteMoveFindingCode.WriteFailed,
            FilesystemNotStartedReason.ContractRejected or null => RouteMoveFindingCode.OperationFailed,
            _ => throw new ArgumentOutOfRangeException(
                nameof(reason), reason, "The filesystem not-started reason is not defined."),
        };
        var status = code == RouteMoveFindingCode.Interrupted
            ? CliSemanticStatus.Interrupted
            : CliSemanticStatus.Failed;
        return new ApplicationStop(
            new RouteMoveFinding(
                code,
                status,
                plan.Preview.Destination.Path,
                cause ?? "A Route Move effect did not complete and verify."),
            FilesystemNotStartedReason.ApplicationFailed,
            "A prior Route Move effect failed.");
    }

    private static ApplicationStop Interrupted(
        RouteMovePlan plan,
        FilesystemNotStartedReason reason)
        => new(
            new RouteMoveFinding(
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                plan.Preview.Destination.Path,
                "Route Move effect application was interrupted."),
            reason,
            "Route Move effect application was interrupted.");

    private sealed record ApplicationStop(
        RouteMoveFinding Finding,
        FilesystemNotStartedReason Reason,
        string Cause);
}
