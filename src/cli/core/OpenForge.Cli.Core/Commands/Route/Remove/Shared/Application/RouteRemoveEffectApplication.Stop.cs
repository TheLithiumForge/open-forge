using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveEffectApplication
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
            FileExpectationValidationState.Mismatched => "The planned target changed before its Route Remove effect.",
            FileExpectationValidationState.Cancelled => "Route Remove effect application was interrupted.",
            FileExpectationValidationState.Failed => "The planned target could not be inspected before its Route Remove effect.",
            FileExpectationValidationState.Blocked => "The planned target became unsafe before its Route Remove effect.",
            FileExpectationValidationState.Matched => throw new ArgumentOutOfRangeException(
                nameof(result), result.State, "A matched check has no failure cause."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result), result.State, "The file validation state is not defined."),
        };

    private static ApplicationStop? ReadStop(RouteRemovePlan plan, FileChangeReceipt receipt)
        => IsVerified(receipt.EffectState, receipt.VerificationState)
            ? null
            : Stop(plan, receipt.NotStartedReason, receipt.Cause);

    private static ApplicationStop? ReadStop(RouteRemovePlan plan, DirectoryDeletionReceipt receipt)
        => IsVerified(receipt.State.EffectState, receipt.State.VerificationState)
            ? null
            : Stop(plan, receipt.State.NotStartedReason, receipt.Cause);

    private static bool IsVerified(
        FilesystemEffectState effect,
        FilesystemVerificationState verification)
        => effect == FilesystemEffectState.Applied
            && verification == FilesystemVerificationState.Verified;

    private static ApplicationStop Stop(
        RouteRemovePlan plan,
        FilesystemNotStartedReason? reason,
        string? cause)
    {
        var code = reason switch
        {
            FilesystemNotStartedReason.Cancelled => RouteRemoveFindingCode.Interrupted,
            FilesystemNotStartedReason.TargetChanged => RouteRemoveFindingCode.TargetChangedDuringApply,
            FilesystemNotStartedReason.ApplicationFailed => RouteRemoveFindingCode.WriteFailed,
            FilesystemNotStartedReason.ContractRejected or null => RouteRemoveFindingCode.OperationFailed,
            _ => throw new ArgumentOutOfRangeException(
                nameof(reason), reason, "The filesystem not-started reason is not defined."),
        };
        var status = code == RouteRemoveFindingCode.Interrupted
            ? CliSemanticStatus.Interrupted
            : CliSemanticStatus.Failed;
        return new ApplicationStop(
            new RouteRemoveFinding(
                code,
                status,
                plan.Preview.Source.Path,
                cause ?? "A Route Remove effect did not complete and verify."),
            FilesystemNotStartedReason.ApplicationFailed,
            "A prior Route Remove effect failed.");
    }

    private static ApplicationStop Interrupted(
        RouteRemovePlan plan,
        FilesystemNotStartedReason reason)
        => new(
            new RouteRemoveFinding(
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                plan.Preview.Source.Path,
                "Route Remove effect application was interrupted."),
            reason,
            "Route Remove effect application was interrupted.");

    private sealed record ApplicationStop(
        RouteRemoveFinding Finding,
        FilesystemNotStartedReason Reason,
        string Cause);
}
