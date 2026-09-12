using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveEffectApplication
{
    private static RouteRemoveApplicationProgress FormProgress(
        RouteRemovePlan plan,
        RecoveryBundlePreparation preparation,
        ImmutableArray<RouteRemoveApplicationReceipt>.Builder receipts,
        ApplicationStop? stop)
    {
        var anyAttempted = receipts.Any(receipt =>
            EffectState(receipt) != FilesystemEffectState.NotStarted);
        return new RouteRemoveApplicationProgress
        {
            Receipts = receipts.MoveToImmutable(),
            Recovery = RetainedRecovery(plan, preparation.BundlePath),
            Verification = stop is null || !anyAttempted
                ? RouteRemoveVerificationState.NotRequested
                : RouteRemoveVerificationState.Unknown,
            Findings = stop is null ? [] : [stop.Finding],
        };
    }

    private static FileChangeReceipt NotStarted(
        RouteRemovePlan plan,
        PlannedFileChange change,
        FilesystemNotStartedReason reason,
        string cause)
        => FileChangeReceipt.NotStarted(
            change,
            Before(plan, change.Expectation),
            reason,
            cause);

    private static DirectoryDeletionReceipt NotStarted(
        RouteRemovePlan plan,
        PlannedDirectoryDeletion deletion,
        FilesystemNotStartedReason reason,
        string cause)
    {
        var before = Before(plan, deletion.Expectation);
        return new DirectoryDeletionReceipt(
            deletion,
            before,
            before,
            new DirectoryDeletionReceiptState(
                FilesystemEffectState.NotStarted,
                FilesystemVerificationState.NotStarted,
                reason,
                DirectoryDeletionDisposition.Retained),
            cause);
    }

    private static FileChangeReceipt CompletionUnknown(
        RouteRemovePlan plan,
        PlannedFileChange change)
        => FileChangeReceipt.CompletionUnknown(
            change,
            Before(plan, change.Expectation),
            after: null,
            "A Route Remove file effect failed unexpectedly after it was attempted.");

    private static DirectoryDeletionReceipt CompletionUnknown(
        RouteRemovePlan plan,
        PlannedDirectoryDeletion deletion)
        => new(
            deletion,
            Before(plan, deletion.Expectation),
            after: null,
            new DirectoryDeletionReceiptState(
                FilesystemEffectState.Unknown,
                FilesystemVerificationState.NotStarted,
                notStartedReason: null,
                DirectoryDeletionDisposition.Unknown),
            "A Route Remove directory deletion failed unexpectedly after it was attempted.");

    private static FileStateSnapshot Before(RouteRemovePlan plan, FileExpectation expectation)
        => expectation.Kind switch
        {
            FileExpectationKind.Missing => FileStateSnapshot.Missing(expectation.LogicalPath),
            FileExpectationKind.Directory => FileStateSnapshot.Directory(
                expectation.LogicalPath,
                expectation.PhysicalPath
                    ?? throw new InvalidOperationException("A directory expectation requires its physical path.")),
            FileExpectationKind.File => plan.Projection.RecoveryTargets
                .Single(target => target.Before.Expectation == expectation).Before,
            _ => throw new ArgumentOutOfRangeException(
                nameof(expectation), expectation.Kind, "The file expectation kind is not defined."),
        };

    private static RouteRemoveRecovery RetainedRecovery(RouteRemovePlan plan, string path)
        => new()
        {
            State = RouteRemoveRecoveryState.Retained,
            ProtectedPaths = plan.Preview.Recovery.ProtectedPaths,
            ResidualPath = path,
        };

    private static FilesystemEffectState EffectState(RouteRemoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteRemoveFileChangeReceipt change => change.Receipt.EffectState,
            RouteRemoveDirectoryDeletionReceipt deletion => deletion.Receipt.State.EffectState,
            _ => throw new ArgumentOutOfRangeException(
                nameof(receipt), receipt, "The Route Remove receipt kind is not defined."),
        };

}
