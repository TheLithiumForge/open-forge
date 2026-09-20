using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Find.Models.Matching;

internal sealed record FindPredicateMatchInput(
    FindPredicate Predicate,
    int PredicateIndex,
    FindRegionSelection Regions,
    FindLayerFacts Layer);

internal sealed record FindRegionalMatchInput(
    FindPredicate Predicate,
    int PredicateIndex,
    FindLayerFacts Layer,
    MarkdownDocumentFacts Document,
    FindRegion Region,
    MarkdownTextSpan? SectionSpan);

internal sealed record FindPredicateMatchFacts
{
    internal FindPredicateMatchFacts(
        IEnumerable<FindEvidenceCandidate> evidence,
        IEnumerable<FindFinding> findings,
        bool unknown)
    {
        Evidence = Array.AsReadOnly(evidence.ToArray());
        Findings = Array.AsReadOnly(findings.ToArray());
        Unknown = unknown;
    }

    internal IReadOnlyList<FindEvidenceCandidate> Evidence { get; }

    internal IReadOnlyList<FindFinding> Findings { get; }

    internal bool Unknown { get; }
}

internal sealed record FindEvidenceCandidate(
    int Predicate,
    FindPredicateKind Kind,
    string Query,
    string Authored,
    FindRegion Region,
    SourceLayer Layer,
    SourceLocation Location,
    FindHeadingEvidence? Heading,
    int Occurrence)
{
    internal FindEvidence Create()
        => new(
            Predicate,
            Kind,
            Query,
            Authored,
            Region,
            Layer.Kind,
            Layer.CanonicalPath,
            Location,
            Occurrence,
            Heading);
}
