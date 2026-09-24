using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Application;

internal enum ExtensionRemoveApplicationStage
{
    Directory,
    Settings,
    TargetEffect,
    Ownership,
    Verification,
    Cleanup,
}

internal sealed record ExtensionRemoveRecoveryCleanup(
    ExtensionRemoveRecovery Recovery,
    ExtensionRemoveFinding? Finding);
