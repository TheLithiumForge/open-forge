using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Find.Models.Projection;

internal sealed record FindMetadataProjectionInput(
    FindProjectionSourceFacts Source,
    int Position);

internal sealed record FindPhysicalProjectionInput(
    FindSourceIdentity SourceIdentity,
    FindProjectionLayer Layer,
    FindContentPart Part);

internal sealed record FindProjectionBuildFacts
{
    internal FindProjectionBuildFacts(
        FindProjection projection,
        IEnumerable<FindFinding> findings,
        bool uncertain)
    {
        Projection = projection;
        Findings = Array.AsReadOnly(findings.ToArray());
        Uncertain = uncertain;
    }

    internal FindProjection Projection { get; }

    internal IReadOnlyList<FindFinding> Findings { get; }

    internal bool Uncertain { get; }
}

internal sealed record FindProjectionFindingInput(
    FindFindingCode Code,
    FindSourceIdentity Source,
    FindProjectionLayer? Layer,
    FindRegion? Region,
    FindContentPartKind Part,
    string Cause);

internal sealed record FindMissingSectionFindingInput
{
    internal FindMissingSectionFindingInput(
        FindSourceIdentity sourceIdentity,
        IEnumerable<FindContentPart> requestedParts,
        IEnumerable<FindProjection> matchProjections)
    {
        SourceIdentity = sourceIdentity;
        RequestedParts = Array.AsReadOnly(requestedParts.ToArray());
        MatchProjections = Array.AsReadOnly(matchProjections.ToArray());
    }

    internal FindSourceIdentity SourceIdentity { get; }

    internal IReadOnlyList<FindContentPart> RequestedParts { get; }

    internal IReadOnlyList<FindProjection> MatchProjections { get; }
}
