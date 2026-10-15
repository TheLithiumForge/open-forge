using System.Collections;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational;

public sealed class LocalReferenceCandidateWorkTests
{
    [Fact(DisplayName = "Candidate evidence scans each source once for many missing occurrences and retains every evidence basis"),
        Trait("Feature", "references"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void CandidateSourcesAreIndexedOnce()
    {
        const string text = "# Guide\n\n[Guide](other.md)\n![Guide](image.png)\n";
        var document = new MarkdownDocumentParser().Parse(text);
        var source = new LocalReferenceSourceObservation(".agents/guide.md", "guide",
            [new(".agents/guide.md", document, new Utf8SourceMap(text))]);
        var sources = new CountedSources([source]);
        var references = Enumerable.Range(0, 20).Select(index => new LocalReferenceObservation
        {
            SourcePath = $".agents/source-{index}.md",
            RoutePath = $".agents/source-{index}.md",
            Kind = LocalReferenceKind.Link,
            Destination = "guide.md",
            Label = MarkdownLinkLabelFact.Supported("Guide"),
            Location = null,
            DestinationLocation = null,
            Facts = new()
            {
                Fragment = null,
                Target = new()
                {
                    Kind = SourceLinkTargetKind.Local,
                    Id = null,
                    Path = ".agents/guide.md",
                    PhysicalPath = null,
                    Layer = null,
                    Resolution = SourceLinkTargetResolution.Missing,
                    Network = null,
                },
                Finding = null,
            },
            Fragment = LocalReferenceFragmentObservation.NotRequested(),
            Canonicalizations = [],
        }).ToArray();
        var candidates = new LocalReferenceCandidateReader().Read(references, sources, new SourceRouteTopology([], []));

        Assert.Equal(1, sources.Visits);
        Assert.Equal(20, candidates.Count);
        Assert.All(candidates, candidate =>
        {
            Assert.Equal(".agents/guide.md", candidate.Candidate.Path);
            Assert.Equal([LocalReferenceCandidateBasisKind.Filename, LocalReferenceCandidateBasisKind.Title,
                LocalReferenceCandidateBasisKind.LiteralContent, LocalReferenceCandidateBasisKind.LiteralContent,
                LocalReferenceCandidateBasisKind.LiteralContent], candidate.Bases.Select(basis => basis.Kind));
        });
    }

    private sealed class CountedSources(IReadOnlyList<LocalReferenceSourceObservation> sources) : IReadOnlyList<LocalReferenceSourceObservation>
    {
        internal int Visits { get; private set; }
        public int Count => sources.Count;
        public LocalReferenceSourceObservation this[int index] => sources[index];
        public IEnumerator<LocalReferenceSourceObservation> GetEnumerator()
        {
            foreach (var source in sources) { Visits++; yield return source; }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
