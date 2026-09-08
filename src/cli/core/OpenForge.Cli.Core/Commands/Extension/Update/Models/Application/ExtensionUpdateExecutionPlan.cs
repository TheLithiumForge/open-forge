using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Models.Permissions;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Application;

internal sealed class ExtensionUpdateExecutionPlan
{
    internal ExtensionUpdateExecutionPlan(ExtensionUpdatePlan content, ExtensionPermissionStage permission)
    {
        Content = content;
        Permission = permission;
        RecoveryTargets = permission.RecoveryTarget is { } target
            ? [target, .. content.RecoveryTargets]
            : [.. content.RecoveryTargets];
    }

    internal ExtensionUpdatePlan Content { get; }

    internal ExtensionPermissionStage Permission { get; }

    internal ImmutableArray<RecoveryBundleTarget> RecoveryTargets { get; }

    internal bool RequiresRecovery => RecoveryTargets.Any(target => target.RequiresRecovery);

    internal bool IsNoOp => Content.IsNoOp && Permission.Change is null;
}
