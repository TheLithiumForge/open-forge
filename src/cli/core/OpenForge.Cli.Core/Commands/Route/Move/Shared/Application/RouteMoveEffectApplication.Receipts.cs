using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMoveEffectApplication
{
    private static RouteMoveApplicationProgress FormProgress(
        RouteMovePlan plan,
        RecoveryBundlePreparation preparation,
        ImmutableArray<RouteMoveApplicationReceipt>.Builder receipts,
        ApplicationStop? stop)
    {
        var anyAttempted = receipts.Any(receipt =>
            EffectState(receipt) != FilesystemEffectState.NotStarted);
        return new RouteMoveApplicationProgress
        {
            Receipts = receipts.MoveToImmutable(),
            Recovery = RetainedRecovery(plan, preparation.BundlePath),
            Verification = stop is null || !anyAttempted
                ? RouteMoveVerificationState.NotRequested
                : RouteMoveVerificationState.Unknown,
            Findings = stop is null ? [] : [stop.Finding],
        };
    }

    private static DirectoryCreationReceipt NotStarted(
        RouteMovePlan plan,
        PlannedDirectoryCreation creation,
        FilesystemNotStartedReason reason,
        string cause)
        => DirectoryCreationReceipt.NotStarted(
            creation,
            FileStateSnapshot.Missing(creation.LogicalPath),
            IntendedPhysicalPath(plan, creation.LogicalPath),
            reason,
            cause);

    private static FileChangeReceipt NotStarted(
        RouteMovePlan plan,
        PlannedFileChange change,
        FilesystemNotStartedReason reason,
        string cause)
        => FileChangeReceipt.NotStarted(
            change,
            Before(plan, change.Expectation),
            reason,
            cause);

    private static DirectoryDeletionReceipt NotStarted(
        RouteMovePlan plan,
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

    private static DirectoryCreationReceipt CompletionUnknown(
        RouteMovePlan plan,
        PlannedDirectoryCreation creation)
        => DirectoryCreationReceipt.CompletionUnknown(
            creation,
            FileStateSnapshot.Missing(creation.LogicalPath),
            IntendedPhysicalPath(plan, creation.LogicalPath),
            after: null,
            "A Route Move directory effect failed unexpectedly after it was attempted.");

    private static FileChangeReceipt CompletionUnknown(
        RouteMovePlan plan,
        PlannedFileChange change)
        => FileChangeReceipt.CompletionUnknown(
            change,
            Before(plan, change.Expectation),
            after: null,
            "A Route Move file effect failed unexpectedly after it was attempted.");

    private static DirectoryDeletionReceipt CompletionUnknown(
        RouteMovePlan plan,
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
            "A Route Move directory deletion failed unexpectedly after it was attempted.");

    private static FileStateSnapshot Before(RouteMovePlan plan, FileExpectation expectation)
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

    private static string IntendedPhysicalPath(RouteMovePlan plan, string logicalPath)
        => Path.Combine(
            plan.Request.Workspace.PhysicalRoot,
            Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, logicalPath));

    private static RouteMoveRecovery RetainedRecovery(RouteMovePlan plan, string path)
        => new()
        {
            State = RouteMoveRecoveryState.Retained,
            ProtectedPaths = plan.Preview.Recovery.ProtectedPaths,
            ResidualPath = path,
        };

    private static FilesystemEffectState EffectState(RouteMoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteMoveDirectoryCreationReceipt creation => creation.Receipt.EffectState,
            RouteMoveFileChangeReceipt change => change.Receipt.EffectState,
            RouteMoveDirectoryDeletionReceipt deletion => deletion.Receipt.State.EffectState,
            _ => throw new ArgumentOutOfRangeException(
                nameof(receipt), receipt, "The Route Move receipt kind is not defined."),
        };

}
