using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal static class LocalReferenceFactReader
{
    internal static LocalReferenceFragmentObservation ReadFragment(
        SourceLinkDestinationFacts facts)
    {
        if (facts.Fragment is not { } fragment)
        {
            return LocalReferenceFragmentObservation.NotRequested();
        }

        if (facts.CanonicalFragment is { } canonical)
        {
            return LocalReferenceFragmentObservation.Correction(
                canonical.Authored,
                canonical.Canonical);
        }

        return facts.Target.Resolution switch
        {
            SourceLinkTargetResolution.Complete => LocalReferenceFragmentObservation.Observed(
                LocalReferenceFragmentState.Verified,
                fragment),
            SourceLinkTargetResolution.FragmentMissing => LocalReferenceFragmentObservation.Observed(
                LocalReferenceFragmentState.Missing,
                fragment),
            SourceLinkTargetResolution.Unreadable or SourceLinkTargetResolution.EncodingUnsupported
                when facts.Target.Path is not null => LocalReferenceFragmentObservation.Observed(
                    LocalReferenceFragmentState.Unverified,
                    fragment),
            SourceLinkTargetResolution.Missing
                or SourceLinkTargetResolution.Malformed
                or SourceLinkTargetResolution.Absolute
                or SourceLinkTargetResolution.Query
                or SourceLinkTargetResolution.EncodingUnsupported
                or SourceLinkTargetResolution.OutsideWorkspace
                or SourceLinkTargetResolution.PhysicalEscape
                or SourceLinkTargetResolution.Ambiguous
                or SourceLinkTargetResolution.Unreadable
                or SourceLinkTargetResolution.Unsupported
                or SourceLinkTargetResolution.ExternalUnchecked => LocalReferenceFragmentObservation.NotRequested(),
            _ => throw new ArgumentOutOfRangeException(nameof(facts), facts.Target.Resolution, "The source-link target resolution is not defined."),
        };
    }

    internal static IReadOnlyList<LocalReferenceCanonicalization> ReadCanonicalizations(
        string sourcePath,
        string destination,
        SourceLocation? destinationLocation,
        SourceLinkDestinationFacts facts)
    {
        if (facts.Target is not { Kind: SourceLinkTargetKind.Local, Path: { } targetPath }
            || facts.Target.Resolution is not (SourceLinkTargetResolution.Complete
                or SourceLinkTargetResolution.FragmentMissing))
        {
            return [];
        }

        if (destinationLocation is null)
        {
            return [];
        }

        var hash = destination.IndexOf('#');
        var authoredPath = hash < 0 ? destination : destination[..hash];
        var suffix = hash < 0 ? string.Empty : destination[hash..];
        string decodedPath;
        try
        {
            decodedPath = Uri.UnescapeDataString(authoredPath);
        }
        catch (UriFormatException)
        {
            return [];
        }

        var sourceDirectory = Path.GetDirectoryName(sourcePath.Replace('/', Path.DirectorySeparatorChar))
            ?? string.Empty;
        var canonicalPath = Path.GetRelativePath(
                sourceDirectory.Length == 0 ? "." : sourceDirectory,
                targetPath.Replace('/', Path.DirectorySeparatorChar))
            .Replace(Path.DirectorySeparatorChar, '/');
        var findings = new List<LocalReferenceCanonicalization>();
        if (authoredPath.Length > 0
            && !string.Equals(authoredPath, canonicalPath, StringComparison.Ordinal))
        {
            var kind = LocalReferenceCanonicalizationKind.Path;
            if (string.Equals(decodedPath, canonicalPath, StringComparison.Ordinal))
            {
                kind = LocalReferenceCanonicalizationKind.Encoding;
            }
            else if (string.Equals(decodedPath, canonicalPath, StringComparison.OrdinalIgnoreCase))
            {
                kind = LocalReferenceCanonicalizationKind.Case;
            }

            findings.Add(new LocalReferenceCanonicalization(
                kind: kind,
                expected: destination,
                intended: $"{canonicalPath}{suffix}",
                destinationLocation: destinationLocation));
        }

        if (facts.CanonicalFragment is { } fragment)
        {
            var intendedPath = authoredPath.Length == 0 ? string.Empty : canonicalPath;
            findings.Add(new LocalReferenceCanonicalization(
                kind: LocalReferenceCanonicalizationKind.Fragment,
                expected: destination,
                intended: $"{intendedPath}#{Uri.EscapeDataString(fragment.Canonical)}",
                destinationLocation: destinationLocation));
        }

        return findings;
    }

}
