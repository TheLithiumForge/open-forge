using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Application;

internal sealed record ExtensionRemoveExecutionPlan(
    ExtensionRemovePlan Content,
    ExtensionPermissionStage Permission,
    PlannedFileChange? SettingsChange,
    RecoveryBundleTarget? SettingsRecoveryTarget,
    ImmutableArray<PlannedDirectoryCreation> DirectoryCreations,
    ImmutableArray<RecoveryBundleTarget> RecoveryTargets)
{
    internal bool RequiresRecovery => RecoveryTargets.Any(target => target.RequiresRecovery);

    internal bool IsNoOp => Content.IsNoOp && SettingsChange is null;
}
