using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Entries;

internal enum RecoveryEntryStateKind
{
    Missing,
    OrdinaryFile,
    RelativeFileLink,
}

internal sealed record RecoveryEntryState
{
    private RecoveryEntryState(
        RecoveryEntryStateKind kind,
        RecoveryContentIdentity? ordinaryFile,
        RelativeFileLinkIdentity? relativeFileLink)
    {
        Kind = kind;
        OrdinaryFile = ordinaryFile;
        RelativeFileLink = relativeFileLink;
    }

    internal RecoveryEntryStateKind Kind { get; }

    internal RecoveryContentIdentity? OrdinaryFile { get; }

    internal RelativeFileLinkIdentity? RelativeFileLink { get; }

    internal static RecoveryEntryState Missing { get; } = new(
        RecoveryEntryStateKind.Missing,
        ordinaryFile: null,
        relativeFileLink: null);

    internal static RecoveryEntryState Ordinary(RecoveryContentIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        return new RecoveryEntryState(
            RecoveryEntryStateKind.OrdinaryFile,
            identity,
            relativeFileLink: null);
    }

    internal static RecoveryEntryState RelativeLink(RelativeFileLinkIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        return new RecoveryEntryState(
            RecoveryEntryStateKind.RelativeFileLink,
            ordinaryFile: null,
            identity);
    }
}
