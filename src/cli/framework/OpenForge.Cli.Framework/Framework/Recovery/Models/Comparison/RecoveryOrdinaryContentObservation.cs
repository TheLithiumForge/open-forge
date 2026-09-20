using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;

internal sealed record RecoveryOrdinaryContentObservation
{
    internal RecoveryOrdinaryContentObservation(
        string logicalPath,
        RecoveryContentIdentity? identity,
        FilesystemFailure? failure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalPath);
        if (!Path.IsPathFullyQualified(logicalPath))
        {
            throw new ArgumentException("An ordinary recovery read requires an absolute observed target path.", nameof(logicalPath));
        }

        if ((identity is null) == (failure is null))
        {
            throw new ArgumentException("An ordinary recovery read requires exactly one identity or failure fact.", nameof(identity));
        }

        LogicalPath = Path.GetFullPath(logicalPath);
        Identity = identity;
        Failure = failure;
    }

    internal string LogicalPath { get; }
    internal RecoveryContentIdentity? Identity { get; }
    internal FilesystemFailure? Failure { get; }
}
