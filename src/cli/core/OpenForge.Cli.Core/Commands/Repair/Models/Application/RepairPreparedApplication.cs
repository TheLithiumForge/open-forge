using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Application;

internal sealed record RepairPreparedApplication(
    RepairPlan Plan,
    WorkspaceLockLease Lease,
    RecoveryBundlePreparation Preparation,
    IReadOnlyList<FileChangeReceipt> Receipts);
