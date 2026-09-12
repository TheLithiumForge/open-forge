using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Library.Models.Permissions;

internal enum LibraryPermissionFailure
{
    Required,
    Declined,
    Invalid,
    Unavailable,
    Changed,
    WriteFailed,
    Interrupted,
}

internal sealed record LibraryPermissionStage
{
    public required WorkspacePermissionRead? Observation { get; init; }
    public required LibraryPermissionApproval? Approval { get; init; }
    public required WorkspacePermissionResult Result { get; init; }
    public required PlannedFileChange? Change { get; init; }
    public required RecoveryBundleTarget? RecoveryTarget { get; init; }
    public required LibraryPermissionFailure? Failure { get; init; }
}

internal sealed record LibraryPermissionApplication(
    WorkspacePermissionResult Result,
    FileChangeReceipt? Receipt,
    LibraryPermissionFailure? Failure);
