using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Library.Models.Permissions;

internal enum LibraryPermissionTargetUse
{
    Live,
    Retired,
}

internal enum LibraryPermissionEffect
{
    CreateLink,
    RetainLink,
    RemoveLink,
}

internal sealed record LibraryPermissionTarget(string DestinationPath, LibraryPermissionTargetUse Use)
{
    public required LibraryPermissionEffect Effect { get; init; }
}

internal sealed record LibraryPermissionRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required LibraryRecord Library { get; init; }
    public required ImmutableArray<LibraryPermissionTarget> Targets { get; init; }
    public required bool AllowPrompt { get; init; }
}

internal sealed record LibraryPermissionRebinding(string PreviousSourceRoot, string SourceRoot);

internal sealed record LibraryPermissionApproval
{
    public required WorkspacePermissionEvaluation Leaves { get; init; }
    public required ImmutableArray<LibraryPermissionScope> ProposedScopes { get; init; }
    public required ImmutableArray<LibraryPermissionScope> ApprovedScopes { get; init; }
    public required LibraryPermissionRebinding? Rebinding { get; init; }
}
