namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

internal enum WorkspaceLockFailureKind
{
    Busy,
    AccessDenied,
    Unsupported,
    InvalidTarget,
    IoError,
    Cancelled,
}
