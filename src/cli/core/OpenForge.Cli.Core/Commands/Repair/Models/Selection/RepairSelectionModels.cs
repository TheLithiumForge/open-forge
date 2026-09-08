using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Selection;

internal enum RepairCatalogueMember
{
    SameTargetPath,
    SameTargetCase,
    SameTargetEncoding,
    UniqueCanonicalFragment,
    MissingTargetRelink,
}

internal enum RepairSelectionOrigin
{
    Automatic,
    Wizard,
    ExplicitRelink,
}

internal enum RepairCandidateEvidenceKind
{
    Filename,
    Title,
    LiteralContent,
    RouteNeighborhood,
}

internal enum RepairCandidateCardinality
{
    None,
    One,
    Several,
}

internal sealed record RepairCandidateEvidence
{
    internal RepairCandidateEvidence(
        RepairCandidateEvidenceKind kind,
        string value,
        SourceLocation? location)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Repair candidate evidence kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Kind = kind;
        Value = value;
        Location = location;
    }

    internal RepairCandidateEvidenceKind Kind { get; }

    internal string Value { get; }

    internal SourceLocation? Location { get; }
}

internal sealed record RepairTargetSelection
{
    internal RepairTargetSelection(
        string canonicalTargetPath,
        string? targetFragment,
        IEnumerable<RepairCandidateEvidence>? candidateProvenance = null)
    {
        var validatedPath = SourceWorkspaceRelativePath.Validate(
            canonicalTargetPath,
            nameof(canonicalTargetPath),
            allowWorkspaceRoot: false);
        if (validatedPath.Contains('#') || validatedPath.Contains('?'))
        {
            throw new ArgumentException(
                "A Repair target path cannot contain an unencoded fragment or query delimiter.",
                nameof(canonicalTargetPath));
        }

        candidateProvenance ??= [];
        CanonicalTargetPath = validatedPath;
        TargetFragment = ValidateFragment(targetFragment);
        CandidateProvenance = new ReadOnlyCollection<RepairCandidateEvidence>([.. candidateProvenance
            .Select(evidence => evidence ?? throw new ArgumentException(
                "Repair candidate provenance cannot contain null members.",
                nameof(candidateProvenance)))]);
    }

    internal string CanonicalTargetPath { get; }

    internal string? TargetFragment { get; }

    internal IReadOnlyList<RepairCandidateEvidence> CandidateProvenance { get; }

    internal static RepairTargetSelection Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var delimiter = value.IndexOf('#');
        var path = delimiter < 0 ? value : value[..delimiter];
        var fragment = delimiter < 0 ? null : value[(delimiter + 1)..];
        if (path.Length == 0
            || value[(delimiter + 1)..].Contains('#')
            || value.Contains('?')
            || delimiter >= 0 && fragment is { Length: 0 })
        {
            throw new ArgumentException(
                "A Repair target must be one contained path with at most one non-empty fragment.",
                nameof(value));
        }

        return new RepairTargetSelection(path, fragment);
    }

    private static string? ValidateFragment(string? fragment)
    {
        if (fragment is null)
        {
            return null;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fragment, nameof(fragment));
        if (fragment.Contains('#')
            || fragment.Contains('?')
            || fragment.Any(char.IsControl))
        {
            throw new ArgumentException(
                "A Repair target fragment must be one non-empty Markdown fragment spelling.",
                nameof(fragment));
        }

        return fragment;
    }
}

internal sealed record RepairCandidate
{
    internal RepairCandidate(
        RepairTargetSelection target,
        IEnumerable<RepairCandidateEvidence> evidence,
        bool recommendedForReview = false)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(evidence);
        var evidenceValues = evidence
            .Select(value => value ?? throw new ArgumentException(
                "Repair candidate evidence cannot contain null members.",
                nameof(evidence)))
            .ToArray();
        if (evidenceValues.Length == 0)
        {
            throw new ArgumentException(
                "A Repair candidate requires visible evidence.",
                nameof(evidence));
        }

        Target = target;
        Evidence = new ReadOnlyCollection<RepairCandidateEvidence>(evidenceValues);
        RecommendedForReview = recommendedForReview;
    }

    internal RepairTargetSelection Target { get; }

    internal IReadOnlyList<RepairCandidateEvidence> Evidence { get; }

    internal bool RecommendedForReview { get; }
}

internal sealed record RepairCandidateSet
{
    internal RepairCandidateSet(IEnumerable<RepairCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        var values = candidates
            .Select(value => value ?? throw new ArgumentException(
                "Repair candidates cannot contain null members.",
                nameof(candidates)))
            .ToArray();
        var targets = new HashSet<(string Path, string? Fragment)>();
        foreach (var candidate in values)
        {
            if (!targets.Add((candidate.Target.CanonicalTargetPath, candidate.Target.TargetFragment)))
            {
                throw new ArgumentException(
                    "Repair candidate targets must be unique within one candidate set.",
                    nameof(candidates));
            }
        }

        if (values.Count(candidate => candidate.RecommendedForReview) > 1)
        {
            throw new ArgumentException(
                "A Repair candidate set can recommend at most one candidate.",
                nameof(candidates));
        }

        Items = new ReadOnlyCollection<RepairCandidate>(values);
        Cardinality = values.Length switch
        {
            0 => RepairCandidateCardinality.None,
            1 => RepairCandidateCardinality.One,
            _ => RepairCandidateCardinality.Several,
        };
    }

    internal RepairCandidateCardinality Cardinality { get; }

    internal IReadOnlyList<RepairCandidate> Items { get; }
}

internal sealed record RepairProposalResolution
{
    internal RepairProposalResolution(
        string intendedDestination,
        RepairTargetSelection target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(intendedDestination);
        ArgumentNullException.ThrowIfNull(target);
        IntendedDestination = intendedDestination;
        Target = target;
    }

    internal string IntendedDestination { get; }

    internal RepairTargetSelection Target { get; }
}

internal sealed record RepairProposal
{
    internal RepairProposal(
        RepairCatalogueMember member,
        string sourceCanonicalPath,
        SourceLocation occurrence,
        string expectedDestination,
        RepairProposalResolution? resolution,
        RepairCandidateSet? candidates)
    {
        if (!Enum.IsDefined(member))
        {
            throw new ArgumentOutOfRangeException(
                nameof(member),
                member,
                "The Repair catalogue member is not defined.");
        }

        SourceCanonicalPath = SourceWorkspaceRelativePath.ValidateMarkdown(
            sourceCanonicalPath,
            nameof(sourceCanonicalPath));
        ArgumentNullException.ThrowIfNull(occurrence);
        if (occurrence.ByteLength < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                occurrence.ByteLength,
                "A Repair proposal must identify a non-empty destination span.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(expectedDestination);
        if (member == RepairCatalogueMember.MissingTargetRelink && resolution is not null)
        {
            throw new ArgumentException(
                "A guided Repair proposal cannot carry a selected resolution.",
                nameof(resolution));
        }

        if (member == RepairCatalogueMember.MissingTargetRelink && candidates is null)
        {
            throw new ArgumentException(
                "A guided Repair proposal requires bounded candidate evidence.",
                nameof(candidates));
        }

        if (member != RepairCatalogueMember.MissingTargetRelink && resolution is null)
        {
            throw new ArgumentException(
                "A safe-exact Repair proposal requires one exact resolution.",
                nameof(resolution));
        }

        if (member != RepairCatalogueMember.MissingTargetRelink && candidates is not null)
        {
            throw new ArgumentException(
                "A safe-exact Repair proposal cannot carry guided candidates.",
                nameof(candidates));
        }

        if (resolution is not null
            && string.Equals(
                expectedDestination,
                resolution.IntendedDestination,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A Repair proposal must describe a changed destination.",
                nameof(resolution));
        }

        Member = member;
        Occurrence = occurrence;
        ExpectedDestination = expectedDestination;
        Resolution = resolution;
        Candidates = candidates;
    }

    internal RepairProposal(
        RepairCatalogueMember member,
        string sourceCanonicalPath,
        SourceLocation occurrence,
        string expectedDestination,
        string intendedDestination,
        RepairTargetSelection target)
        : this(
            member,
            sourceCanonicalPath,
            occurrence,
            expectedDestination,
            new RepairProposalResolution(intendedDestination, target),
            candidates: null)
    {
    }

    internal RepairProposal(
        RepairCatalogueMember member,
        string sourceCanonicalPath,
        SourceLocation occurrence,
        string expectedDestination,
        RepairCandidateSet candidates)
        : this(
            member,
            sourceCanonicalPath,
            occurrence,
            expectedDestination,
            resolution: null,
            candidates: candidates)
    {
    }

    internal RepairCatalogueMember Member { get; }

    internal string SourceCanonicalPath { get; }

    internal SourceLocation Occurrence { get; }

    internal string ExpectedDestination { get; }

    internal RepairProposalResolution? Resolution { get; }

    internal string? IntendedDestination => Resolution?.IntendedDestination;

    internal RepairTargetSelection? Target => Resolution?.Target;

    internal RepairCandidateSet? Candidates { get; }

    internal bool IsSafeExact
        => Member != RepairCatalogueMember.MissingTargetRelink && Resolution is not null;

    internal bool IsGuided
        => Member == RepairCatalogueMember.MissingTargetRelink && Resolution is null && Candidates is not null;
}

internal sealed record RepairSelectedProposal
{
    internal RepairSelectedProposal(
        RepairProposal proposal,
        RepairProposalResolution resolution,
        IEnumerable<RepairSelectionOrigin> origins)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(origins);
        var originValues = origins
            .Select(origin =>
            {
                if (!Enum.IsDefined(origin))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(origins),
                        origin,
                        "The Repair selection origin is not defined.");
                }

                return origin;
            })
            .Distinct()
            .ToArray();
        if (originValues.Length == 0)
        {
            throw new ArgumentException(
                "A selected Repair proposal requires at least one selection origin.",
                nameof(origins));
        }

        if (proposal.IsSafeExact)
        {
            if (proposal.Resolution is null || !Matches(proposal.Resolution.Target, resolution.Target)
                || !string.Equals(
                    proposal.Resolution.IntendedDestination,
                    resolution.IntendedDestination,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "A selected safe-exact Repair proposal must retain its exact resolution.",
                    nameof(resolution));
            }
        }
        else if (proposal.IsGuided)
        {
            if (originValues.Contains(RepairSelectionOrigin.Automatic))
            {
                throw new ArgumentException(
                    "Automatic Repair selection cannot select a guided proposal.",
                    nameof(origins));
            }

            if (proposal.Candidates is null
                || !proposal.Candidates.Items.Any(candidate => Matches(candidate.Target, resolution.Target)))
            {
                throw new ArgumentException(
                    "A selected guided Repair target must be one of the bounded candidates.",
                    nameof(resolution));
            }
        }
        else
        {
            throw new ArgumentException(
                "A selected Repair proposal must be resolved and catalogue-valid.",
                nameof(proposal));
        }

        if (string.Equals(
            proposal.ExpectedDestination,
            resolution.IntendedDestination,
            StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A selected Repair resolution must change the authored destination.",
                nameof(resolution));
        }

        Proposal = proposal;
        Resolution = resolution;
        Origins = new ReadOnlyCollection<RepairSelectionOrigin>(originValues);
    }

    internal RepairProposal Proposal { get; }

    internal RepairProposalResolution Resolution { get; }

    internal IReadOnlyList<RepairSelectionOrigin> Origins { get; }

    private static bool Matches(RepairTargetSelection left, RepairTargetSelection right)
        => string.Equals(left.CanonicalTargetPath, right.CanonicalTargetPath, StringComparison.Ordinal)
            && string.Equals(left.TargetFragment, right.TargetFragment, StringComparison.Ordinal);
}

internal sealed record RepairCatalogue
{
    internal RepairCatalogue(IEnumerable<RepairProposal> proposals)
    {
        ArgumentNullException.ThrowIfNull(proposals);
        var values = proposals
            .Select(value => value ?? throw new ArgumentException(
                "Repair catalogue proposals cannot contain null members.",
                nameof(proposals)))
            .ToArray();
        if (values
            .GroupBy(value => (value.SourceCanonicalPath, value.Occurrence.Line, value.Occurrence.Column))
            .Any(group => group.Count() != 1))
        {
            throw new ArgumentException(
                "A Repair catalogue cannot contain multiple proposals for one source occurrence.",
                nameof(proposals));
        }

        SafeExact = new ReadOnlyCollection<RepairProposal>([.. values.Where(value => value.IsSafeExact)]);
        Guided = new ReadOnlyCollection<RepairProposal>([.. values.Where(value => value.IsGuided)]);
        Proposals = new ReadOnlyCollection<RepairProposal>(values);
    }

    internal IReadOnlyList<RepairProposal> Proposals { get; }

    internal IReadOnlyList<RepairProposal> SafeExact { get; }

    internal IReadOnlyList<RepairProposal> Guided { get; }
}

internal sealed record RepairSelection
{
    internal RepairSelection(
        RepairSelectionMode mode,
        IEnumerable<RepairSelectedProposal> selected,
        IEnumerable<RepairProposal> unselected,
        RepairLibrarySelection libraries)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Repair selection mode is not defined.");
        }

        ArgumentNullException.ThrowIfNull(selected);
        ArgumentNullException.ThrowIfNull(unselected);
        ArgumentNullException.ThrowIfNull(libraries);
        Libraries = libraries;
        Mode = mode;
        var selectedValues = SnapshotSelected(selected, nameof(selected));
        var unselectedValues = SnapshotUnselected(unselected, nameof(unselected));
        ValidateProposalOccurrences(selectedValues, unselectedValues);
        Selected = selectedValues;
        Unselected = unselectedValues;
    }

    internal RepairSelectionMode Mode { get; }

    internal RepairLibrarySelection Libraries { get; }

    internal IReadOnlyList<RepairSelectedProposal> Selected { get; }

    internal IReadOnlyList<RepairProposal> Unselected { get; }

    private static ReadOnlyCollection<RepairSelectedProposal> SnapshotSelected(
        IEnumerable<RepairSelectedProposal> values,
        string parameterName)
        => new([.. values
            .Select(value => value ?? throw new ArgumentException(
                "Repair selections cannot contain null members.",
                parameterName))]);

    private static ReadOnlyCollection<RepairProposal> SnapshotUnselected(
        IEnumerable<RepairProposal> values,
        string parameterName)
        => new([.. values
            .Select(value => value ?? throw new ArgumentException(
                "Repair selections cannot contain null members.",
                parameterName))]);

    private static void ValidateProposalOccurrences(
        IReadOnlyList<RepairSelectedProposal> selected,
        IReadOnlyList<RepairProposal> unselected)
    {
        var occurrences = selected
            .Select(value => value.Proposal)
            .Concat(unselected)
            .Select(value => (
                SourcePath: value.SourceCanonicalPath, value.Occurrence.Line, value.Occurrence.Column,
                Offset: value.Occurrence.ByteOffset,
                Length: value.Occurrence.ByteLength))
            .OrderBy(value => value.SourcePath, StringComparer.Ordinal)
            .ThenBy(value => value.Offset)
            .ThenBy(value => value.Length)
            .ToArray();

        var identities = new HashSet<(string SourcePath, int Line, int Column)>();
        string? previousSourcePath = null;
        long previousEnd = 0;
        foreach (var (SourcePath, Line, Column, Offset, Length) in occurrences)
        {
            if (!identities.Add((SourcePath, Line, Column)))
            {
                throw new ArgumentException(
                    "Repair selection proposal occurrences must be unique and non-overlapping across selected and unselected proposals.",
                    nameof(selected));
            }

            if (string.Equals(previousSourcePath, SourcePath, StringComparison.Ordinal)
                && Offset < previousEnd)
            {
                throw new ArgumentException(
                    "Repair selection proposal occurrences must be unique and non-overlapping across selected and unselected proposals.",
                    nameof(selected));
            }

            previousSourcePath = SourcePath;
            previousEnd = checked(Offset + Length);
        }
    }
}
