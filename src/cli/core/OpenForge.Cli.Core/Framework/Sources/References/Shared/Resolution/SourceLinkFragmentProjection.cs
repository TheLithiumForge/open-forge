using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;

internal static class SourceLinkFragmentProjection
{
    internal static SourceLinkDestinationFacts? ReadFailure(
        IReadOnlyList<MarkdownHeadingFact> headings,
        SourceLinkTarget target,
        string rawFragment,
        string? decodedFragment)
    {
        if (headings.Any(heading =>
                heading.IsCanonical
                && heading.FragmentIdentifier is not null
                && string.Equals(heading.FragmentIdentifier, decodedFragment, StringComparison.Ordinal)))
        {
            return null;
        }

        var canonicalFragmentCandidates = headings
            .Where(heading => heading.IsCanonical
                && heading.FragmentIdentifier is not null
                && string.Equals(
                    heading.FragmentIdentifier,
                    decodedFragment,
                    StringComparison.OrdinalIgnoreCase))
            .Select(heading => heading.FragmentIdentifier)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (canonicalFragmentCandidates.Length == 1)
        {
            return new SourceLinkDestinationFacts
            {
                Fragment = rawFragment,
                Target = target with { Resolution = SourceLinkTargetResolution.FragmentMissing },
                Finding = new SourceLinkDestinationFinding
                {
                    Code = SourceLinkDestinationFindingCode.FragmentMissing,
                    Cause = "The authored fragment differs from one exact canonical fragment spelling.",
                    Candidates = [],
                },
            }.WithCanonicalFragment(new SourceLinkCanonicalFragment(
                authored: rawFragment,
                canonical: canonicalFragmentCandidates[0]));
        }

        if (headings.Any(heading => heading.IsCanonical && heading.FragmentIdentifier is null))
        {
            return new SourceLinkDestinationFacts
            {
                Fragment = rawFragment,
                Target = target with { Resolution = SourceLinkTargetResolution.Unreadable },
                Finding = new SourceLinkDestinationFinding
                {
                    Code = SourceLinkDestinationFindingCode.TargetUnreadable,
                    Cause = "A canonical target heading has no available fragment identifier.",
                    Candidates = [],
                },
            };
        }

        return new SourceLinkDestinationFacts
        {
            Fragment = rawFragment,
            Target = target with { Resolution = SourceLinkTargetResolution.FragmentMissing },
            Finding = new SourceLinkDestinationFinding
            {
                Code = SourceLinkDestinationFindingCode.FragmentMissing,
                Cause = "The authored fragment is not present on the target.",
                Candidates = [],
            },
        };
    }
}
