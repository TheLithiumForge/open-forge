using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Models;

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

internal sealed record ExtensionPermissionTarget(string Path, ExtensionPermissionEffect Effect);

internal sealed record ExtensionPermissionRequest(
    CliWorkspace Workspace,
    ImmutableArray<ExtensionPermissionTarget> Targets,
    string? SourceIdentity,
    bool AllowPrompt,
    ImmutableArray<string> ExplicitGrantPaths = default)
{
    internal ImmutableArray<string> Grants => ExplicitGrantPaths.IsDefault ? [] : ExplicitGrantPaths;
}

internal sealed record ExtensionPermissionStage(
    WorkspaceSettingsRead? Observation,
    WorkspacePermissionResult Result,
    PlannedFileChange? Change,
    RecoveryBundleTarget? RecoveryTarget,
    ExtensionPermissionFailure? Failure);

internal sealed record ExtensionPermissionApplication(
    WorkspacePermissionResult Result,
    ExtensionPermissionFailure? Failure);
