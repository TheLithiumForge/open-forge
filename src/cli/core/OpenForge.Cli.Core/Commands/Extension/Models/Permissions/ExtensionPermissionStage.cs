using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Extension.Models.Permissions;

internal enum ExtensionPermissionFailure
{
    Required,
    Declined,
    Invalid,
    Unavailable,
    Changed,
    WriteFailed,
    Interrupted,
}

internal enum ExtensionPermissionEffect
{
    Copy,
    Delete,
    ReleaseOwnership,
    Preserve,
}

internal sealed record ExtensionPermissionTarget(WorkspacePermissionRequirement Requirement, ExtensionPermissionEffect Effect);

internal sealed record ExtensionPermissionRequest(
    CliWorkspace Workspace,
    ImmutableArray<ExtensionPermissionTarget> Targets,
    string? SourceIdentity,
    bool AllowPrompt);

internal sealed record ExtensionPermissionStage(
    WorkspacePermissionRead? Observation,
    WorkspacePermissionResult Result,
    PlannedFileChange? Change,
    RecoveryBundleTarget? RecoveryTarget,
    ExtensionPermissionFailure? Failure);

internal sealed record ExtensionPermissionApplication(
    WorkspacePermissionResult Result,
    ExtensionPermissionFailure? Failure);
