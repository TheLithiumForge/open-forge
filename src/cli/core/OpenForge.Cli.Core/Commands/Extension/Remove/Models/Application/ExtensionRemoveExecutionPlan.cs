using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Models.Permissions;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Application;

internal sealed class ExtensionRemoveExecutionPlan
{
    internal ExtensionRemoveExecutionPlan(ExtensionRemovePlan content, ExtensionPermissionStage permission)
    {
        Content = content;
        Permission = permission;
        var contentTargets = content.Effects.Select(effect => effect.RecoveryTarget)
            .Append(content.LifecycleRecoveryTarget).OfType<RecoveryBundleTarget>();
        RecoveryTargets = permission.RecoveryTarget is { } target
            ? [target, .. contentTargets]
            : [.. contentTargets];
    }

    internal ExtensionRemovePlan Content { get; }

    internal ExtensionPermissionStage Permission { get; }

    internal ImmutableArray<RecoveryBundleTarget> RecoveryTargets { get; }

    internal bool RequiresRecovery => RecoveryTargets.Any(target => target.RequiresRecovery);

    internal bool IsNoOp => Content.IsNoOp && Permission.Change is null;
}
