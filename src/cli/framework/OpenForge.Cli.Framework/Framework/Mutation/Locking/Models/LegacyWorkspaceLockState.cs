namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

internal enum LegacyWorkspaceLockState
{
    HeldReadWrite,
    HeldReadOnly,
    SkippedAbsent,
    Unavailable,
    Failed,
}
