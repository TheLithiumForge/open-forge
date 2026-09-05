using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models;

internal enum LocalReferenceKind
{
    Link,
    Image,
}

internal sealed record LocalReferenceObservation
{
    public required string SourcePath { get; init; }

    public required string RoutePath { get; init; }

    public required LocalReferenceKind Kind { get; init; }

    public required string Destination { get; init; }

    public required MarkdownLinkLabelFact Label { get; init; }

    public required SourceLocation? Location { get; init; }

    public required SourceLocation? DestinationLocation { get; init; }

    public required SourceLinkDestinationFacts Facts { get; init; }

    public required LocalReferenceFragmentObservation Fragment { get; init; }

    public required IReadOnlyList<LocalReferenceCanonicalization> Canonicalizations { get; init; }
}

internal enum LocalReferenceCandidateScanState
{
    Complete,
    Incomplete,
    Blocked,
    Interrupted,
}

internal enum LocalReferenceCandidateBasisKind
{
    Filename,
    Title,
    LiteralContent,
    RouteNeighborhood,
}

internal sealed record LocalReferenceCandidateBasis
{
    private LocalReferenceCandidateBasis(
        LocalReferenceCandidateBasisKind kind,
        string value,
        SourceLocation? location)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The local-reference candidate basis is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Kind = kind;
        Value = value;
        Location = location;
    }

    internal LocalReferenceCandidateBasisKind Kind { get; }

    internal string Value { get; }

    internal SourceLocation? Location { get; }

    internal static LocalReferenceCandidateBasis Create(
        LocalReferenceCandidateBasisKind kind,
        string value,
        SourceLocation? location)
        => new(kind, value, location);
}

internal sealed class LocalReferenceCandidateObservation
{
    private LocalReferenceCandidateObservation(
        string sourcePath,
        string destination,
        SourceLinkIdentity candidate,
        IReadOnlyList<LocalReferenceCandidateBasis> bases)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(bases);
        if (bases.Count == 0 || bases.Any(basis => basis is null))
        {
            throw new ArgumentException(
                "A local-reference candidate requires at least one typed basis.",
                nameof(bases));
        }

        SourcePath = sourcePath;
        Destination = destination;
        Candidate = candidate;
        Bases = bases.ToArray();
    }

    internal string SourcePath { get; }

    internal string Destination { get; }

    internal SourceLinkIdentity Candidate { get; }

    internal IReadOnlyList<LocalReferenceCandidateBasis> Bases { get; }

    internal static LocalReferenceCandidateObservation Create(
        string sourcePath,
        string destination,
        SourceLinkIdentity candidate,
        IReadOnlyList<LocalReferenceCandidateBasis> bases)
        => new(sourcePath, destination, candidate, bases);
}

internal sealed record LocalReferenceDoctorView(
    OperationalViewState State,
    IReadOnlyList<LocalReferenceObservation> References,
    LocalReferenceCandidateScanState CandidateScan,
    IReadOnlyList<LocalReferenceCandidateObservation> Candidates,
    IReadOnlyList<LocalReferenceGraphFinding> GraphFindings);
