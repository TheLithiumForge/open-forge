using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models;

internal enum LocalReferenceFragmentState
{
    NotRequested,
    Verified,
    Missing,
    Unverified,
    CanonicalCorrection,
}

internal sealed class LocalReferenceFragmentObservation
{
    private LocalReferenceFragmentObservation(LocalReferenceFragmentState state, string? authored, string? canonical)
    {
        State = state;
        Authored = authored;
        Canonical = canonical;
    }

    internal LocalReferenceFragmentState State { get; }

    internal string? Authored { get; }

    internal string? Canonical { get; }

    internal static LocalReferenceFragmentObservation NotRequested()
        => new(LocalReferenceFragmentState.NotRequested, authored: null, canonical: null);

    internal static LocalReferenceFragmentObservation Observed(LocalReferenceFragmentState state, string authored)
    {
        if (state is not (LocalReferenceFragmentState.Verified or LocalReferenceFragmentState.Missing or LocalReferenceFragmentState.Unverified))
        {
            throw new ArgumentException("An observed fragment state must be verified, missing, or unverified.", nameof(state));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(authored);
        return new(state, authored, canonical: null);
    }

    internal static LocalReferenceFragmentObservation Correction(string authored, string canonical)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authored);
        ArgumentException.ThrowIfNullOrWhiteSpace(canonical);
        if (string.Equals(authored, canonical, StringComparison.Ordinal))
        {
            throw new ArgumentException("A canonical fragment correction must change the authored spelling.", nameof(canonical));
        }

        return new(LocalReferenceFragmentState.CanonicalCorrection, authored, canonical);
    }
}

internal enum LocalReferenceCanonicalizationKind
{
    Path,
    Case,
    Encoding,
    Fragment,
}

internal sealed record LocalReferenceCanonicalization
{
    internal LocalReferenceCanonicalization(
        LocalReferenceCanonicalizationKind kind,
        string expected,
        string intended,
        SourceLocation destinationLocation)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The canonicalization kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(expected);
        ArgumentException.ThrowIfNullOrWhiteSpace(intended);
        ArgumentNullException.ThrowIfNull(destinationLocation);
        if (string.Equals(expected, intended, StringComparison.Ordinal))
        {
            throw new ArgumentException("A local-reference canonicalization must change the authored value.", nameof(intended));
        }

        Kind = kind;
        Expected = expected;
        Intended = intended;
        DestinationLocation = destinationLocation;
    }

    internal LocalReferenceCanonicalizationKind Kind { get; }

    internal string Expected { get; }

    internal string Intended { get; }

    internal SourceLocation DestinationLocation { get; }
}
