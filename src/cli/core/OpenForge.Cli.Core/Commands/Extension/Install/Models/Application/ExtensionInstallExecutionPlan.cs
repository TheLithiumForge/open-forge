using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Models.Permissions;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;

internal sealed class ExtensionInstallExecutionPlan
{
    internal ExtensionInstallExecutionPlan(ExtensionInstallPlan content, ExtensionPermissionStage permission)
    {
        Content = content;
        Permission = permission;
        RecoveryTargets = permission.RecoveryTarget is { } target
            ? [target, .. content.RecoveryTargets]
            : [.. content.RecoveryTargets];
    }

    internal ExtensionInstallPlan Content { get; }

    internal ExtensionPermissionStage Permission { get; }

    internal ImmutableArray<RecoveryBundleTarget> RecoveryTargets { get; }

    internal bool RequiresRecovery => RecoveryTargets.Any(target => target.RequiresRecovery);

    internal bool IsNoOp => Content.IsNoOp && Permission.Change is null;
}
