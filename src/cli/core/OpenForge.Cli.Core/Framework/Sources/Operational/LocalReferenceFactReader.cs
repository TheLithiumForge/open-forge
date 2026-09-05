using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;

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
            _ => LocalReferenceFragmentObservation.NotRequested(),
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
            var kind = string.Equals(decodedPath, canonicalPath, StringComparison.Ordinal)
                ? LocalReferenceCanonicalizationKind.Encoding
                : string.Equals(decodedPath, canonicalPath, StringComparison.OrdinalIgnoreCase)
                    ? LocalReferenceCanonicalizationKind.Case
                    : LocalReferenceCanonicalizationKind.Path;
            findings.Add(new LocalReferenceCanonicalization(
                kind,
                destination,
                canonicalPath + suffix,
                destinationLocation));
        }

        if (facts.CanonicalFragment is { } fragment)
        {
            findings.Add(new LocalReferenceCanonicalization(
                LocalReferenceCanonicalizationKind.Fragment,
                destination,
                (authoredPath.Length == 0 ? string.Empty : canonicalPath)
                    + "#" + Uri.EscapeDataString(fragment.Canonical),
                destinationLocation));
        }

        return findings;
    }

}
