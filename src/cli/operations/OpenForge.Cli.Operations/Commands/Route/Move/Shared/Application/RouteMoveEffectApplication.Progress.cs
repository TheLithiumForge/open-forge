using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMoveEffectApplication
{
    private sealed class ApplicationReceiptProgress(
        RouteMovePlan plan,
        RecoveryBundlePreparation preparation,
        int receiptCount)
    {
        private readonly ImmutableArray<RouteMoveApplicationReceipt>.Builder _receipts =
            ImmutableArray.CreateBuilder<RouteMoveApplicationReceipt>(receiptCount);
        private RouteMoveApplicationReceipt? _pending;

        internal void Enter(PlannedDirectoryCreation creation)
            => _pending = new RouteMoveDirectoryCreationReceipt(CompletionUnknown(plan, creation));

        internal void Enter(PlannedFileChange change)
            => _pending = new RouteMoveFileChangeReceipt(CompletionUnknown(plan, change));

        internal void Enter(PlannedDirectoryDeletion deletion)
            => _pending = new RouteMoveDirectoryDeletionReceipt(CompletionUnknown(plan, deletion));

        internal void Complete(RouteMoveApplicationReceipt receipt)
        {
            if (_pending is null)
            {
                throw new InvalidOperationException(
                    "A completed Route Move effect requires an entered effect.");
            }

            _receipts.Add(receipt);
            _pending = null;
        }

        internal void Add(RouteMoveApplicationReceipt receipt)
        {
            if (_pending is not null)
            {
                throw new InvalidOperationException(
                    "A Route Move not-started receipt cannot replace an entered effect.");
            }

            _receipts.Add(receipt);
        }

        internal RouteMoveApplicationProgress Form(ApplicationStop? stop)
            => FormProgress(plan, preparation, _receipts, stop);

        internal RouteMoveApplicationProgress CloseUnexpected(bool interrupted)
        {
            if (_pending is not null)
            {
                _receipts.Add(_pending);
                _pending = null;
            }

            var stop = Unexpected(plan, interrupted);
            AppendRemaining(stop);
            return FormProgress(plan, preparation, _receipts, stop);
        }

        private void AppendRemaining(ApplicationStop stop)
        {
            var index = 0;
            foreach (var creation in plan.Projection.DirectoryCreations)
            {
                if (index++ >= _receipts.Count)
                {
                    Add(new RouteMoveDirectoryCreationReceipt(
                        NotStarted(plan, creation, stop.Reason, stop.Cause)));
                }
            }

            foreach (var change in plan.Projection.FileChanges)
            {
                if (index++ >= _receipts.Count)
                {
                    Add(new RouteMoveFileChangeReceipt(
                        NotStarted(plan, change, stop.Reason, stop.Cause)));
                }
            }

            foreach (var deletion in plan.Projection.DirectoryDeletions)
            {
                if (index++ >= _receipts.Count)
                {
                    Add(new RouteMoveDirectoryDeletionReceipt(
                        NotStarted(plan, deletion, stop.Reason, stop.Cause)));
                }
            }
        }
    }

    private static ApplicationStop Unexpected(RouteMovePlan plan, bool interrupted)
    {
        var cause = interrupted
            ? "Route Move effect application was interrupted after concrete progress."
            : "Route Move effect application failed unexpectedly after concrete progress.";
        return new ApplicationStop(
            new RouteMoveFinding(
                interrupted ? RouteMoveFindingCode.Interrupted : RouteMoveFindingCode.OperationFailed,
                interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
                plan.Preview.Destination.Path,
                cause),
            interrupted
                ? FilesystemNotStartedReason.Cancelled
                : FilesystemNotStartedReason.ApplicationFailed,
            cause);
    }
}
