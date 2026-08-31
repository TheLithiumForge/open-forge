using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;

internal sealed class RouteCreateEffectApplication(FileChangeApplier fileApplier)
{
    private readonly FileChangeApplier _fileApplier = fileApplier;

    internal async ValueTask<RouteCreateApplicationProgress> ApplyAsync(
        RouteCreatePlan plan,
        WorkspaceLockLease lease,
        MutationValidationResult validation,
        RecoveryBundlePreparation? preparation,
        CancellationToken cancellationToken)
    {
        var receipts = ImmutableArray.CreateBuilder<FileChangeReceipt>(
            plan.FileChanges.Length);
        for (var index = 0; index < plan.FileChanges.Length; index++)
        {
            var change = plan.FileChanges[index];
            FileChangeReceipt receipt;
            try
            {
                receipt = await _fileApplier.ApplyAsync(
                        lease,
                        change,
                        validation.Checks[index],
                        change.Kind == PlannedFileChangeKind.Create
                            ? null
                            : preparation,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Failure(
                    plan,
                    receipts,
                    preparation,
                    new RouteCreateFinding(
                        RouteCreateFindingCode.Interrupted,
                        "Route Create file application was interrupted.",
                        Relative(plan, change.LogicalPath)),
                    change);
            }
            catch (Exception)
            {
                return Failure(
                    plan,
                    receipts,
                    preparation,
                    new RouteCreateFinding(
                        RouteCreateFindingCode.OperationFailed,
                        "Route Create file application failed unexpectedly.",
                        Relative(plan, change.LogicalPath)),
                    change);
            }

            receipts.Add(receipt);
            if (!IsVerified(receipt))
            {
                return Failure(
                    plan,
                    receipts,
                    preparation,
                    ReceiptFinding(plan, receipt),
                    uncertainAttempt: null);
            }
        }

        return new RouteCreateApplicationProgress
        {
            Receipts = receipts.MoveToImmutable(),
            UncertainAttempt = null,
            Recovery = RecoveryAfterPreparation(plan, preparation),
            Verification = RouteCreateVerificationState.NotRequested,
            Findings = [],
        };
    }

    private static RouteCreateApplicationProgress Failure(
        RouteCreatePlan plan,
        ImmutableArray<FileChangeReceipt>.Builder receipts,
        RecoveryBundlePreparation? preparation,
        RouteCreateFinding finding,
        PlannedFileChange? uncertainAttempt)
        => new()
        {
            Receipts = receipts.ToImmutable(),
            UncertainAttempt = uncertainAttempt,
            Recovery = RecoveryAfterPreparation(plan, preparation),
            Verification = RouteCreateVerificationState.Unknown,
            Findings = [finding],
        };

    private static RouteCreateFinding ReceiptFinding(
        RouteCreatePlan plan,
        FileChangeReceipt receipt)
        => new(
            ReadReceiptFinding(receipt),
            receipt.Cause
                ?? "A planned Route Create file could not be applied and verified.",
            Relative(plan, receipt.Change.LogicalPath));

    private static RouteCreateFindingCode ReadReceiptFinding(
        FileChangeReceipt receipt)
        => (receipt.EffectState, receipt.VerificationState, receipt.NotStartedReason) switch
        {
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.Cancelled) => RouteCreateFindingCode.Interrupted,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.TargetChanged) =>
                RouteCreateFindingCode.TargetChangedDuringApply,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ApplicationFailed) => RouteCreateFindingCode.WriteFailed,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ContractRejected) =>
                RouteCreateFindingCode.OperationFailed,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed, null) =>
                RouteCreateFindingCode.VerificationFailed,
            (FilesystemEffectState.Unknown, FilesystemVerificationState.NotStarted, null) =>
                RouteCreateFindingCode.WriteFailed,
            _ => throw new InvalidOperationException(
                "The Route Create filesystem receipt is incoherent."),
        };

    private static bool IsVerified(FileChangeReceipt receipt)
        => receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified;

    private static RouteCreateRecovery RecoveryAfterPreparation(
        RouteCreatePlan plan,
        RecoveryBundlePreparation? preparation)
        => preparation is null
            ? BaseRecovery(plan)
            : new RouteCreateRecovery
            {
                State = RouteCreateRecoveryState.Retained,
                ResidualPath = preparation.BundlePath,
            };

    internal static RouteCreateRecovery BaseRecovery(RouteCreatePlan plan)
        => new()
        {
            State = plan.RecoveryTargets.IsEmpty
                ? RouteCreateRecoveryState.NotRequired
                : RouteCreateRecoveryState.NotCreated,
            ResidualPath = null,
        };

    internal static RouteCreateFinding Finding(
        RouteCreatePlan plan,
        RouteCreateFindingCode code,
        string cause)
        => new(code, cause, plan.Preview.Target.Path ?? plan.Preview.Target.Requested);

    private static string Relative(RouteCreatePlan plan, string absolutePath)
        => Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, absolutePath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
}
