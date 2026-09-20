using OpenForge.Cli.Core.Commands.Repair;
using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairAuthoredReferenceBoundaryTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair skips a complete generated link and retains an authored link after Entries")]
    public void CompleteEntriesBlockSkipsGeneratedLinkAndRetainsAuthoredLinkAfterBlock()
    {
        const string source =
            "# Source\n\n"
            + "😀 before the boundary.\n\n"
            + "## Entries\n\n"
            + "- [Generated](generated-missing.md)\n\n"
            + "## Notes\n\n"
            + "[Authored](./guide.md)\n";
        var parser = new MarkdownDocumentParser();
        var document = parser.Parse(source);
        var generated = Assert.Single(document.Links, link => link.RawDestination == "generated-missing.md");
        var authored = Assert.Single(document.Links, link => link.RawDestination == "./guide.md");
        var map = new Utf8SourceMap(source);
        var entriesBlock = Assert.IsType<MarkdownEntriesBlock>(document.GeneratedRegion.EntriesBlock);
        var entriesLocation = map.Map(entriesBlock.Span.Start, entriesBlock.Span.Length);
        Assert.Equal(
            Encoding.UTF8.GetByteCount(source[..entriesBlock.Span.Start]),
            entriesLocation.ByteOffset);
        Assert.Equal(
            Encoding.UTF8.GetByteCount(source[entriesBlock.Span.Start..entriesBlock.Span.End]),
            entriesLocation.ByteLength);
        var generatedLocation = map.Map(generated.Span.Start, generated.Span.Length);
        Assert.True(generatedLocation.ByteOffset >= entriesLocation.ByteOffset);
        Assert.True(
            generatedLocation.ByteOffset + generatedLocation.ByteLength
                <= entriesLocation.ByteOffset + entriesLocation.ByteLength);
        var generatedReference = Reference(document, generated, SourceLinkTargetResolution.Missing);
        var authoredReference = Reference(document, authored, SourceLinkTargetResolution.Complete);

        var scope = RepairAuthoredReferenceBoundary.Filter(
            [generatedReference, authoredReference],
            new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal)
            {
                [".agents/docs/source.md"] = document,
            });

        var retained = Assert.Single(scope.References);
        Assert.Same(authoredReference, retained);
        Assert.Empty(scope.IncompleteSources);
        var authoredDestination = Assert.IsType<MarkdownTextSpan>(authored.DestinationSpan);
        Assert.Equal(
            map.Map(authored.Span.Start, authored.Span.Length),
            retained.Location);
        Assert.Equal(
            map.Map(authoredDestination.Start, authoredDestination.Length),
            retained.DestinationLocation);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair retains invalid and unavailable generated boundaries as incomplete evidence")]
    public void InvalidAndUnavailableEntriesBoundaryRetainsEvidenceAndSignalsIncomplete()
    {
        const string invalidSource =
            "# Source\n\n"
            + "## Entries\n\n"
            + "- [Generated](generated-missing.md)\n\n"
            + "## Entries\n";
        const string unavailableSource =
            "---\n"
            + "open-forge:\n"
            + "  tags: [Docs]\n"
            + "# Missing terminator\n";
        var parser = new MarkdownDocumentParser();
        var invalidDocument = parser.Parse(invalidSource);
        var unavailableDocument = parser.Parse(unavailableSource);
        Assert.Equal(MarkdownGeneratedRegionState.Invalid, invalidDocument.GeneratedRegion.State);
        Assert.Equal(MarkdownGeneratedRegionState.Unavailable, unavailableDocument.GeneratedRegion.State);
        var invalidReference = Reference(
            invalidDocument,
            Assert.Single(invalidDocument.Links),
            SourceLinkTargetResolution.Missing,
            ".agents/docs/invalid.md");
        var unavailableReferenceDocument = parser.Parse("# Source\n\n[Generated](generated-missing.md)\n");
        var unavailableReference = Reference(
            unavailableReferenceDocument,
            unavailableReferenceDocument.Links.Single(),
            SourceLinkTargetResolution.Missing,
            ".agents/docs/unavailable.md");

        var invalidScope = RepairAuthoredReferenceBoundary.Filter(
            [invalidReference],
            new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal)
            {
                [invalidReference.SourcePath] = invalidDocument,
            });
        var unavailableScope = RepairAuthoredReferenceBoundary.Filter(
            [unavailableReference],
            new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal)
            {
                [unavailableReference.SourcePath] = unavailableDocument,
            });

        Assert.Same(invalidReference, Assert.Single(invalidScope.References));
        Assert.Contains(invalidReference.SourcePath, invalidScope.IncompleteSources);
        Assert.Same(unavailableReference, Assert.Single(unavailableScope.References));
        Assert.Contains(unavailableReference.SourcePath, unavailableScope.IncompleteSources);
        Assert.All(
            RepairAuthoredReferenceBoundary.IncompleteFindings(
                invalidScope.IncompleteSources.Concat(unavailableScope.IncompleteSources)),
            finding => Assert.Equal(RepairFindingCode.DiagnosisIncomplete, finding.Code));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair remaining findings consume the same authored scope as proposals")]
    public void RemainingFindingsUseTheSameScopedSequenceAsProposals()
    {
        const string source =
            "# Source\n\n"
            + "## Entries\n\n"
            + "- [Generated](generated-missing.md)\n\n"
            + "## Notes\n\n"
            + "[Authored](./guide.md)\n";
        var document = new MarkdownDocumentParser().Parse(source);
        var generated = Assert.Single(document.Links, link => link.RawDestination == "generated-missing.md");
        var authored = Assert.Single(document.Links, link => link.RawDestination == "./guide.md");
        var generatedReference = Reference(document, generated, SourceLinkTargetResolution.Missing);
        var authoredReference = Reference(document, authored, SourceLinkTargetResolution.Complete);
        authoredReference = authoredReference with
        {
            Canonicalizations = [new LocalReferenceCanonicalization(
                LocalReferenceCanonicalizationKind.Path,
                "./guide.md",
                "guide.md",
                authoredReference.DestinationLocation!)],
        };

        var scope = RepairAuthoredReferenceBoundary.Filter(
            [generatedReference, authoredReference],
            new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal)
            {
                [generatedReference.SourcePath] = document,
            });
        var findings = RepairRemainingFindingReader.Read(scope.References).ToArray();

        var finding = Assert.Single(findings);
        Assert.Equal(RepairFindingCode.ManualFindingRemaining, finding.Code);
        Assert.Equal(authoredReference.SourcePath, finding.SourceCanonicalPath);
        Assert.Equal(authoredReference.DestinationLocation, finding.Occurrence);
        Assert.DoesNotContain(findings, value => value.Occurrence == generatedReference.DestinationLocation);
    }

    private static LocalReferenceObservation Reference(
        MarkdownDocumentFacts document,
        MarkdownLinkFact link,
        SourceLinkTargetResolution resolution,
        string sourcePath = ".agents/docs/source.md")
    {
        var map = new Utf8SourceMap(document.Source);
        return new LocalReferenceObservation
        {
            SourcePath = sourcePath,
            RoutePath = sourcePath,
            Kind = link.IsImage ? LocalReferenceKind.Image : LocalReferenceKind.Link,
            Destination = link.RawDestination,
            Label = link.Label,
            Location = map.Map(link.Span.Start, link.Span.Length),
            DestinationLocation = link.DestinationSpan is { } destination
                ? map.Map(destination.Start, destination.Length)
                : null,
            Facts = new SourceLinkDestinationFacts
            {
                Fragment = null,
                Target = new SourceLinkTarget
                {
                    Kind = SourceLinkTargetKind.Local,
                    Id = null,
                    Path = link.RawDestination,
                    PhysicalPath = null,
                    Layer = null,
                    Resolution = resolution,
                    Network = null,
                },
                Finding = null,
            },
            Fragment = LocalReferenceFragmentObservation.NotRequested(),
            Canonicalizations = [],
        };
    }
}
