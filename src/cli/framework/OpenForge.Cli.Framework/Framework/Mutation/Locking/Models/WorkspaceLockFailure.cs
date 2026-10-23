using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

internal sealed record WorkspaceLockFailure(
    WorkspaceLockFailureKind Kind,
    string Path,
    FilesystemFailure? Detail = null,
    int? NativeError = null);
