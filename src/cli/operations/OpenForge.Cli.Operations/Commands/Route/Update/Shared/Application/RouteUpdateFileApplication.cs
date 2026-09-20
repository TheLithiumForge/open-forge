using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal delegate ValueTask<FileChangeReceipt> RouteUpdateFileApplication(
    WorkspaceLockLease lease,
    PlannedFileChange change,
    FileExpectationValidationResult check,
    RecoveryBundlePreparation? recoveryPreparation,
    CancellationToken cancellationToken);
