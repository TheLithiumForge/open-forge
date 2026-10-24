using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Shared.Mutation;

internal static class WorkspaceLockFailureProjection
{
    internal static WorkspaceLockFailureInfo From(WorkspaceLockFailure failure)
        => new(
            failure.Kind switch
            {
                WorkspaceLockFailureKind.Busy => WorkspaceLockFailureReason.Busy,
                WorkspaceLockFailureKind.AccessDenied => WorkspaceLockFailureReason.AccessDenied,
                WorkspaceLockFailureKind.Unsupported => WorkspaceLockFailureReason.Unsupported,
                WorkspaceLockFailureKind.InvalidTarget => WorkspaceLockFailureReason.InvalidTarget,
                WorkspaceLockFailureKind.IoError => WorkspaceLockFailureReason.IoError,
                WorkspaceLockFailureKind.Cancelled => WorkspaceLockFailureReason.Cancelled,
                _ => throw new ArgumentOutOfRangeException(nameof(failure)),
            },
            failure.Path);
}
