using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Application;

internal sealed record ExtensionUpdateApplicationFailureContext
{
    internal IReadOnlyList<ExtensionUpdateEffect> Effects { get; init; } = [];
    internal WorkspacePermissionResult? Permissions { get; init; }
    internal RecoveryBundlePreparation? Preparation { get; init; }
    internal ExtensionUpdateLifecycleOutcome LifecycleOutcome { get; init; } = ExtensionUpdateLifecycleOutcome.NotStarted;
    internal string? RecoveryResidualPath { get; init; }
    internal bool RecoveryUnknown { get; init; }
}
