using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
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
    public required LibraryId LibraryId { get; init; }
    public required WorkspaceSettingsRead SettingsObservation { get; init; }
    public required ImmutableArray<LibraryPermissionTarget> Targets { get; init; }
    public WorkspaceRemovalSelection? RemovalSelection { get; init; }

    /// <summary>Paths supplied by the caller as explicit grants.</summary>
    public ImmutableArray<string> ExplicitGrantPaths { get; init; } = [];

    public required bool AllowPrompt { get; init; }
}

internal sealed record LibraryPermissionApproval
{
    public required WorkspacePermissionEvaluation Leaves { get; init; }
    public required ImmutableArray<LibraryPermissionScope> ProposedScopes { get; init; }
    public required ImmutableArray<LibraryPermissionScope> ApprovedScopes { get; init; }
}
