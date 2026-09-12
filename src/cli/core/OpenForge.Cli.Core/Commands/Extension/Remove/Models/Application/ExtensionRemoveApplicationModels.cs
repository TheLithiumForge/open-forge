using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Application;

internal enum ExtensionRemoveApplicationStage
{
    TargetEffect,
    Lifecycle,
    Verification,
    Cleanup,
}

internal sealed record ExtensionRemoveRecoveryCleanup(
    ExtensionRemoveRecovery Recovery,
    ExtensionRemoveFinding? Finding);
