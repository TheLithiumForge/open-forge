using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;

internal enum RelativeFileLinkEffectKind
{
    Create,
    Delete,
}

internal sealed record RelativeFileLinkState
{
    private RelativeFileLinkState(
        NoFollowLeafState state,
        RelativeFileLinkIdentity? link)
    {
        State = state;
        Link = link;
    }

    internal NoFollowLeafState State { get; }

    internal RelativeFileLinkIdentity? Link { get; }

    internal bool MatchesObservation(NoFollowLeafObservation observation)
        => State == observation.State
            && (Link is null || Link == observation.RelativeFileLink);

    internal static RelativeFileLinkState Missing { get; } = new(
        NoFollowLeafState.Missing,
        link: null);

    internal static RelativeFileLinkState LinkIdentity(RelativeFileLinkIdentity link)
    {
        ArgumentNullException.ThrowIfNull(link);
        return new RelativeFileLinkState(NoFollowLeafState.RelativeFileLink, link);
    }
}

internal sealed record RelativeFileLinkEffect
{
    private RelativeFileLinkEffect(
        RelativeFileLinkEffectKind kind,
        CanonicalRelativePath destinationPath,
        RelativeFileLinkIdentity link,
        RelativeFileLinkState expected,
        RelativeFileLinkState intended)
    {
        Kind = kind;
        DestinationPath = destinationPath;
        Link = link;
        Expected = expected;
        Intended = intended;
    }

    internal RelativeFileLinkEffectKind Kind { get; }

    // Library plans retain the repository-relative destination. The applier
    // resolves it against the leased workspace at the mutation boundary.
    internal CanonicalRelativePath DestinationPath { get; }

    internal CanonicalRelativePath LogicalPath => DestinationPath;

    internal RelativeFileLinkIdentity Link { get; }

    internal NoFollowLinkKind LinkKind => Link.LinkKind;

    internal string RawRelativeTarget => Link.RawRelativeTarget;

    internal RelativeFileLinkState Expected { get; }

    internal RelativeFileLinkState Intended { get; }

    internal static RelativeFileLinkEffect Create(
        CanonicalRelativePath destinationPath,
        RelativeFileLinkIdentity link)
    {
        ArgumentNullException.ThrowIfNull(destinationPath);
        ArgumentNullException.ThrowIfNull(link);
        return new RelativeFileLinkEffect(
            RelativeFileLinkEffectKind.Create,
            destinationPath,
            link,
            RelativeFileLinkState.Missing,
            RelativeFileLinkState.LinkIdentity(link));
    }

    internal static RelativeFileLinkEffect Delete(
        CanonicalRelativePath destinationPath,
        RelativeFileLinkIdentity link)
    {
        ArgumentNullException.ThrowIfNull(destinationPath);
        ArgumentNullException.ThrowIfNull(link);
        return new RelativeFileLinkEffect(
            RelativeFileLinkEffectKind.Delete,
            destinationPath,
            link,
            RelativeFileLinkState.LinkIdentity(link),
            RelativeFileLinkState.Missing);
    }

}
