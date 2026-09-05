using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveEffectApplication
{
    private sealed class ApplicationReceiptProgress(
        RouteRemovePlan plan,
        RecoveryBundlePreparation preparation,
        int receiptCount)
    {
        private readonly ImmutableArray<RouteRemoveApplicationReceipt>.Builder _receipts =
            ImmutableArray.CreateBuilder<RouteRemoveApplicationReceipt>(receiptCount);
        private RouteRemoveApplicationReceipt? _pending;

        internal void Enter(PlannedFileChange change)
            => _pending = new RouteRemoveFileChangeReceipt(CompletionUnknown(plan, change));

        internal void Enter(PlannedDirectoryDeletion deletion)
            => _pending = new RouteRemoveDirectoryDeletionReceipt(CompletionUnknown(plan, deletion));

        internal void Complete(RouteRemoveApplicationReceipt receipt)
        {
            if (_pending is null)
            {
                throw new InvalidOperationException(
                    "A completed Route Remove effect requires an entered effect.");
            }

            _receipts.Add(receipt);
            _pending = null;
        }

        internal void Add(RouteRemoveApplicationReceipt receipt)
        {
            if (_pending is not null)
            {
                throw new InvalidOperationException(
                    "A Route Remove not-started receipt cannot replace an entered effect.");
            }

            _receipts.Add(receipt);
        }

        internal RouteRemoveApplicationProgress Form(ApplicationStop? stop)
            => FormProgress(plan, preparation, _receipts, stop);

        internal RouteRemoveApplicationProgress CloseUnexpected(bool interrupted)
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
            foreach (var change in plan.Projection.FileChanges)
            {
                if (index++ >= _receipts.Count)
                {
                    Add(new RouteRemoveFileChangeReceipt(
                        NotStarted(plan, change, stop.Reason, stop.Cause)));
                }
            }

            foreach (var deletion in plan.Projection.DirectoryDeletions)
            {
                if (index++ >= _receipts.Count)
                {
                    Add(new RouteRemoveDirectoryDeletionReceipt(
                        NotStarted(plan, deletion, stop.Reason, stop.Cause)));
                }
            }
        }
    }

    private static ApplicationStop Unexpected(RouteRemovePlan plan, bool interrupted)
    {
        var cause = interrupted
            ? "Route Remove effect application was interrupted after concrete progress."
            : "Route Remove effect application failed unexpectedly after concrete progress.";
        return new ApplicationStop(
            new RouteRemoveFinding(
                interrupted ? RouteRemoveFindingCode.Interrupted : RouteRemoveFindingCode.OperationFailed,
                interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed,
                plan.Preview.Source.Path,
                cause),
            interrupted
                ? FilesystemNotStartedReason.Cancelled
                : FilesystemNotStartedReason.ApplicationFailed,
            cause);
    }
}
