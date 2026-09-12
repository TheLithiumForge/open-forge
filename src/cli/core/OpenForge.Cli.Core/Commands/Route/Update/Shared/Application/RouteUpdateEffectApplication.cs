using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal sealed class RouteUpdateEffectApplication(FileChangeApplier fileApplier)
{
    private readonly FileChangeApplier _fileApplier = fileApplier;

    internal async ValueTask<RouteUpdateApplicationProgress> ApplyAsync(
        RouteUpdateEffectApplicationInput input,
        CancellationToken cancellationToken)
    {
        var receipts = ImmutableArray.CreateBuilder<FileChangeReceipt>(
            input.Plan.FileChanges.Length);
        for (var index = 0; index < input.Plan.FileChanges.Length; index++)
        {
            var change = input.Plan.FileChanges[index];
            FileChangeReceipt receipt;
            try
            {
                receipt = await _fileApplier.ApplyAsync(
                        input.Lease,
                        change,
                        input.Validation.Checks[index],
                        input.Preparation,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return FailureBeforeReceipt(
                    input,
                    receipts,
                    RouteUpdateFindingCode.Interrupted,
                    "Route Update file application was interrupted.",
                    change);
            }
            catch (Exception)
            {
                return FailureBeforeReceipt(
                    input,
                    receipts,
                    RouteUpdateFindingCode.OperationFailed,
                    "Route Update file application failed unexpectedly.",
                    change);
            }

            receipts.Add(receipt);
            if (!IsVerified(receipt))
            {
                return FailureFromReceipt(input, receipts, receipt);
            }
        }

        return new RouteUpdateApplicationProgress
        {
            Receipts = receipts.MoveToImmutable(),
            UncertainAttempt = null,
            Recovery = Retained(input.Preparation),
            Verification = RouteUpdateVerificationState.NotRequested,
            Findings = [],
        };
    }

    private static RouteUpdateApplicationProgress FailureBeforeReceipt(
        RouteUpdateEffectApplicationInput input,
        ImmutableArray<FileChangeReceipt>.Builder receipts,
        RouteUpdateFindingCode code,
        string cause,
        PlannedFileChange uncertainAttempt)
        => new()
        {
            Receipts = receipts.ToImmutable(),
            UncertainAttempt = uncertainAttempt,
            Recovery = Retained(input.Preparation),
            Verification = RouteUpdateVerificationState.Unknown,
            Findings = [Finding(input.Plan, code, cause, uncertainAttempt.LogicalPath)],
        };

    private static RouteUpdateApplicationProgress FailureFromReceipt(
        RouteUpdateEffectApplicationInput input,
        ImmutableArray<FileChangeReceipt>.Builder receipts,
        FileChangeReceipt receipt)
        => new()
        {
            Receipts = receipts.ToImmutable(),
            UncertainAttempt = null,
            Recovery = Retained(input.Preparation),
            Verification = RouteUpdateVerificationState.Unknown,
            Findings =
            [
                Finding(
                    input.Plan,
                    ReadReceiptFinding(receipt),
                    receipt.Cause
                        ?? "A planned Route Update file could not be applied and verified.",
                    receipt.Change.LogicalPath),
            ],
        };

    private static RouteUpdateFinding Finding(
        RouteUpdatePlan plan,
        RouteUpdateFindingCode code,
        string cause,
        string? absolutePath)
        => new(
            code,
            cause,
            absolutePath is null
                ? plan.Preview.Target.Path
                : Relative(plan, absolutePath));

    private static RouteUpdateFindingCode ReadReceiptFinding(FileChangeReceipt receipt)
        => (receipt.EffectState, receipt.VerificationState, receipt.NotStartedReason) switch
        {
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.Cancelled) => RouteUpdateFindingCode.Interrupted,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.TargetChanged) =>
                RouteUpdateFindingCode.TargetChangedDuringApply,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ApplicationFailed) => RouteUpdateFindingCode.WriteFailed,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ContractRejected) => RouteUpdateFindingCode.OperationFailed,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed, null) =>
                RouteUpdateFindingCode.VerificationFailed,
            (FilesystemEffectState.Unknown, FilesystemVerificationState.NotStarted, null) =>
                RouteUpdateFindingCode.WriteFailed,
            _ => throw new InvalidOperationException(
                "The Route Update filesystem receipt is incoherent."),
        };

    private static bool IsVerified(FileChangeReceipt receipt)
        => receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified;

    private static RouteUpdateRecovery Retained(RecoveryBundlePreparation preparation)
        => new()
        {
            State = RouteUpdateRecoveryState.Retained,
            ResidualPath = preparation.BundlePath,
        };

    private static string Relative(RouteUpdatePlan plan, string absolutePath)
        => Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, absolutePath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
}
