using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Application;

internal sealed record ExtensionUpdateApplicationOutcome
{
    internal WorkspacePermissionResult Permissions { get; init; } = WorkspacePermissionResult.NotEvaluated;

    internal required IReadOnlyList<ExtensionUpdateEffect> Effects { get; init; }

    internal required ExtensionUpdateLifecycle Lifecycle { get; init; }

    internal required ExtensionUpdateRecovery Recovery { get; init; }

    internal required ExtensionUpdateVerification Verification { get; init; }

    internal ExtensionUpdateFinding? Finding { get; init; }
}

internal sealed record ExtensionUpdateRecoveryCleanup(
    ExtensionUpdateRecovery Recovery,
    ExtensionUpdateFinding? Finding);
