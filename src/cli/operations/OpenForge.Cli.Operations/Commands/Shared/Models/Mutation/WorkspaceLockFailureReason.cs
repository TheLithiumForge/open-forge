namespace OpenForge.Cli.Core.Commands.Shared.Models.Mutation;

internal enum WorkspaceLockFailureReason
{
    Busy,
    AccessDenied,
    Unsupported,
    InvalidTarget,
    IoError,
    Cancelled,
}
