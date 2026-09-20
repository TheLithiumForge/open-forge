using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Shared.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.UnitTests.Commands.Context.Shared.Selection;

public sealed class ContextSelectionAccumulatorTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Context selection retains the first source and distinct reasons in encounter order"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void RetainsFirstSourceAndDistinctReasonsInEncounterOrder()
    {
        var first = Source("A.md", "first");
        var duplicate = Source("A.md", "second");
        var other = Source("B.md", "other");
        var firstReason = Reason("first");
        var equalReason = Reason("first");
        var secondReason = Reason("second");
        var otherReason = Reason("other");
        var selection = new ContextSelectionAccumulator();

        Assert.True(selection.Add(first, firstReason));
        Assert.False(selection.Add(duplicate, equalReason));
        Assert.False(selection.Add(duplicate, secondReason));
        Assert.True(selection.Add(other, otherReason));

        var sources = selection.Sources;
        Assert.Equal(["A.md", "B.md"], sources.Select(row => row.Source.CanonicalPath));
        Assert.Same(first, sources[0].Source);
        Assert.Same(other, sources[1].Source);
        Assert.Equal(["first", "second"], sources[0].InclusionReasons.Select(reason => reason.Reference));
        Assert.Same(firstReason, sources[0].InclusionReasons[0]);
        Assert.Same(secondReason, sources[0].InclusionReasons[1]);
        Assert.Same(otherReason, Assert.Single(sources[1].InclusionReasons));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Context snapshots and cloned selections keep independent reason collections"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void SnapshotsAndCloneKeepIndependentReasonCollections()
    {
        var first = Source("A.md", "first");
        var other = Source("B.md", "other");
        var selection = new ContextSelectionAccumulator();
        selection.Add(first, Reason("initial"));
        var snapshot = selection.Sources;
        var repeatedSnapshot = selection.Sources;
        var clone = selection.Clone();

        selection.Add(first, Reason("original"));
        clone.Add(first, Reason("clone"));
        clone.Add(other, Reason("other"));

        Assert.NotSame(snapshot, repeatedSnapshot);
        Assert.NotSame(snapshot[0].InclusionReasons, repeatedSnapshot[0].InclusionReasons);
        Assert.Equal(["initial"], Assert.Single(snapshot).InclusionReasons.Select(reason => reason.Reference));
        Assert.Equal(["initial"], Assert.Single(repeatedSnapshot).InclusionReasons.Select(reason => reason.Reference));
        Assert.Equal(["initial", "original"], Assert.Single(selection.Sources).InclusionReasons.Select(reason => reason.Reference));
        var clonedSources = clone.Sources;
        Assert.Equal(["A.md", "B.md"], clonedSources.Select(row => row.Source.CanonicalPath));
        Assert.Equal(["initial", "clone"], clonedSources[0].InclusionReasons.Select(reason => reason.Reference));
        Assert.Equal(["other"], clonedSources[1].InclusionReasons.Select(reason => reason.Reference));
        Assert.Same(first, clonedSources[0].Source);
        Assert.Same(other, clonedSources[1].Source);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Context selection compares canonical paths ordinally"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void CanonicalPathComparisonIsOrdinal()
    {
        var selection = new ContextSelectionAccumulator();

        Assert.True(selection.Add(Source("A.md", "upper"), Reason("upper")));
        Assert.True(selection.Add(Source("a.md", "lower"), Reason("lower")));

        Assert.Equal(["A.md", "a.md"], selection.Sources.Select(row => row.Source.CanonicalPath));
    }

    private static ContextInclusionReason Reason(string reference)
        => new(kind: ContextInclusionReasonKind.SelectedSource, source: null, reference: reference, depth: null, location: null);

    private static ContextGraphSource Source(string canonicalPath, string text)
        => new(
            logicalSource: null,
            id: null,
            canonicalPath: canonicalPath,
            form: SourceDocumentForm.Markdown,
            routeState: SourceRouteState.Unrouted,
            route: null,
            metadata: SourceAuthoredMetadataFacts.Complete("A source", []),
            generatedEntries: SourceGeneratedEntriesFacts.Absent,
            layers:
            [
                new ContextGraphLayer
                {
                    Kind = SourceLayerKind.Base,
                    CanonicalPath = canonicalPath,
                    PhysicalPath = PhysicalPath(canonicalPath),
                    ReadState = FileReadState.Complete,
                    Text = text,
                    Document = new MarkdownDocumentParser().Parse(text),
                },
            ]);

    private static string PhysicalPath(string canonicalPath)
        => Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "open-forge-context-accumulator",
            canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
}
