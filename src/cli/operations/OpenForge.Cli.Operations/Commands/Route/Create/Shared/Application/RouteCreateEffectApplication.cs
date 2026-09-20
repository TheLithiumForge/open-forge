using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;

internal sealed class RouteCreateEffectApplication(
    DirectoryCreationApplier directoryApplier,
    FileChangeApplier fileApplier)
{
    private readonly DirectoryCreationApplier _directoryApplier = directoryApplier;
    private readonly FileChangeApplier _fileApplier = fileApplier;

    internal async ValueTask<RouteCreateApplicationProgress> ApplyAsync(
        RouteCreatePlan plan,
        WorkspaceLockLease lease,
        MutationValidationResult validation,
        RecoveryBundlePreparation? preparation,
        CancellationToken cancellationToken)
    {
        var directoryReceipts = ImmutableArray.CreateBuilder<DirectoryCreationReceipt>(
            plan.DirectoryCreations.Length);
        var fileReceipts = ImmutableArray.CreateBuilder<FileChangeReceipt>(
            plan.FileChanges.Length);
        var checkIndex = 0;
        foreach (var creation in plan.DirectoryCreations)
        {
            DirectoryCreationReceipt receipt;
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
                return Failure(
                    plan,
                    directoryReceipts,
                    fileReceipts,
                    preparation,
                    new RouteCreateFinding(
                        RouteCreateFindingCode.Interrupted,
                        "Route Create directory application was interrupted.",
                        Relative(plan, creation.LogicalPath)),
                    uncertainDirectoryAttempt: creation,
                    uncertainFileAttempt: null);
            }
            catch (Exception)
            {
                return Failure(
                    plan,
                    directoryReceipts,
                    fileReceipts,
                    preparation,
                    new RouteCreateFinding(
                        RouteCreateFindingCode.OperationFailed,
                        "Route Create directory application failed unexpectedly.",
                        Relative(plan, creation.LogicalPath)),
                    uncertainDirectoryAttempt: creation,
                    uncertainFileAttempt: null);
            }

            directoryReceipts.Add(receipt);
            if (!IsVerified(receipt))
            {
                return Failure(
                    plan,
                    directoryReceipts,
                    fileReceipts,
                    preparation,
                    DirectoryReceiptFinding(plan, receipt),
                    uncertainDirectoryAttempt: null,
                    uncertainFileAttempt: null);
            }
        }

        foreach (var change in plan.FileChanges)
        {
            FileChangeReceipt receipt;
            try
            {
                receipt = await _fileApplier.ApplyAsync(
                        lease,
                        change,
                        validation.Checks[checkIndex++],
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
                    directoryReceipts,
                    fileReceipts,
                    preparation,
                    new RouteCreateFinding(
                        RouteCreateFindingCode.Interrupted,
                        "Route Create file application was interrupted.",
                        Relative(plan, change.LogicalPath)),
                    uncertainDirectoryAttempt: null,
                    uncertainFileAttempt: change);
            }
            catch (Exception)
            {
                return Failure(
                    plan,
                    directoryReceipts,
                    fileReceipts,
                    preparation,
                    new RouteCreateFinding(
                        RouteCreateFindingCode.OperationFailed,
                        "Route Create file application failed unexpectedly.",
                        Relative(plan, change.LogicalPath)),
                    uncertainDirectoryAttempt: null,
                    uncertainFileAttempt: change);
            }

            fileReceipts.Add(receipt);
            if (!IsVerified(receipt))
            {
                return Failure(
                    plan,
                    directoryReceipts,
                    fileReceipts,
                    preparation,
                    ReceiptFinding(plan, receipt),
                    uncertainDirectoryAttempt: null,
                    uncertainFileAttempt: null);
            }
        }

        return new RouteCreateApplicationProgress
        {
            DirectoryReceipts = directoryReceipts.MoveToImmutable(),
            UncertainDirectoryAttempt = null,
            Receipts = fileReceipts.MoveToImmutable(),
            UncertainAttempt = null,
            Recovery = RecoveryAfterPreparation(plan, preparation),
            Verification = RouteCreateVerificationState.NotRequested,
            Findings = [],
        };
    }

    private static RouteCreateApplicationProgress Failure(
        RouteCreatePlan plan,
        ImmutableArray<DirectoryCreationReceipt>.Builder directoryReceipts,
        ImmutableArray<FileChangeReceipt>.Builder fileReceipts,
        RecoveryBundlePreparation? preparation,
        RouteCreateFinding finding,
        PlannedDirectoryCreation? uncertainDirectoryAttempt,
        PlannedFileChange? uncertainFileAttempt)
        => new()
        {
            DirectoryReceipts = directoryReceipts.ToImmutable(),
            UncertainDirectoryAttempt = uncertainDirectoryAttempt,
            Receipts = fileReceipts.ToImmutable(),
            UncertainAttempt = uncertainFileAttempt,
            Recovery = RecoveryAfterPreparation(plan, preparation),
            Verification = RouteCreateVerificationState.Unknown,
            Findings = [finding],
        };

    private static RouteCreateFinding DirectoryReceiptFinding(
        RouteCreatePlan plan,
        DirectoryCreationReceipt receipt)
        => new(
            ReadReceiptFinding(receipt),
            receipt.Cause
                ?? "A planned Route Create directory could not be applied and verified.",
            Relative(plan, receipt.Creation.LogicalPath));

    private static RouteCreateFinding ReceiptFinding(
        RouteCreatePlan plan,
        FileChangeReceipt receipt)
        => new(
            ReadReceiptFinding(receipt),
            receipt.Cause
                ?? "A planned Route Create file could not be applied and verified.",
            Relative(plan, receipt.Change.LogicalPath));

    private static RouteCreateFindingCode ReadReceiptFinding(
        DirectoryCreationReceipt receipt)
        => ReadReceiptFinding(
            receipt.EffectState,
            receipt.VerificationState,
            receipt.NotStartedReason,
            "directory");

    private static RouteCreateFindingCode ReadReceiptFinding(
        FileChangeReceipt receipt)
        => ReadReceiptFinding(
            receipt.EffectState,
            receipt.VerificationState,
            receipt.NotStartedReason,
            "file");

    private static RouteCreateFindingCode ReadReceiptFinding(
        FilesystemEffectState effect,
        FilesystemVerificationState verification,
        FilesystemNotStartedReason? notStarted,
        string kind)
        => (effect, verification, notStarted) switch
        {
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.Cancelled) => RouteCreateFindingCode.Interrupted,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.TargetChanged) => RouteCreateFindingCode.TargetChangedDuringApply,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ApplicationFailed) => RouteCreateFindingCode.WriteFailed,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ContractRejected) => RouteCreateFindingCode.OperationFailed,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed, null) =>
                RouteCreateFindingCode.VerificationFailed,
            (FilesystemEffectState.Unknown, _, null) => RouteCreateFindingCode.WriteFailed,
            _ => throw new InvalidOperationException(
                $"The Route Create {kind} filesystem receipt is incoherent."),
        };

    private static bool IsVerified(DirectoryCreationReceipt receipt)
        => receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified;

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
